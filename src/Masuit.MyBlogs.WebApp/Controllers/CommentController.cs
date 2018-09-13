using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Common;
using Hangfire;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Html;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Models.ViewModel;
using static Common.CommonHelper;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class CommentController : BaseController
    {
        private ICommentBll CommentBll { get; }
        private IPostBll PostBll { get; }
        private IInternalMessageBll MessageBll { get; }

        public CommentController(ICommentBll commentBll, IPostBll postBll, IInternalMessageBll messageBll)
        {
            CommentBll = commentBll;
            PostBll = postBll;
            MessageBll = messageBll;
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Put(CommentInputDto comment)
        {
            Post post = PostBll.GetById(comment.PostId);
            if (post is null)
            {
                return ResultData(null, false, "评论失败，文章不存在！");
            }
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            comment.Content = comment.Content.Trim().Replace("<p><br></p>", string.Empty);

            if (Regex.Match(comment.Content, ModRegex).Length <= 0)
            {
                comment.Status = Status.Pended;
            }
            if (user != null)
            {
                comment.NickName = user.NickName;
                comment.QQorWechat = user.QQorWechat;
                comment.Email = user.Email;
                if (user.IsAdmin)
                {
                    comment.Status = Status.Pended;
                    comment.IsMaster = true;
                }
            }
            comment.Content = Regex.Replace(comment.Content.HtmlSantinizerStandard().ConvertImgSrcToRelativePath(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>");
            comment.CommentDate = DateTime.Now;
            comment.Browser = comment.Browser ?? Request.Browser.Type;
            comment.IP = Request.UserHostAddress;
            Comment com = CommentBll.AddEntitySaved(comment.Mapper<Comment>());
            if (com != null)
            {
                var emails = new List<string>();
                var email = GetSettings("ReceiveEmail"); //站长邮箱
                emails.Add(email);
                string content = System.IO.File.ReadAllText(Request.MapPath("/template/notify.html")).Replace("{{title}}", post.Title).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", com.NickName).Replace("{{content}}", com.Content);
                if (comment.Status == Status.Pended)
                {
                    if (!com.IsMaster)
                    {
                        MessageBll.AddEntitySaved(new InternalMessage()
                        {
                            Title = $"来自【{com.NickName}】的新文章评论",
                            Content = com.Content,
                            Link = Url.Action("Details", "Post", new
                            {
                                id = com.PostId,
                                cid = com.Id
                            }, Request.Url.Scheme) + "#comment"
                        });
                    }
#if !DEBUG
                    if (com.ParentId == 0)
                    {
                        emails.Add(PostBll.GetById(com.PostId).Email);
                        //新评论，只通知博主和楼主
                        BackgroundJob.Enqueue(() => SendMail(Request.Url.Authority + "|博客文章新评论：", content.Replace("{{link}}", Url.Action("Details", "Post", new
                        {
                            id = com.PostId,
                            cid = com.Id
                        }, Request.Url.Scheme) + "#comment"), string.Join(",", emails.Distinct())));
                    }
                    else
                    {
                        //通知博主和上层所有关联的评论访客
                        var pid = CommentBll.GetParentCommentIdByChildId(com.Id);
                        emails = CommentBll.GetSelfAndAllChildrenCommentsByParentId(pid).Select(c => c.Email).Except(new List<string>()
                        {
                            com.Email
                        }).Distinct().ToList();
                        string link = Url.Action("Details", "Post", new
                        {
                            id = com.PostId,
                            cid = com.Id
                        }, Request.Url.Scheme) + "#comment";
                        BackgroundJob.Enqueue(() => SendMail($"{Request.Url.Authority}{GetSettings("Title")}文章评论回复：", content.Replace("{{link}}", link), string.Join(",", emails)));
                    }
#endif
                    return ResultData(null, true, "评论发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将显示到评论列表中");
                }
                BackgroundJob.Enqueue(() => SendMail(Request.Url.Authority + "|博客文章新评论(待审核)：", content.Replace("{{link}}", Url.Action("Details", "Post", new
                {
                    id = com.PostId,
                    cid = com.Id
                }, Request.Url.Scheme) + "#comment") + "<p style='color:red;'>(待审核)</p>", string.Join(",", emails)));
                return ResultData(null, true, "评论成功，待站长审核通过以后将显示");
            }
            return ResultData(null, false, "评论失败");
        }

        [HttpPost]
        public ActionResult CommentVote(int id)
        {
            Comment cm = CommentBll.GetFirstEntity(c => c.Id == id && c.Status == Status.Pended);
            if (Session["cm" + id] != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }
            if (cm != null)
            {
                cm.VoteCount++;
                CommentBll.UpdateEntity(cm);
                Session["cm" + id] = id;
                bool b = CommentBll.SaveChanges() > 0;
                return ResultData(null, b, b ? "投票成功" : "投票失败");
            }
            return ResultData(null, false, "非法操作");
        }

        [HttpPost]
        public ActionResult GetComments(int? id, int page = 1, int size = 5, int cid = 0)
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            int total; //总条数，用于前台分页
            if (cid != 0)
            {
                int pid = CommentBll.GetParentCommentIdByChildId(cid);
                List<Comment> single = CommentBll.GetSelfAndAllChildrenCommentsByParentId(pid).ToList();
                if (single.Any())
                {
                    total = 1;
                    return ResultData(new
                    {
                        total,
                        parentTotal = total,
                        page,
                        size,
                        rows = single.Mapper<IList<CommentViewModel>>()
                    });
                }
            }
            IList<Comment> parent = CommentBll.LoadPageEntities(page, size, out total, c => c.PostId == id && c.ParentId == 0 && (c.Status == Status.Pended || user.IsAdmin), c => c.CommentDate, false).ToList();
            if (!parent.Any())
            {
                return ResultData(null, false, "没有评论");
            }
            var list = new List<Comment>();
            parent.ForEach(c => CommentBll.GetSelfAndAllChildrenCommentsByParentId(c.Id).ForEach(result => list.Add(result)));
            var qlist = list.Where(c => (c.Status == Status.Pended || user.IsAdmin));
            if (total > 0)
            {
                return ResultData(new
                {
                    total,
                    parentTotal = total,
                    page,
                    size,
                    rows = qlist.Mapper<IList<CommentViewModel>>()
                });
            }
            return ResultData(null, false, "没有评论");
        }

        [HttpPost]
        public ActionResult GetPageComments(int page = 1, int size = 5, int cid = 0)
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            int total; //总条数，用于前台分页
            if (cid != 0)
            {
                int pid = CommentBll.GetParentCommentIdByChildId(cid);
                List<Comment> single = CommentBll.GetSelfAndAllChildrenCommentsByParentId(pid).ToList();
                if (single.Any())
                {
                    total = 1;
                    return ResultData(new
                    {
                        total,
                        parentTotal = total,
                        page,
                        size,
                        rows = single.Mapper<IList<CommentViewModel>>()
                    });
                }
            }
            IList<Comment> parent = CommentBll.LoadPageEntities(page, size, out total, c => c.ParentId == 0 && (c.Status == Status.Pended || user.IsAdmin), c => c.CommentDate, false).ToList();
            if (!parent.Any())
            {
                return ResultData(null, false, "没有评论");
            }
            var list = new List<Comment>();
            parent.ForEach(c => CommentBll.GetSelfAndAllChildrenCommentsByParentId(c.Id).ForEach(result => list.Add(result)));
            var qlist = list.Where(c => (c.Status == Status.Pended || user.IsAdmin));
            if (total > 0)
            {
                return ResultData(new
                {
                    total,
                    parentTotal = total,
                    page,
                    size,
                    rows = qlist.Mapper<IList<CommentViewModel>>()
                });
            }
            return ResultData(null, false, "没有评论");
        }

        [Authority]
        public ActionResult Pass(int id)
        {
            Comment comment = CommentBll.GetById(id);
            comment.Status = Status.Pended;
            bool b = CommentBll.UpdateEntitySaved(comment);
            var pid = comment.ParentId == 0 ? comment.Id : CommentBll.GetParentCommentIdByChildId(id);
#if !DEBUG
            string content = System.IO.File.ReadAllText(Request.MapPath("/template/notify.html")).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", comment.NickName).Replace("{{content}}", comment.Content);
            var emails = CommentBll.GetSelfAndAllChildrenCommentsByParentId(pid).Select(c => c.Email).Distinct().Except(new List<string>()
            {
                comment.Email
            }).ToList();
            string link = Url.Action("Details", "Post", new
            {
                id = comment.PostId,
                cid = pid
            }, Request.Url.Scheme) + "#comment";
            BackgroundJob.Enqueue(() => SendMail($"{Request.Url.Authority}{GetSettings("Title")}文章评论回复：", content.Replace("{{link}}", link), string.Join(",", emails)));
#endif
            return ResultData(null, b, b ? "审核通过！" : "审核失败！");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            var b = CommentBll.DeleteEntitiesSaved(CommentBll.GetSelfAndAllChildrenCommentsByParentId(id).ToList());
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取未审核的评论
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPendingComments(int page = 1, int size = 10)
        {
            List<CommentOutputDto> list = CommentBll.LoadPageEntities<DateTime, CommentOutputDto>(page, size, out int total, c => c.Status == Status.Pending, c => c.CommentDate, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }
    }
}