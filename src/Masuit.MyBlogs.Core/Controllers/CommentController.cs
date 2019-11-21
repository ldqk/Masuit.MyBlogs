using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
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
        public ICommentService CommentService { get; set; }
        public IPostService PostService { get; set; }
        public IInternalMessageService MessageService { get; set; }
        public IWebHostEnvironment HostEnvironment { get; set; }

        /// <summary>
        /// 发表评论
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Put(CommentInputDto dto)
        {
            if (Regex.Match(dto.Content, CommonHelper.BanRegex).Length > 0)
            {
                return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请检查您的内容后尝试重新提交！");
            }

            Post post = PostService.GetById(dto.PostId);
            if (post is null)
            {
                return ResultData(null, false, "评论失败，文章不存在！");
            }

            if (post.DisableComment)
            {
                return ResultData(null, false, "本文已禁用评论功能，不允许任何人回复！");
            }

            dto.Content = dto.Content.Trim().Replace("<p><br></p>", string.Empty);
            if (dto.Content.RemoveHtmlTag().Trim().Equals(HttpContext.Session.Get<string>("comment" + dto.PostId)))
            {
                return ResultData(null, false, "您刚才已经在这篇文章发表过一次评论了，换一篇文章吧，或者换一下评论内容吧！");
            }

            var comment = dto.Mapper<Comment>();
            if (Regex.Match(dto.Content, CommonHelper.ModRegex).Length <= 0)
            {
                comment.Status = Status.Pended;
            }

            comment.CommentDate = DateTime.Now;
            var user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
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
            comment.Content = dto.Content.HtmlSantinizerStandard().ClearImgAttributes();
            comment.Browser = dto.Browser ?? Request.Headers[HeaderNames.UserAgent];
            comment.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            comment.Location = comment.IP.GetIPLocation().Split("|").Where(s => !int.TryParse(s, out _)).ToHashSet().Join("|");
            comment = CommentService.AddEntitySaved(comment);
            if (comment == null)
            {
                return ResultData(null, false, "评论失败");
            }

            HttpContext.Session.Set("comment" + comment.PostId, comment.Content.RemoveHtmlTag().Trim());
            var emails = new HashSet<string>();
            var email = CommonHelper.SystemSettings["ReceiveEmail"]; //站长邮箱
            emails.Add(email);
            var content = System.IO.File.ReadAllText(HostEnvironment.WebRootPath + "/template/notify.html")
                .Replace("{{title}}", post.Title)
                .Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{{nickname}}", comment.NickName)
                .Replace("{{content}}", comment.Content);
            if (comment.Status == Status.Pended)
            {
                if (!comment.IsMaster)
                {
                    MessageService.AddEntitySaved(new InternalMessage()
                    {
                        Title = $"来自【{comment.NickName}】的新文章评论",
                        Content = comment.Content,
                        Link = Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment"
                    });
                }
#if !DEBUG
                if (comment.ParentId == 0)
                {
                    emails.Add(post.Email);
                    emails.Add(post.ModifierEmail);
                    //新评论，只通知博主和楼主
                    foreach (var s in emails)
                    {
                        BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Domain"] + "|博客文章新评论：", content.Replace("{{link}}", Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment"), s));
                    }
                }
                else
                {
                    //通知博主和上层所有关联的评论访客
                    var pid = CommentService.GetParentCommentIdByChildId(comment.Id);
                    emails.AddRange(CommentService.GetSelfAndAllChildrenCommentsByParentId(pid).Select(c => c.Email).ToArray());
                    emails.AddRange(post.Email, post.ModifierEmail);
                    emails.Remove(comment.Email);
                    string link = Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment";
                    foreach (var s in emails)
                    {
                        BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{CommonHelper.SystemSettings["Domain"]}{CommonHelper.SystemSettings["Title"]}文章评论回复：", content.Replace("{{link}}", link), s));
                    }
                }
#endif
                return ResultData(null, true, "评论发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将显示到评论列表中");
            }

            foreach (var s in emails)
            {
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Domain"] + "|博客文章新评论(待审核)：", content.Replace("{{link}}", Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment") + "<p style='color:red;'>(待审核)</p>", s));
            }

            return ResultData(null, true, "评论成功，待站长审核通过以后将显示");
        }

        /// <summary>
        /// 评论投票
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CommentVote(int id)
        {
            Comment cm = CommentService.Get(c => c.Id == id && c.Status == Status.Pended);
            if (HttpContext.Session.Get("cm" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            if (cm == null)
            {
                return ResultData(null, false, "非法操作");
            }

            cm.VoteCount++;
            HttpContext.Session.Set("cm" + id, id.GetBytes());
            bool b = CommentService.SaveChanges() > 0;
            return ResultData(null, b, b ? "投票成功" : "投票失败");
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
            int total; //总条数，用于前台分页
            if (cid != 0)
            {
                int pid = CommentService.GetParentCommentIdByChildId(cid);
                var single = CommentService.GetSelfAndAllChildrenCommentsByParentId(pid).ToList();
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
            var parent = CommentService.GetPagesFromCache(page, size, out total, c => c.PostId == id && c.ParentId == 0 && (c.Status == Status.Pended || CurrentUser.IsAdmin), c => c.CommentDate, false).ToList();
            if (!parent.Any())
            {
                return ResultData(null, false, "没有评论");
            }
            parent = parent.SelectMany(c => CommentService.GetSelfAndAllChildrenCommentsByParentId(c.Id).Where(x => (x.Status == Status.Pended || CurrentUser.IsAdmin))).ToList();
            if (total > 0)
            {
                return ResultData(new
                {
                    total,
                    parentTotal = total,
                    page,
                    size,
                    rows = parent.Mapper<IList<CommentViewModel>>()
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
            bool b = CommentService.SaveChanges() > 0;
            if (b)
            {
                var pid = comment.ParentId == 0 ? comment.Id : CommentService.GetParentCommentIdByChildId(id);
#if !DEBUG
                var content = System.IO.File.ReadAllText(Path.Combine(HostEnvironment.WebRootPath, "template", "notify.html"))
                    .Replace("{{title}}", post.Title)
                    .Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    .Replace("{{nickname}}", comment.NickName)
                    .Replace("{{content}}", comment.Content);
                var emails = CommentService.GetSelfAndAllChildrenCommentsByParentId(pid).Select(c => c.Email).Append(post.ModifierEmail).Except(new List<string> { comment.Email, CurrentUser.Email }).ToHashSet();
                var link = Url.Action("Details", "Post", new
                {
                    id = comment.PostId,
                    cid = pid
                }, Request.Scheme) + "#comment";
                foreach (var email in emails)
                {
                    BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]}文章评论回复：", content.Replace("{{link}}", link), email));
                }
#endif
                return ResultData(null, true, "审核通过！");
            }

            return ResultData(null, false, "审核失败！");
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
            var list = CommentService.GetPages<DateTime, CommentOutputDto>(page, size, out int total, c => c.Status == Status.Pending, c => c.CommentDate, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }
    }
}