using Common;
using Hangfire;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 评论管理
    /// </summary>
    public class CommentController : BaseController
    {
        private ICommentService CommentService { get; }
        private IPostService PostService { get; }
        private IInternalMessageService MessageService { get; }
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// 评论管理
        /// </summary>
        /// <param name="commentService"></param>
        /// <param name="postService"></param>
        /// <param name="messageService"></param>
        /// <param name="hostingEnvironment"></param>
        public CommentController(ICommentService commentService, IPostService postService, IInternalMessageService messageService, IHostingEnvironment hostingEnvironment)
        {
            CommentService = commentService;
            PostService = postService;
            MessageService = messageService;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 发表评论
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Put(CommentInputDto comment)
        {
            if (Regex.Match(comment.Content, CommonHelper.BanRegex).Length > 0)
            {
                return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请注意改善您的言辞！");
            }

            Post post = PostService.GetById(comment.PostId);
            if (post is null)
            {
                return ResultData(null, false, "评论失败，文章不存在！");
            }
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
            comment.Content = comment.Content.Trim().Replace("<p><br></p>", string.Empty);

            if (comment.Content.RemoveHtml().Trim().Equals(HttpContext.Session.Get<string>("comment" + comment.PostId)))
            {
                return ResultData(null, false, "您刚才已经在这篇文章发表过一次评论了，换一篇文章吧，或者换一下评论内容吧！");
            }

            if (Regex.Match(comment.Content, CommonHelper.ModRegex).Length <= 0)
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
            comment.Browser = comment.Browser ?? Request.Headers[HeaderNames.UserAgent];
            comment.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            Comment com = CommentService.AddEntitySaved(comment.Mapper<Comment>());
            if (com != null)
            {
                HttpContext.Session.Set("comment" + comment.PostId, comment.Content.RemoveHtml().Trim());
                var emails = new List<string>();
                var email = CommonHelper.SystemSettings["ReceiveEmail"]; //站长邮箱
                emails.Add(email);
                string content = System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/notify.html").Replace("{{title}}", post.Title).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", com.NickName).Replace("{{content}}", com.Content);
                if (comment.Status == Status.Pended)
                {
                    if (!com.IsMaster)
                    {
                        MessageService.AddEntitySaved(new InternalMessage()
                        {
                            Title = $"来自【{com.NickName}】的新文章评论",
                            Content = com.Content,
                            Link = Url.Action("Details", "Post", new { id = com.PostId, cid = com.Id }, Request.Scheme) + "#comment"
                        });
                    }
#if !DEBUG
                    if (com.ParentId == 0)
                    {
                        emails.Add(PostService.GetById(com.PostId).Email);
                        //新评论，只通知博主和楼主
                        foreach (var s in emails.Distinct())
                        {
                            BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Domain"] + "|博客文章新评论：", content.Replace("{{link}}", Url.Action("Details", "Post", new { id = com.PostId, cid = com.Id }, Request.Scheme) + "#comment"), s));
                        }
                    }
                    else
                    {
                        //通知博主和上层所有关联的评论访客
                        var pid = CommentService.GetParentCommentIdByChildId(com.Id);
                        emails = CommentService.GetSelfAndAllChildrenCommentsByParentId(pid).Select(c => c.Email).Except(new List<string> { com.Email }).Distinct().ToList();
                        string link = Url.Action("Details", "Post", new { id = com.PostId, cid = com.Id }, Request.Scheme) + "#comment";
                        foreach (var s in emails)
                        {
                            BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{CommonHelper.SystemSettings["Domain"]}{CommonHelper.SystemSettings["Title"]}文章评论回复：", content.Replace("{{link}}", link), s));
                        }
                    }
#endif
                    return ResultData(null, true, "评论发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将显示到评论列表中");
                }
                foreach (var s in emails.Distinct())
                {
                    BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Domain"] + "|博客文章新评论(待审核)：", content.Replace("{{link}}", Url.Action("Details", "Post", new { id = com.PostId, cid = com.Id }, Request.Scheme) + "#comment") + "<p style='color:red;'>(待审核)</p>", s));
                }
                return ResultData(null, true, "评论成功，待站长审核通过以后将显示");
            }
            return ResultData(null, false, "评论失败");
        }

        /// <summary>
        /// 评论投票
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CommentVote(int id)
        {
            Comment cm = CommentService.GetFirstEntity(c => c.Id == id && c.Status == Status.Pended);
            if (HttpContext.Session.Get("cm" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }
            if (cm != null)
            {
                cm.VoteCount++;
                CommentService.UpdateEntity(cm);
                HttpContext.Session.Set("cm" + id, id.GetBytes());
                bool b = CommentService.SaveChanges() > 0;
                return ResultData(null, b, b ? "投票成功" : "投票失败");
            }
            return ResultData(null, false, "非法操作");
        }

        /// <summary>
        /// 获取评论
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetComments(int? id, int page = 1, int size = 5, int cid = 0)
        {
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            int total; //总条数，用于前台分页
            if (cid != 0)
            {
                int pid = CommentService.GetParentCommentIdByChildId(cid);
                List<Comment> single = CommentService.GetSelfAndAllChildrenCommentsByParentId(pid).ToList();
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
            IList<Comment> parent = CommentService.LoadPageEntities(page, size, out total, c => c.PostId == id && c.ParentId == 0 && (c.Status == Status.Pended || user.IsAdmin), c => c.CommentDate, false).ToList();
            if (!parent.Any())
            {
                return ResultData(null, false, "没有评论");
            }
            var list = new List<Comment>();
            parent.ForEach(c => CommentService.GetSelfAndAllChildrenCommentsByParentId(c.Id).ForEach(result => list.Add(result)));
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

        /// <summary>
        /// 分页获取评论
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetPageComments(int page = 1, int size = 5, int cid = 0)
        {
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            int total; //总条数，用于前台分页
            if (cid != 0)
            {
                int pid = CommentService.GetParentCommentIdByChildId(cid);
                List<Comment> single = CommentService.GetSelfAndAllChildrenCommentsByParentId(pid).ToList();
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
            IList<Comment> parent = CommentService.LoadPageEntities(page, size, out total, c => c.ParentId == 0 && (c.Status == Status.Pended || user.IsAdmin), c => c.CommentDate, false).ToList();
            if (!parent.Any())
            {
                return ResultData(null, false, "没有评论");
            }
            var list = new List<Comment>();
            parent.ForEach(c => CommentService.GetSelfAndAllChildrenCommentsByParentId(c.Id).ForEach(result => list.Add(result)));
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

        /// <summary>
        /// 审核评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Pass(int id)
        {
            Comment comment = CommentService.GetById(id);
            comment.Status = Status.Pended;
            Post post = PostService.GetById(comment.PostId);
            bool b = CommentService.UpdateEntitySaved(comment);
            var pid = comment.ParentId == 0 ? comment.Id : CommentService.GetParentCommentIdByChildId(id);
#if !DEBUG
            string content = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "template", "notify.html")).Replace("{{title}}", post.Title).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", comment.NickName).Replace("{{content}}", comment.Content);
            var emails = CommentService.GetSelfAndAllChildrenCommentsByParentId(pid).Select(c => c.Email).Distinct().Except(new List<string> { comment.Email, CommonHelper.SystemSettings["ReceiveEmail"] }).ToList();
            string link = Url.Action("Details", "Post", new
            {
                id = comment.PostId,
                cid = pid
            }, Request.Scheme) + "#comment";
            foreach (var email in emails)
            {
                BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]}文章评论回复：", content.Replace("{{link}}", link), email));
            }
#endif
            return ResultData(null, b, b ? "审核通过！" : "审核失败！");
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete(int id)
        {
            var b = CommentService.DeleteEntitiesSaved(CommentService.GetSelfAndAllChildrenCommentsByParentId(id).ToList());
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取未审核的评论
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPendingComments(int page = 1, int size = 10)
        {
            List<CommentOutputDto> list = CommentService.LoadPageEntities<DateTime, CommentOutputDto>(page, size, out int total, c => c.Status == Status.Pending, c => c.CommentDate, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }
    }
}