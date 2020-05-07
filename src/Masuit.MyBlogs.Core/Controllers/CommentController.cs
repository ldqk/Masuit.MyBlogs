using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Command;
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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public async Task<ActionResult> Put(CommentCommand dto)
        {
            if (Regex.Match(dto.Content, CommonHelper.BanRegex).Length > 0)
            {
                return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请检查您的内容后尝试重新提交！");
            }

            Post post = await PostService.GetByIdAsync(dto.PostId) ?? throw new NotFoundException("评论失败，文章未找到");
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
                comment.Status = Status.Published;
            }

            comment.CommentDate = DateTime.Now;
            var user = HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
            if (user != null)
            {
                comment.NickName = user.NickName;
                comment.QQorWechat = user.QQorWechat;
                comment.Email = user.Email;
                if (user.IsAdmin)
                {
                    comment.Status = Status.Published;
                    comment.IsMaster = true;
                }
            }
            comment.Content = dto.Content.HtmlSantinizerStandard().ClearImgAttributes();
            comment.Browser = dto.Browser ?? Request.Headers[HeaderNames.UserAgent];
            comment.IP = ClientIP;
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
            var content = (await System.IO.File.ReadAllTextAsync(HostEnvironment.WebRootPath + "/template/notify.html"))
                .Replace("{{title}}", post.Title)
                .Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{{nickname}}", comment.NickName)
                .Replace("{{content}}", comment.Content);
            if (comment.Status == Status.Published)
            {
                if (!comment.IsMaster)
                {
                    await MessageService.AddEntitySavedAsync(new InternalMessage()
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
                        BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "|博客文章新评论：", content.Replace("{{link}}", Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment"), s));
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
                        BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]}文章评论回复：", content.Replace("{{link}}", link), s));
                    }
                }
#endif
                return ResultData(null, true, "评论发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将显示到评论列表中");
            }

            foreach (var s in emails)
            {
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "|博客文章新评论(待审核)：", content.Replace("{{link}}", Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment") + "<p style='color:red;'>(待审核)</p>", s));
            }

            return ResultData(null, true, "评论成功，待站长审核通过以后将显示");
        }

        /// <summary>
        /// 评论投票
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CommentVote(int id)
        {
            if (HttpContext.Session.Get("cm" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            var cm = await CommentService.GetAsync(c => c.Id == id && c.Status == Status.Published) ?? throw new NotFoundException("评论不存在！");
            cm.VoteCount++;
            bool b = await CommentService.SaveChangesAsync() > 0;
            if (b)
            {
                HttpContext.Session.Set("cm" + id, id.GetBytes());
            }

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
        public ActionResult GetComments(int? id, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")]int size = 15, int cid = 0)
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
            var parent = CommentService.GetPagesNoTracking(page, size, c => c.PostId == id && c.ParentId == 0 && (c.Status == Status.Published || CurrentUser.IsAdmin), c => c.CommentDate, false);
            if (!parent.Data.Any())
            {
                return ResultData(null, false, "没有评论");
            }
            total = parent.TotalCount;
            var result = parent.Data.SelectMany(c => CommentService.GetSelfAndAllChildrenCommentsByParentId(c.Id).Where(x => (x.Status == Status.Published || CurrentUser.IsAdmin))).ToList();
            if (total > 0)
            {
                return ResultData(new
                {
                    total,
                    parentTotal = total,
                    page,
                    size,
                    rows = result.Mapper<IList<CommentViewModel>>()
                });
            }
            return ResultData(null, false, "没有评论");
        }

        /// <summary>
        /// 审核评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Pass(int id)
        {
            Comment comment = await CommentService.GetByIdAsync(id) ?? throw new NotFoundException("评论不存在！");
            comment.Status = Status.Published;
            Post post = await PostService.GetByIdAsync(comment.PostId);
            bool b = await CommentService.SaveChangesAsync() > 0;
            if (b)
            {
                var pid = comment.ParentId == 0 ? comment.Id : CommentService.GetParentCommentIdByChildId(id);
#if !DEBUG
                var content = (await System.IO.File.ReadAllTextAsync(Path.Combine(HostEnvironment.WebRootPath, "template", "notify.html")))
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
        [MyAuthorize]
        public ActionResult Delete(int id)
        {
            var b = CommentService.DeleteEntitiesSaved(CommentService.GetSelfAndAllChildrenCommentsByParentId(id).ToList());
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取未审核的评论
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult GetPendingComments([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")]int size = 15)
        {
            var pages = CommentService.GetPages<DateTime, CommentDto>(page, size, c => c.Status == Status.Pending, c => c.CommentDate, false);
            return Ok(pages);
        }
    }
}