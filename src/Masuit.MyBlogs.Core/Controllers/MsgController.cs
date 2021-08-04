using CacheManager.Core;
using EFCoreSecondLevelCacheInterceptor;
using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Common.Mails;
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
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 留言板和站内信
    /// </summary>
    public class MsgController : BaseController
    {
        /// <summary>
        /// 留言
        /// </summary>
        public ILeaveMessageService LeaveMessageService { get; set; }

        /// <summary>
        /// 站内信
        /// </summary>
        public IInternalMessageService MessageService { get; set; }

        public IWebHostEnvironment HostEnvironment { get; set; }

        public ICacheManager<int> MsgFeq { get; set; }

        /// <summary>
        /// 留言板
        /// </summary>
        /// <returns></returns>
        [Route("msg"), Route("msg/{cid:int}"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Index()
        {
            ViewBag.TotalCount = LeaveMessageService.Count(m => m.ParentId == 0 && m.Status == Status.Published);
            var text = await System.IO.File.ReadAllTextAsync(Path.Combine(HostEnvironment.WebRootPath, "template", "agreement.html"));
            return CurrentUser.IsAdmin ? View("Index_Admin", text) : View(model: text);
        }

        /// <summary>
        /// 获取留言
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetMsgs([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15, int cid = 0)
        {
            if (cid != 0)
            {
                var message = await LeaveMessageService.GetByIdAsync(cid) ?? throw new NotFoundException("留言未找到");
                var single = new[] { message.Root() };
                foreach (var m in single.Flatten())
                {
                    m.PostDate = m.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                }

                return ResultData(new
                {
                    total = 1,
                    parentTotal = 1,
                    page,
                    size,
                    rows = single.Mapper<IList<LeaveMessageViewModel>>()
                });
            }

            var parent = await LeaveMessageService.GetPagesAsync(page, size, m => m.ParentId == 0 && (m.Status == Status.Published || CurrentUser.IsAdmin), m => m.PostDate, false);
            if (!parent.Data.Any())
            {
                return ResultData(null, false, "没有留言");
            }
            var total = parent.TotalCount;
            parent.Data.Flatten().ForEach(m =>
            {
                m.PostDate = m.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            });
            if (total > 0)
            {
                return ResultData(new
                {
                    total,
                    parentTotal = total,
                    page,
                    size,
                    rows = Mapper.Map<List<LeaveMessageViewModel>>(parent.Data)
                });
            }

            return ResultData(null, false, "没有留言");
        }

        /// <summary>
        /// 发表留言
        /// </summary>
        /// <param name="mailSender"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Submit([FromServices] IMailSender mailSender, LeaveMessageCommand cmd)
        {
            var match = Regex.Match(cmd.NickName + cmd.Content.RemoveHtmlTag(), CommonHelper.BanRegex);
            if (match.Success)
            {
                LogManager.Info($"提交内容：{cmd.NickName}/{cmd.Content}，敏感词：{match.Value}");
                return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请检查您的内容后尝试重新提交！");
            }

            var error = await ValidateEmailCode(mailSender, cmd.Email, cmd.Code);
            if (!string.IsNullOrEmpty(error))
            {
                return ResultData(null, false, error);
            }

            if (cmd.ParentId > 0 && DateTime.Now - LeaveMessageService[cmd.ParentId, m => m.PostDate] > TimeSpan.FromDays(180))
            {
                return ResultData(null, false, "当前留言过于久远，不再允许回复！");
            }

            cmd.Content = cmd.Content.Trim().Replace("<p><br></p>", string.Empty);
            if (MsgFeq.GetOrAdd("Comments:" + ClientIP, 1) > 2)
            {
                MsgFeq.Expire("Comments:" + ClientIP, TimeSpan.FromMinutes(1));
                return ResultData(null, false, "您的发言频率过快，请稍后再发表吧！");
            }

            var msg = cmd.Mapper<LeaveMessage>();
            if (Regex.Match(cmd.NickName + cmd.Content, CommonHelper.ModRegex).Length <= 0)
            {
                msg.Status = Status.Published;
            }

            msg.PostDate = DateTime.Now;
            var user = HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
            if (user != null)
            {
                msg.NickName = user.NickName;
                msg.Email = user.Email;
                if (user.IsAdmin)
                {
                    msg.Status = Status.Published;
                    msg.IsMaster = true;
                }
            }

            msg.Content = await cmd.Content.HtmlSantinizerStandard().ClearImgAttributes();
            msg.Browser = cmd.Browser ?? Request.Headers[HeaderNames.UserAgent];
            msg.IP = ClientIP;
            msg.Location = Request.Location();
            msg = LeaveMessageService.AddEntitySaved(msg);
            if (msg == null)
            {
                return ResultData(null, false, "留言发表失败！");
            }

            Response.Cookies.Append("NickName", msg.NickName, new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            WriteEmailKeyCookie(cmd.Email);
            MsgFeq.AddOrUpdate("Comments:" + ClientIP, 1, i => i + 1, 5);
            MsgFeq.Expire("Comments:" + ClientIP, TimeSpan.FromMinutes(1));
            var email = CommonHelper.SystemSettings["ReceiveEmail"];
            var content = new Template(await System.IO.File.ReadAllTextAsync(HostEnvironment.WebRootPath + "/template/notify.html")).Set("title", "网站留言板").Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Set("nickname", msg.NickName).Set("content", msg.Content);
            if (msg.Status == Status.Published)
            {
                if (!msg.IsMaster)
                {
                    await MessageService.AddEntitySavedAsync(new InternalMessage()
                    {
                        Title = $"来自【{msg.NickName}】的新留言",
                        Content = msg.Content,
                        Link = Url.Action("Index", "Msg", new { cid = msg.Id })
                    });
                }
                if (msg.ParentId == 0)
                {
                    //新评论，只通知博主
                    BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "|博客新留言：", content.Set("link", Url.Action("Index", "Msg", new { cid = msg.Id }, Request.Scheme)).Render(false), email, ClientIP));
                }
                else
                {
                    //通知博主和上层所有关联的评论访客
                    var emails = (await LeaveMessageService.GetByIdAsync(msg.Id)).Root().Flatten().Select(c => c.Email).Append(email).Except(new[] { msg.Email }).ToHashSet();
                    string link = Url.Action("Index", "Msg", new { cid = msg.Id }, Request.Scheme);
                    foreach (var s in emails)
                    {
                        BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]} 留言回复：", content.Set("link", link).Render(false), s, ClientIP));
                    }
                }
                return ResultData(null, true, "留言发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将会显示到列表中！");
            }

            BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "|博客新留言(待审核)：", content.Set("link", Url.Action("Index", "Msg", new
            {
                cid = msg.Id
            }, Request.Scheme)).Render(false) + "<p style='color:red;'>(待审核)</p>", email, ClientIP));
            return ResultData(null, true, "留言发表成功，待站长审核通过以后将显示到列表中！");
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Pass(int id)
        {
            var msg = await LeaveMessageService.GetByIdAsync(id);
            msg.Status = Status.Published;
            bool b = await LeaveMessageService.SaveChangesAsync() > 0;
            if (b)
            {
                var root = msg.Root();
                var content = new Template(await System.IO.File.ReadAllTextAsync(Path.Combine(HostEnvironment.WebRootPath, "template", "notify.html"))).Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Set("nickname", msg.NickName).Set("content", msg.Content);
                var emails = root.Flatten().Select(c => c.Email).Except(new List<string> { msg.Email, CurrentUser.Email }).ToHashSet();
                var link = Url.Action("Index", "Msg", new { cid = root.Id }, Request.Scheme);
                foreach (var s in emails)
                {
                    BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]} 留言回复：", content.Set("link", link).Render(false), s, ClientIP));
                }
            }

            return ResultData(null, b, b ? "审核通过！" : "审核失败！");
        }

        /// <summary>
        /// 删除留言
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult Delete(int id)
        {
            var b = LeaveMessageService.DeleteById(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取待审核的留言
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> GetPendingMsgs([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var list = await LeaveMessageService.GetPagesAsync<DateTime, LeaveMessageDto>(page, size, m => m.Status == Status.Pending, l => l.PostDate, false);
            foreach (var m in list.Data)
            {
                m.PostDate = m.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return Ok(list);
        }

        #region 站内消息

        /// <summary>
        /// 已读站内信
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Read(int id)
        {
            await MessageService.GetQuery(m => m.Id == id).UpdateFromQueryAsync(m => new InternalMessage()
            {
                Read = true
            });
            return Content("ok");
        }

        /// <summary>
        /// 标记为未读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Unread(int id)
        {
            await MessageService.GetQuery(m => m.Id == id).UpdateFromQueryAsync(m => new InternalMessage()
            {
                Read = false
            });
            return Content("ok");
        }

        /// <summary>
        /// 标记为已读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> MarkRead(int id)
        {
            await MessageService.GetQuery(m => m.Id <= id).UpdateFromQueryAsync(m => new InternalMessage()
            {
                Read = true
            });
            return ResultData(null);
        }

        /// <summary>
        /// 删除站内信
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> DeleteMsg(int id)
        {
            bool b = await MessageService.DeleteByIdAsync(id) > 0;
            return ResultData(null, b, b ? "站内消息删除成功！" : "站内消息删除失败！");
        }

        /// <summary>
        /// 获取站内信
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult GetInternalMsgs([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var msgs = MessageService.GetPagesNoTracking(page, size, m => true, m => m.Time, false);
            return Ok(msgs);
        }

        /// <summary>
        /// 获取未读消息
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult GetUnreadMsgs()
        {
            var msgs = MessageService.GetQueryNoTracking(m => !m.Read, m => m.Time, false).NotCacheable().ToList();
            return ResultData(msgs);
        }

        /// <summary>
        /// 清除站内信
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> ClearMsgs()
        {
            await MessageService.DeleteEntitySavedAsync(m => m.Read);
            return ResultData(null, true, "站内消息清除成功！");
        }

        #endregion
    }
}