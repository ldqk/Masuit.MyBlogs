using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if !DEBUG
#endif
using System.Web.Mvc;
using Common;
using Hangfire;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Models.ViewModel;
using static Common.CommonHelper;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class MsgController : BaseController
    {
        public ILeaveMessageBll LeaveMessageBll { get; set; }
        public IInternalMessageBll MessageBll { get; set; }

        public MsgController(ILeaveMessageBll leaveMessageBll, IInternalMessageBll messageBll)
        {
            LeaveMessageBll = leaveMessageBll;
            MessageBll = messageBll;
        }

        [OutputCache(Duration = 10, VaryByParam = "cid"), Route("msg")]
        public ActionResult Index()
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            ViewBag.TotalCount = LeaveMessageBll.LoadEntitiesNoTracking(m => m.ParentId == 0 && m.Status == Status.Pended).Count();
            if (user.IsAdmin)
            {
                return View("Index_Admin");
            }
            return View();
        }

        public ActionResult GetMsgs(int page = 1, int size = 10, int cid = 0)
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            int total;
            if (cid != 0)
            {
                int pid = LeaveMessageBll.GetParentMessageIdByChildId(cid);
                var single = LeaveMessageBll.GetSelfAndAllChildrenMessagesByParentId(pid).ToList();
                if (single.Any())
                {
                    total = 1;
                    return ResultData(new { total, parentTotal = total, page, size, rows = single.Mapper<IList<LeaveMessageViewModel>>() });
                }
            }
            IEnumerable<LeaveMessage> parent = LeaveMessageBll.LoadPageEntitiesNoTracking(page, size, out total, m => m.ParentId == 0 && (m.Status == Status.Pended || user.IsAdmin), m => m.PostDate, false);
            if (!parent.Any())
            {
                return ResultData(null, false, "没有留言");
            }
            var list = new List<LeaveMessageViewModel>();
            parent.ForEach(c => LeaveMessageBll.GetSelfAndAllChildrenMessagesByParentId(c.Id).ForEach(result => list.Add(result.Mapper<LeaveMessageViewModel>())));
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

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Put(LeaveMessageInputDto msg)
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            msg.Content = msg.Content.Trim().Replace("<p><br></p>", string.Empty);
            if (msg.Content.RemoveHtml().Trim().Equals(Session.GetByRedis<string>("msg")))
            {
                return ResultData(null, false, "您刚才已经发表过一次留言了！");
            }
            if (Regex.Match(msg.Content, ModRegex).Length <= 0)
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
            msg.Browser = msg.Browser ?? Request.Browser.Type;
            msg.IP = Request.UserHostAddress;
            LeaveMessage msg2 = LeaveMessageBll.AddEntitySaved(msg.Mapper<LeaveMessage>());
            if (msg2 != null)
            {
                Session.SetByRedis("msg", msg.Content.RemoveHtml().Trim());
                var email = GetSettings("ReceiveEmail");
                string content = System.IO.File.ReadAllText(Request.MapPath("/template/notify.html")).Replace("{{title}}", "网站留言板").Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", msg2.NickName).Replace("{{content}}", msg2.Content);
                if (msg.Status == Status.Pended)
                {
                    if (!msg2.IsMaster)
                    {
                        MessageBll.AddEntitySaved(new InternalMessage() { Title = $"来自【{msg2.NickName}】的新留言", Content = msg2.Content, Link = Url.Action("Index", "Msg", new { cid = msg2.Id }, Request.Url.Scheme) });
                    }
#if !DEBUG
                    if (msg.ParentId == 0)
                    {
                        //新评论，只通知博主
                        BackgroundJob.Enqueue(() => SendMail(Request.Url.Authority + "|博客新留言：", content.Replace("{{link}}", Url.Action("Index", "Msg", new { cid = msg2.Id }, Request.Url.Scheme)), email));
                    }
                    else
                    {
                        //通知博主和上层所有关联的评论访客
                        var pid = LeaveMessageBll.GetParentMessageIdByChildId(msg2.Id);
                        var emails = LeaveMessageBll.GetSelfAndAllChildrenMessagesByParentId(pid).Select(c => c.Email).ToList();
                        emails.Add(email);
                        string link = Url.Action("Index", "Msg", new { cid = msg2.Id }, Request.Url.Scheme);
                        BackgroundJob.Enqueue(() => SendMail($"{Request.Url.Authority}{GetSettings("Title")} 留言回复：", content.Replace("{{link}}", link), string.Join(",", emails.Distinct().Except(new[] { msg2.Email }))));
                    }
#endif
                    return ResultData(null, true, "留言发表成功，服务器正在后台处理中，这会有一定的延迟，稍后将会显示到列表中！");
                }
                BackgroundJob.Enqueue(() => SendMail(Request.Url.Authority + "|博客新留言(待审核)：", content.Replace("{{link}}", Url.Action("Index", "Msg", new { cid = msg2.Id }, Request.Url.Scheme)) + "<p style='color:red;'>(待审核)</p>", email));
                return ResultData(null, true, "留言发表成功，待站长审核通过以后将显示到列表中！");
            }
            return ResultData(null, false, "留言发表失败！");
        }

        [Authority]
        public ActionResult Pass(int id)
        {
            var msg = LeaveMessageBll.GetById(id);
            msg.Status = Status.Pended;
            bool b = LeaveMessageBll.UpdateEntitySaved(msg);
#if !DEBUG
            var pid = msg.ParentId == 0 ? msg.Id : LeaveMessageBll.GetParentMessageIdByChildId(id);
            string content = System.IO.File.ReadAllText(Request.MapPath("/template/notify.html")).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{nickname}}", msg.NickName).Replace("{{content}}", msg.Content);
            var emails = LeaveMessageBll.GetSelfAndAllChildrenMessagesByParentId(pid).Select(c => c.Email).Distinct().Except(new List<string>() { msg.Email }).ToList();
            string link = Url.Action("Index", "Msg", new { cid = pid }, Request.Url.Scheme);
            BackgroundJob.Enqueue(() => SendMail($"{Request.Url.Authority}{GetSettings("Title")} 留言回复：", content.Replace("{{link}}", link), string.Join(",", emails)));
