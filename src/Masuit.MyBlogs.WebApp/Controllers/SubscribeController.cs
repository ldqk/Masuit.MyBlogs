using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Hangfire;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Models.Entity;
using Models.Enum;
using static Common.CommonHelper;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class SubscribeController : BaseController
    {
        public IBroadcastBll BroadcastBll { get; set; }
        public IPostBll PostBll { get; set; }

        public SubscribeController(IBroadcastBll broadcastBll, IPostBll postBll)
        {
            BroadcastBll = broadcastBll;
            PostBll = postBll;
        }

        [Route("rss")]
        public RssResult Rss()
        {
            var time = DateTime.Now.AddDays(-1);
            var posts = PostBll.LoadEntitiesFromL2CacheNoTracking(p => p.Status == Status.Pended && p.ModifyDate >= time, p => p.ModifyDate, false).ToList();
            return new RssResult(posts);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Subscribe(string email)
        {
            Broadcast entity = BroadcastBll.GetFirstEntity(b => b.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            var guid = Guid.NewGuid();
            if (entity != null)
            {
                if (entity.Status == Status.Subscribed)
                {
                    return ResultData(null, false, "您已经订阅过了，无需再重复订阅！");
                }
                entity.ValidateCode = guid.ToString();
                entity.UpdateTime = DateTime.Now;
                BroadcastBll.UpdateEntity(entity);
            }
            else
            {
                BroadcastBll.AddEntity(new Broadcast()
                {
                    Email = email,
                    ValidateCode = guid.ToString(),
                    Status = Status.Subscribing,
                    UpdateTime = DateTime.Now
                });
            }
            try
            {
                var ts = DateTime.Now.GetTotalMilliseconds();
                string link = Url.Action("Subscribe", "Subscribe", new
                {
                    email,
                    act = "verify",
                    validate = guid,
                    timespan = ts,
                    hash = (email + "verify" + guid + ts).AESEncrypt(ConfigurationManager.AppSettings["BaiduAK"])
                }, "http");
                BackgroundJob.Enqueue(() => SendMail(GetSettings("Title") + "博客订阅：" + Request.Url, System.IO.File.ReadAllText(Request.MapPath("/template/subscribe.html")).Replace("{{link}}", link), email));
                BroadcastBll.SaveChanges();
                return ResultData(null, message: "订阅成功，请打开您的邮箱确认操作后便可收到订阅更新！");
            }
            catch (Exception e)
            {
                LogManager.Error(GetType(), e);
                return ResultData(null, false, "订阅失败，这可能是服务器出现了一点问题，去留言板给站长反馈吧！");
            }
        }

        public ActionResult Cancel()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Cancel(string email)
        {
            Broadcast c = BroadcastBll.GetFirstEntity(b => b.Email.Equals(email) && b.Status == Status.Subscribed);
            if (c != null)
            {
                var ts = DateTime.Now.GetTotalMilliseconds();
                string url = Url.Action("Subscribe", "Subscribe", new
                {
                    email,
                    act = "cancel",
                    validate = c.ValidateCode,
                    timespan = ts,
                    hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(ConfigurationManager.AppSettings["BaiduAK"])
                }, Request.Url.Scheme);
                BackgroundJob.Enqueue(() => SendMail("取消本站订阅", $"请<a href=\"{url}\">点击这里</a>取消订阅本站更新。", email));
                return Content("取消订阅的链接已经发送到了您的邮箱，请到您的邮箱内进行取消订阅");
            }
            return Content("您输入的邮箱没有订阅本站更新，或者已经取消订阅了");
        }

        public ActionResult Subscribe(string email, string act, string validate, double timespan, string hash)
        {
            var ts = DateTime.Now.GetTotalMilliseconds();
            if (ts - timespan > 86400000)
            {
                return Content("链接已失效");
            }
            var hash2 = (email + act + validate + timespan).AESEncrypt(ConfigurationManager.AppSettings["BaiduAK"]);
            if (!hash2.Equals(hash))
            {
                return Content("操作失败，链接已被非法篡改");
            }
            Broadcast entity = BroadcastBll.GetFirstEntity(b => b.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) && b.ValidateCode.Equals(validate));
            if (entity != null)
            {
                switch (act)
                {
                    case "verify":
                        entity.Status = Status.Subscribed;
                        entity.ValidateCode = Guid.NewGuid().ToString();
                        entity.UpdateTime = DateTime.Now;
                        BroadcastBll.UpdateEntity(entity);
                        BroadcastBll.SaveChanges();
                        return Content("订阅成功！");
                    case "cancel":
                        entity.Status = Status.Canceled;
                        entity.UpdateTime = DateTime.Now;
                        BroadcastBll.UpdateEntity(entity);
                        BroadcastBll.SaveChanges();
                        return Content("取消订阅成功，您将不会再接收到文章更新，如果您以后需要再次接收更新推送，可以到主站点重新进行订阅操作！");
                    default: return RedirectToAction("Index", "Home");
                }
            }
            return Content("该邮箱账户未使用邮件订阅！");
        }


        #region 管理端

        [Authority]
        public ActionResult Save(Broadcast model)
        {
            model.UpdateTime = DateTime.Now;
            bool b = BroadcastBll.AddOrUpdateSaved(c => c.Email, model) > 0;
            return ResultData(model, b, b ? "更新订阅成功！" : "更新订阅失败！");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            bool b = BroadcastBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除订阅成功！" : "删除订阅失败！");
        }

        [Authority]
        public ActionResult Change(int id)
        {
            Broadcast cast = BroadcastBll.GetById(id);
            Status status = cast.Status;
            cast.UpdateTime = DateTime.Now;
            cast.Status = status == Status.Subscribed ? Status.Subscribing : Status.Subscribed;
            bool b = BroadcastBll.UpdateEntitySaved(cast);
            return ResultData(null, b, status == Status.Subscribed ? "订阅成功" : "取消订阅成功！");
        }

        [Authority]
        public ActionResult Get(int id) => ResultData(BroadcastBll.GetById(id));

        [Authority]
        public ActionResult GetPageData(int page = 1, int size = 10, string search = "")
        {
            List<Broadcast> list;
            int total;
            if (string.IsNullOrEmpty(search))
            {
                list = BroadcastBll.LoadPageEntitiesFromL2CacheNoTracking(page, size, out total, b => true, b => b.UpdateTime, false).ToList();
            }
            else
            {
                list = BroadcastBll.LoadPageEntitiesFromL2CacheNoTracking(page, size, out total, b => b.Email.Contains(search), b => b.UpdateTime, false).ToList();
            }
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        #endregion
    }
}