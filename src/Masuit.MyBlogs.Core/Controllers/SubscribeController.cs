using Common;
using EFSecondLevelCache.Core;
using Hangfire;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using WilderMinds.RssSyndication;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 订阅服务
    /// </summary>
    public class SubscribeController : BaseController
    {
        /// <summary>
        /// 邮箱广播
        /// </summary>
        public IBroadcastService BroadcastService { get; set; }

        /// <summary>
        /// 文章
        /// </summary>
        public IPostService PostService { get; set; }

        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// 订阅服务
        /// </summary>
        /// <param name="broadcastService"></param>
        /// <param name="postService"></param>
        /// <param name="hostingEnvironment"></param>
        public SubscribeController(IBroadcastService broadcastService, IPostService postService, IHostingEnvironment hostingEnvironment)
        {
            BroadcastService = broadcastService;
            PostService = postService;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// RSS订阅
        /// </summary>
        /// <returns></returns>
        [Route("rss"), ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public IActionResult Rss()
        {
            var time = DateTime.Now.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var posts = PostService.LoadEntitiesNoTracking(p => p.Status == Status.Pended && p.ModifyDate >= time, p => p.ModifyDate, false).Select(p => new Item()
            {
                Author = new Author()
                {
                    Name = p.Author,
                    Email = p.Email
                },
                Body = p.Content,
                Categories = new List<string>()
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate,
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id
            }).Cacheable().ToList();
            var feed = new Feed()
            {
                Title = CommonHelper.SystemSettings["Title"],
                Description = CommonHelper.SystemSettings["Description"],
                Link = new Uri(scheme + "://" + host + "/rss"),
                Copyright = "(c) 2019"
            };
            feed.Items.AddRange(posts.ToArray());
            var rss = feed.Serialize();
            return Content(rss);
        }

        /// <summary>
        /// 邮箱订阅
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Subscribe(string email)
        {
            if (bool.TryParse(CommonHelper.SystemSettings["DisabledEmailBroadcast"], out var disabled) && disabled)
            {
                return ResultData(null, false, CommonHelper.SystemSettings["DisabledEmailBroadcastTip"]);
            }
            Broadcast entity = BroadcastService.GetFirstEntity(b => b.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            var guid = Guid.NewGuid();
            if (entity != null)
            {
                if (entity.Status == Status.Subscribed)
                {
                    return ResultData(null, false, "您已经订阅过了，无需再重复订阅！");
                }
                entity.ValidateCode = guid.ToString();
                entity.UpdateTime = DateTime.Now;
                BroadcastService.UpdateEntity(entity);
            }
            else
            {
                BroadcastService.AddEntity(new Broadcast()
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
                    hash = (email + "verify" + guid + ts).AESEncrypt(AppConfig.BaiduAK)
                }, "http");
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客订阅：" + Request.Host, System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/subscribe.html").Replace("{{link}}", link), email));
                BroadcastService.SaveChanges();
                return ResultData(null, message: "订阅成功，请打开您的邮箱确认操作后便可收到订阅更新！");
            }
            catch (Exception e)
            {
                LogManager.Error(GetType(), e);
                return ResultData(null, false, "订阅失败，这可能是服务器出现了一点问题，去留言板给站长反馈吧！");
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <returns></returns>
        public ActionResult Cancel()
        {
            return View();
        }

        /// <summary>
        /// 取消邮箱订阅
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Cancel(string email)
        {
            Broadcast c = BroadcastService.GetFirstEntity(b => b.Email.Equals(email) && b.Status == Status.Subscribed);
            if (c != null)
            {
                var ts = DateTime.Now.GetTotalMilliseconds();
                string url = Url.Action("Subscribe", "Subscribe", new
                {
                    email,
                    act = "cancel",
                    validate = c.ValidateCode,
                    timespan = ts,
                    hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(AppConfig.BaiduAK)
                }, Request.Scheme);
                BackgroundJob.Enqueue(() => CommonHelper.SendMail("取消本站订阅", $"请<a href=\"{url}\">点击这里</a>取消订阅本站更新。", email));
                return Content("取消订阅的链接已经发送到了您的邮箱，请到您的邮箱内进行取消订阅");
            }
            return Content("您输入的邮箱没有订阅本站更新，或者已经取消订阅了");
        }

        /// <summary>
        /// 邮箱订阅
        /// </summary>
        /// <param name="email"></param>
        /// <param name="act"></param>
        /// <param name="validate"></param>
        /// <param name="timespan"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public ActionResult Subscribe(string email, string act, string validate, double timespan, string hash)
        {
            var ts = DateTime.Now.GetTotalMilliseconds();
            if (ts - timespan > 86400000)
            {
                return Content("链接已失效");
            }
            var hash2 = (email + act + validate + timespan).AESEncrypt(AppConfig.BaiduAK);
            if (!hash2.Equals(hash))
            {
                return Content("操作失败，链接已被非法篡改");
            }
            Broadcast entity = BroadcastService.GetFirstEntity(b => b.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) && b.ValidateCode.Equals(validate));
            if (entity != null)
            {
                switch (act)
                {
                    case "verify":
                        entity.Status = Status.Subscribed;
                        entity.ValidateCode = Guid.NewGuid().ToString();
                        entity.UpdateTime = DateTime.Now;
                        BroadcastService.UpdateEntity(entity);
                        BroadcastService.SaveChanges();
                        return Content("订阅成功！");
                    case "cancel":
                        entity.Status = Status.Canceled;
                        entity.UpdateTime = DateTime.Now;
                        BroadcastService.UpdateEntity(entity);
                        BroadcastService.SaveChanges();
                        return Content("取消订阅成功，您将不会再接收到文章更新，如果您以后需要再次接收更新推送，可以到主站点重新进行订阅操作！");
                    default: return RedirectToAction("Index", "Home");
                }
            }
            return Content("该邮箱账户未使用邮件订阅！");
        }


        #region 管理端

        /// <summary>
        /// 保存订阅
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Save(Broadcast model)
        {
            model.UpdateTime = DateTime.Now;
            var entry = BroadcastService.GetFirstEntity(c => c.Email.Equals(model.Email));
            bool b;
            if (entry is null)
            {
                b = BroadcastService.AddEntitySaved(model) != null;
            }
            else
            {
                entry.Email = model.Email;
                entry.SubscribeType = model.SubscribeType;
                entry.UpdateTime = DateTime.Now;
                b = BroadcastService.UpdateEntitySaved(entry);
            }
            return ResultData(model, b, b ? "更新订阅成功！" : "更新订阅失败！");
        }

        /// <summary>
        /// 删除订阅
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete(int id)
        {
            bool b = BroadcastService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除订阅成功！" : "删除订阅失败！");
        }

        /// <summary>
        /// 改变订阅类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Change(int id)
        {
            Broadcast cast = BroadcastService.GetById(id);
            Status status = cast.Status;
            cast.UpdateTime = DateTime.Now;
            cast.Status = status == Status.Subscribed ? Status.Subscribing : Status.Subscribed;
            bool b = BroadcastService.UpdateEntitySaved(cast);
            return ResultData(null, b, status == Status.Subscribed ? "订阅成功" : "取消订阅成功！");
        }

        /// <summary>
        /// 获取订阅
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Get(int id) => ResultData(BroadcastService.GetById(id));

        /// <summary>
        /// 订阅分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPageData(int page = 1, int size = 10, string search = "")
        {
            List<Broadcast> list;
            int total;
            if (string.IsNullOrEmpty(search))
            {
                list = BroadcastService.LoadPageEntitiesFromL2CacheNoTracking(page, size, out total, b => true, b => b.UpdateTime, false).ToList();
            }
            else
            {
                list = BroadcastService.LoadPageEntitiesFromL2CacheNoTracking(page, size, out total, b => b.Email.Contains(search), b => b.UpdateTime, false).ToList();
            }
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        #endregion
    }
}