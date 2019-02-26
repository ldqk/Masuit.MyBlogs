using Common;
using Hangfire;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

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

        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// 留言板和站内信
        /// </summary>
        /// <param name="leaveMessageService"></param>
        /// <param name="messageService"></param>
        /// <param name="hostingEnvironment"></param>
        public MsgController(ILeaveMessageService leaveMessageService, IInternalMessageService messageService, IHostingEnvironment hostingEnvironment)
        {
            LeaveMessageService = leaveMessageService;
            MessageService = messageService;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 留言板
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie), Route("msg")]
        public ActionResult Index()
        {
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            ViewBag.TotalCount = LeaveMessageService.LoadEntitiesNoTracking(m => m.ParentId == 0 && m.Status == Status.Pended).Count();
            if (user.IsAdmin)
            {
                return View("Index_Admin");
            }
            return View();
        }

        /// <summary>
        /// 获取留言
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        public ActionResult GetMsgs(int page = 1, int size = 10, int cid = 0)
        {
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            int total;
            if (cid != 0)
            {
                int pid = LeaveMessageService.GetParentMessageIdByChildId(cid);
                var single = LeaveMessageService.GetSelfAndAllChildrenMessagesByParentId(pid).ToList();
                if (single.Any())
                {
                    total = 1;
                    return ResultData(new
                    {
                        total,
                        parentTotal = total,
                        page,
                        size,
                        rows = single.Mapper<IList<LeaveMessageViewModel>>()
                    });
                }
            }
            IEnumerable<LeaveMessage> parent = LeaveMessageService.LoadPageEntitiesNoTracking(page, size, out total, m => m.ParentId == 0 && (m.Status == Status.Pended || user.IsAdmin), m => m.PostDate, false);
            if (!parent.Any())
            {
                return ResultData(null, false, "没有留言");
            }
            var list = new List<LeaveMessageViewModel>();
            parent.ForEach(c => LeaveMessageService.GetSelfAndAllChildrenMessagesByParentId(c.Id).ForEach(result => list.Add(result.Mapper<LeaveMessageViewModel>())));
            var qlist = list.Where(c => c.Status == Status.Pended || user.IsAdmin);
            if (total > 0)
            {
                return ResultData(new
                {
                    total,
                    parentTotal = total,
                    page,
                    size,
                    rows = qlist
                });
            }
            return ResultData(null, false, "没有留言");
        }

        /// <summary>
        /// 发表留言
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Put(LeaveMessageInputDto msg)
        {
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            msg.Content = msg.Content.Trim().Replace("<p><br></p>", string.Empty);
            if (msg.Content.RemoveHtml().Trim().Equals(HttpContext.Session.GetByRedis<string>("msg")))
            {
                return ResultData(null, false, "您刚才已经发表过一次留言了！");
            }
            if (Regex.Match(msg.Content, CommonHelper.ModRegex).Length <= 0)
            {
                msg.Status = Status.Pended;
            }

            if (user != null)
            {
                msg.NickName = user.NickName;
                msg.QQorWechat = user.QQorWechat;
                msg.Email = user.Email;
                if (user.IsAdmin)
                {
                    msg.Status = Status.Pended;
                    msg.IsMaster = true;
                }
            }
            msg.PostDate = DateTime.Now;
            msg.Content = Regex.Replace(msg.Content.HtmlSantinizerStandard().ConvertImgSrcToRelativePath(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>");
            msg.Browser = msg.Browser ?? Request.Headers[HeaderNames.UserAgent];
            msg.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            LeaveMessage msg2 = LeaveMessageService.AddEntitySaved(msg.Mapper<LeaveMessage>());
            if (msg2 != null)
            {
                HttpContext.Session.SetByRedis("msg", msg.Content.RemoveHtml().Trim());
                var email = CommonHelper.SystemSettings["ReceiveEmail"];
                string content = System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/notify.html").Replace("{{title}}", "网站留言板").Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", msg2.NickName).Replace("{{content}}", msg2.Content);
                if (msg.Status == Status.Pended)
                {
                    if (!msg2.IsMaster)
                    {
                        MessageService.AddEntitySaved(new InternalMessage()
                        {
                            Title = $"来自【{msg2.NickName}】的新留言",
                            Content = msg2.Content,
                            Link = Url.Action("Index", "Msg", new
                            {
                                cid = msg2.Id
                            }, Request.Scheme)
                        });
                    }
#if !DEBUG
                    if (msg.ParentId == 0)
                    {
                        //新评论，只通知博主
                        BackgroundJob.Enqueue(() => CommonHelper.SendMail(HttpUtility.UrlDecode(Request.Headers[HeaderNames.Referer]) + "|博客新留言：", content.Replace("{{link}}", Url.Action("Index", "Msg", new { cid = msg2.Id }, Request.Scheme)), email));
                    }
                    else
                    {
                        //通知博主和上层所有关联的评论访客
                        var pid = LeaveMessageService.GetParentMessageIdByChildId(msg2.Id);
                        var emails = LeaveMessageService.GetSelfAndAllChildrenMessagesByParentId(pid).Select(c => c.Email).ToList();
                        emails.Add(email);
                        string link = Url.Action("Index", "Msg", new { cid = msg2.Id }, Request.Scheme);
                        foreach (var s in emails.Distinct().Except(new[] { msg2.Email }))
                        {
                            BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{HttpUtility.UrlDecode(Request.Headers[HeaderNames.Referer])}{CommonHelper.SystemSettings["Title"]} 留言回复：", content.Replace("{{link}}", link), s));
                        }
                    }
#endif
                    return ResultData(null, true, "留言发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将会显示到列表中！");
                }
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(HttpUtility.UrlDecode(Request.Headers[HeaderNames.Referer]) + "|博客新留言(待审核)：", content.Replace("{{link}}", Url.Action("Index", "Msg", new
                {
                    cid = msg2.Id
                }, Request.Scheme)) + "<p style='color:red;'>(待审核)</p>", email));
                return ResultData(null, true, "留言发表成功，待站长审核通过以后将显示到列表中！");
            }
            return ResultData(null, false, "留言发表失败！");
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Pass(int id)
        {
            var msg = LeaveMessageService.GetById(id);
            msg.Status = Status.Pended;
            bool b = LeaveMessageService.UpdateEntitySaved(msg);
#if !DEBUG
            var pid = msg.ParentId == 0 ? msg.Id : LeaveMessageService.GetParentMessageIdByChildId(id);
            string content = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "template", "notify.html")).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", msg.NickName).Replace("{{content}}", msg.Content);
            var emails = LeaveMessageService.GetSelfAndAllChildrenMessagesByParentId(pid).Select(c => c.Email).Distinct().Except(new List<string>() { msg.Email }).ToList();
            string link = Url.Action("Index", "Msg", new { cid = pid }, Request.Scheme);
            foreach (var s in emails)
            {
                BackgroundJob.Enqueue(() => CommonHelper.SendMail($"{Request.Host}{CommonHelper.SystemSettings["Title"]} 留言回复：", content.Replace("{{link}}", link), string.Join(",", s)));
            }