#endif
            return ResultData(null, b, b ? "审核通过！" : "审核失败！");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            var b = LeaveMessageBll.DeleteEntitiesSaved(LeaveMessageBll.GetSelfAndAllChildrenMessagesByParentId(id).ToList());
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取待审核的留言
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPendingMsgs(int page = 1, int size = 10)
        {
            List<LeaveMessageOutputDto> list = LeaveMessageBll.LoadPageEntities<DateTime, LeaveMessageOutputDto>(page, size, out int total, m => m.Status == Status.Pending, l => l.PostDate, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        #region 站内消息

        [Authority]
        public ActionResult Read(int id)
        {
            InternalMessage msg = MessageBll.GetById(id);
            msg.Read = true;
            MessageBll.UpdateEntitySaved(msg);
            return Content("ok");
        }

        [Authority]
        public ActionResult Unread(int id)
        {
            InternalMessage msg = MessageBll.GetById(id);
            msg.Read = false;
            MessageBll.UpdateEntitySaved(msg);
            return Content("ok");
        }

        [Authority]
        public ActionResult DeleteMsg(int id)
        {
            bool b = MessageBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "站内消息删除成功！" : "站内消息删除失败！");
        }

        [Authority]
        public ActionResult GetInternalMsgs(int page = 1, int size = 10)
        {
            IEnumerable<InternalMessage> msgs = MessageBll.LoadPageEntitiesNoTracking(page, size, out int total, m => true, m => m.Time, false);
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(msgs, pageCount, total);
        }

        public ActionResult GetUnreadMsgs()
        {
            IEnumerable<InternalMessage> msgs = MessageBll.LoadEntitiesNoTracking(m => !m.Read, m => m.Time, false);
            return ResultData(msgs);
        }

        [Authority]
        public ActionResult ClearMsgs()
        {
            MessageBll.DeleteEntitySaved(m => m.Read);
            return ResultData(null, true, "站内消息清除成功！");
        }

        [Authority]
        public ActionResult MarkRead(int id)
        {
            List<InternalMessage> msgs = MessageBll.LoadEntities(m => m.Id <= id).ToList();
            for (var i = 0; i < msgs.Count; i++)
            {
                msgs[i].Read = true;
            }
            MessageBll.UpdateEntities(msgs);
            MessageBll.SaveChanges();
            return ResultData(null);
        }

        #endregion
    }
}