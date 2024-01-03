using Dispose.Scope;
using Hangfire;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.Html;
using Masuit.Tools.Logging;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 评论管理
/// </summary>
public sealed class CommentController : BaseController
{
    public ICommentService CommentService { get; set; }

    public IPostService PostService { get; set; }

    public IWebHostEnvironment HostEnvironment { get; set; }

    /// <summary>
    /// 发表评论
    /// </summary>
    /// <param name="messageService"></param>
    /// <param name="blocklistService"></param>
    /// <param name="cmd"></param>
    /// <returns></returns>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<ActionResult> Submit([FromServices] IInternalMessageService messageService, [FromServices] IEmailBlocklistService blocklistService, CommentCommand cmd)
    {
        var match = Regex.Match(cmd.NickName + cmd.Content.RemoveHtmlTag(), CommonHelper.BanRegex);
        if (match.Success)
        {
            LogManager.Info($"提交内容：{cmd.NickName}/{cmd.Content}，敏感词：{match.Value}");
            return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请检查您的内容后尝试重新提交！");
        }

        var error = await ValidateEmailCode(blocklistService, cmd.Email, cmd.Code);
        if (!string.IsNullOrEmpty(error))
        {
            return ResultData(null, false, error);
        }

        if (cmd.ParentId > 0 && DateTime.Now - CommentService[cmd.ParentId.Value, c => c.CommentDate] > TimeSpan.FromDays(180))
        {
            return ResultData(null, false, "当前评论过于久远，不再允许回复！");
        }

        var post = await PostService.GetByIdAsync(cmd.PostId) ?? throw new NotFoundException("评论失败，文章未找到");
        CheckPermission(post);
        if (post.DisableComment)
        {
            return ResultData(null, false, "本文已禁用评论功能，不允许任何人回复！");
        }

        cmd.Content = cmd.Content.Trim().Replace("<p><br></p>", string.Empty);
        var ip = ClientIP.ToString();

        if (!CurrentUser.IsAdmin)
        {
            if (await RedisHelper.SAddAsync("Comments:" + ip, cmd.Content) == 0)
            {
                await RedisHelper.ExpireAsync("Comments:" + ip, TimeSpan.FromMinutes(2));
                return ResultData(null, false, "您已发表了相同的评论内容，请稍后再发表吧！");
            }

            if (await RedisHelper.SCardAsync("Comments:" + ip) > 2)
            {
                await RedisHelper.ExpireAsync("Comments:" + ip, TimeSpan.FromMinutes(2));
                return ResultData(null, false, "您的发言频率过快，请稍后再发表吧！");
            }
        }

        var comment = Mapper.Map<Comment>(cmd);
        if (cmd.ParentId > 0)
        {
            comment.GroupTag = CommentService.GetQuery(c => c.Id == cmd.ParentId).Select(c => c.GroupTag).FirstOrDefault();
            comment.Path = (CommentService.GetQuery(c => c.Id == cmd.ParentId).Select(c => c.Path).FirstOrDefault() + "," + cmd.ParentId).Trim(',');
        }
        else
        {
            comment.GroupTag = SnowFlake.NewId;
            comment.Path = SnowFlake.NewId;
        }

        if (cmd.Email == post.Email || cmd.Email == post.ModifierEmail || Regex.Match(cmd.NickName + cmd.Content, CommonHelper.ModRegex).Length <= 0)
        {
            comment.Status = Status.Published;
        }

        comment.CommentDate = DateTime.Now;
        var user = HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
        if (user != null)
        {
            comment.NickName = user.NickName;
            comment.Email = user.Email;
            if (user.IsAdmin)
            {
                comment.Status = Status.Published;
                comment.IsMaster = true;
            }
        }
        comment.Content = await cmd.Content.HtmlSanitizerStandard().ClearImgAttributes();
        comment.Browser = cmd.Browser ?? Request.Headers[HeaderNames.UserAgent];
        comment.IP = ip;
        comment.Location = Request.Location();
        comment = CommentService.AddEntitySaved(comment);
        if (comment == null)
        {
            return ResultData(null, false, "评论失败");
        }

        Response.Cookies.Append("NickName", comment.NickName, new CookieOptions()
        {
            Expires = DateTimeOffset.Now.AddYears(1),
            SameSite = SameSiteMode.Lax
        });
        WriteEmailKeyCookie(cmd.Email);
        await RedisHelper.ExpireAsync("Comments:" + comment.IP, TimeSpan.FromMinutes(1));
        var emails = new HashSet<string>();
        var email = CommonHelper.SystemSettings["ReceiveEmail"]; //站长邮箱
        emails.Add(email);
        var content = new Template(await new FileInfo(HostEnvironment.WebRootPath + "/template/notify.html").ShareReadWrite().ReadAllTextAsync(Encoding.UTF8))
            .Set("title", post.Title)
            .Set("time", DateTime.Now.ToTimeZoneF(HttpContext.Session.Get<string>(SessionKey.TimeZone)))
            .Set("nickname", comment.NickName)
            .Set("content", comment.Content);
        Response.Cookies.Append("Comment_" + post.Id, "1", new CookieOptions()
        {
            Expires = DateTimeOffset.Now.AddDays(2),
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromDays(2),
            Secure = true
        });
        if (comment.Status == Status.Published)
        {
            if (!comment.IsMaster)
            {
                await messageService.AddEntitySavedAsync(new InternalMessage()
                {
                    Title = $"来自【{comment.NickName}】在文章《{post.Title}》的新评论",
                    Content = comment.Content,
                    Link = Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }) + "#comment"
                });
            }
            if (comment.ParentId == null)
            {
                emails.Add(post.Email);
                emails.Add(post.ModifierEmail);

                //新评论，只通知博主和楼主
                foreach (var s in emails)
                {
                    BackgroundJob.Enqueue<IMailSender>(sender => sender.Send(Request.Host + "|博客文章新评论：", content.Set("link", Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment").Render(false), s, comment.IP));
                }
            }
            else
            {
                //通知博主和所有关联的评论访客
                emails.AddRange(await CommentService.GetQuery(c => c.GroupTag == comment.GroupTag).Select(c => c.Email).Distinct().ToArrayAsync());
                emails.AddRange(post.Email, post.ModifierEmail);
                emails.Remove(comment.Email);
                string link = Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment";
                foreach (var s in emails)
                {
                    BackgroundJob.Enqueue<IMailSender>(sender => sender.Send($"{Request.Host}{CommonHelper.SystemSettings["Title"]}文章评论回复：", content.Set("link", link).Render(false), s, comment.IP));
                }
            }
            return ResultData(null, true, "评论发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将显示到评论列表中");
        }

        foreach (var s in emails)
        {
            BackgroundJob.Enqueue<IMailSender>(sender => sender.Send(Request.Host + "|博客文章新评论(待审核)：", content.Set("link", Url.Action("Details", "Post", new { id = comment.PostId, cid = comment.Id }, Request.Scheme) + "#comment").Render(false) + "<p style='color:red;'>(待审核)</p>", s, comment.IP));
        }

        return ResultData(null, true, "评论成功，待审核通过以后显示");
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
    public async Task<ActionResult> GetComments(int? id, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15, int? cid = null)
    {
        if (cid > 0)
        {
            var comment = await CommentService.GetByIdAsync(cid.Value) ?? throw new NotFoundException("评论未找到");
            var layer = CommentService.GetQueryNoTracking(c => c.GroupTag == comment.GroupTag).ToPooledListScope();
            foreach (var c in layer)
            {
                c.CommentDate = c.CommentDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                c.IsAuthor = c.Email == comment.Post.Email || c.Email == comment.Post.ModifierEmail;
                if (!CurrentUser.IsAdmin)
                {
                    c.Email = null;
                    c.IP = null;
                    c.Location = null;
                }
            }

            return ResultData(new
            {
                total = 1,
                parentTotal = 1,
                page,
                size,
                rows = Mapper.Map<IList<CommentViewModel>>(layer.ToTree(c => c.Id, c => c.ParentId))
            });
        }

        var parent = await CommentService.GetPagesAsync(page, size, c => c.PostId == id && c.ParentId == null && (c.Status == Status.Published || CurrentUser.IsAdmin), c => c.CommentDate, false);
        if (!parent.Data.Any())
        {
            return ResultData(null, false, "没有评论");
        }
        int total = parent.TotalCount; //总条数，用于前台分页
        var tags = parent.Data.Select(c => c.GroupTag).ToArray();
        var comments = CommentService.GetQuery(c => tags.Contains(c.GroupTag)).Include(c => c.Post).AsNoTracking().ToPooledListScope();
        comments.ForEach(c =>
        {
            c.CommentDate = c.CommentDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            c.IsAuthor = c.Email == c.Post.Email || c.Email == c.Post.ModifierEmail;
            if (!CurrentUser.IsAdmin)
            {
                c.Email = null;
                c.IP = null;
                c.Location = null;
            }
        });
        if (total > 0)
        {
            return ResultData(new
            {
                total,
                parentTotal = total,
                page,
                size,
                rows = Mapper.Map<IList<CommentViewModel>>(comments.OrderByDescending(c => c.CommentDate).ToTree(c => c.Id, c => c.ParentId))
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
        var comment = await CommentService.GetByIdAsync(id) ?? throw new NotFoundException("评论不存在！");
        comment.Status = Status.Published;
        Post post = await PostService.GetByIdAsync(comment.PostId);
        bool b = await CommentService.SaveChangesAsync() > 0;
        if (b)
        {
            var content = new Template(await new FileInfo(Path.Combine(HostEnvironment.WebRootPath, "template", "notify.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8))
                .Set("title", post.Title)
                .Set("time", DateTime.Now.ToTimeZoneF(HttpContext.Session.Get<string>(SessionKey.TimeZone)))
                .Set("nickname", comment.NickName)
                .Set("content", comment.Content);
            var emails = CommentService.GetQuery(c => c.GroupTag == comment.GroupTag).Select(c => c.Email).Distinct().AsEnumerable().Append(post.ModifierEmail).Except(new List<string> { comment.Email, CurrentUser.Email }).ToPooledSetScope();
            var link = Url.Action("Details", "Post", new
            {
                id = comment.PostId,
                cid = id
            }, Request.Scheme) + "#comment";
            foreach (var email in emails)
            {
                BackgroundJob.Enqueue<IMailSender>(sender => sender.Send($"{Request.Host}{CommonHelper.SystemSettings["Title"]}文章评论回复：", content.Set("link", link).Render(false), email, ClientIP.ToString()));
            }

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
        var b = CommentService.DeleteById(id);
        return ResultData(null, b, b ? "删除成功！" : "删除失败！");
    }

    /// <summary>
    /// 获取未审核的评论
    /// </summary>
    /// <returns></returns>
    [MyAuthorize]
    public async Task<ActionResult> GetPendingComments([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
    {
        var pages = await CommentService.GetPagesAsync<DateTime, CommentDto>(page, size, c => c.Status == Status.Pending, c => c.CommentDate, false);
        foreach (var item in pages.Data)
        {
            item.CommentDate = item.CommentDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
        }

        return Ok(pages);
    }
}