#endif
            return ResultData(null, b, b ? "审核通过！" : "审核失败！");
        }

        /// <summary>
        /// 删除留言
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete(int id)
        {
            var b = LeaveMessageService.DeleteEntitiesSaved(LeaveMessageService.GetSelfAndAllChildrenMessagesByParentId(id).ToList());
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取待审核的留言
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPendingMsgs(int page = 1, int size = 10)
        {
            List<LeaveMessageOutputDto> list = LeaveMessageService.LoadPageEntities<DateTime, LeaveMessageOutputDto>(page, size, out int total, m => m.Status == Status.Pending, l => l.PostDate, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        #region 站内消息

        /// <summary>
        /// 已读站内信
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Read(int id)
        {
            InternalMessage msg = MessageService.GetById(id);
            msg.Read = true;
            MessageService.UpdateEntitySaved(msg);
            return Content("ok");
        }

        /// <summary>
        /// 标记为未读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Unread(int id)
        {
            InternalMessage msg = MessageService.GetById(id);
            msg.Read = false;
            MessageService.UpdateEntitySaved(msg);
            return Content("ok");
        }

        /// <summary>
        /// 删除站内信
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult DeleteMsg(int id)
        {
            bool b = MessageService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "站内消息删除成功！" : "站内消息删除失败！");
        }

        /// <summary>
        /// 获取站内信
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult GetInternalMsgs(int page = 1, int size = 10)
        {
            IEnumerable<InternalMessage> msgs = MessageService.LoadPageEntitiesNoTracking(page, size, out int total, m => true, m => m.Time, false);
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(msgs, pageCount, total);
        }

        /// <summary>
        /// 获取未读消息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUnreadMsgs()
        {
            IEnumerable<InternalMessage> msgs = MessageService.LoadEntitiesNoTracking(m => !m.Read, m => m.Time, false);
            return ResultData(msgs);
        }

        /// <summary>
        /// 清除站内信
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult ClearMsgs()
        {
            MessageService.DeleteEntitySaved(m => m.Read);
            return ResultData(null, true, "站内消息清除成功！");
        }

        /// <summary>
        /// 标记为已读
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult MarkRead(int id)
        {
            List<InternalMessage> msgs = MessageService.LoadEntities(m => m.Id <= id).ToList();
            foreach (var t in msgs)
            {
                t.Read = true;
            }
            MessageService.UpdateEntities(msgs);
            MessageService.SaveChanges();
            return ResultData(null);
        }

        #endregion
    }
}