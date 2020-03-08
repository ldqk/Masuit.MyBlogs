using EFSecondLevelCache.Core;
using Hangfire;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using WilderMinds.RssSyndication;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 订阅服务
    /// </summary>
    public class SubscribeController : Controller
    {
        /// <summary>
        /// 邮箱广播
        /// </summary>
        public IBroadcastService BroadcastService { get; set; }

        /// <summary>
        /// 文章
        /// </summary>
        public IPostService PostService { get; set; }

        public ICategoryService CategoryService { get; set; }
        public ICommentService CommentService { get; set; }
        public IWebHostEnvironment HostEnvironment { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="success">响应状态</param>
        /// <param name="message">响应消息</param>
        /// <param name="isLogin">登录状态</param>
        /// <param name="code">http响应码</param>
        /// <returns></returns>
        public ActionResult ResultData(object data, bool success = true, string message = "", bool isLogin = true, HttpStatusCode code = HttpStatusCode.OK)
        {
            return Ok(new
            {
                IsLogin = isLogin,
                Success = success,
                Message = message,
                Data = data,
                code
            });
        }

        /// <summary>
        /// RSS订阅
        /// </summary>
        /// <returns></returns>
        [Route("/rss"), ResponseCache(Duration = 600)]
        public IActionResult Rss()
        {
            var time = DateTime.Today.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var posts = PostService.GetQueryNoTracking(p => p.Status == Status.Pended && p.ModifyDate >= time, p => p.ModifyDate, false).Select(p => new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email
                },
                Body = p.Content.GetSummary(300, 50),
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate,
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = p.Content.GetSummary(300, 50)
            }).Cacheable().ToList();
            var feed = new Feed()
            {
                Title = CommonHelper.SystemSettings["Title"],
                Description = CommonHelper.SystemSettings["Description"],
                Link = new Uri(scheme + "://" + host + "/rss"),
                Copyright = CommonHelper.SystemSettings["Title"],
                Language = "zh-cn",
                Items = posts.ToArray()
            };
            var rss = feed.Serialize(new SerializeOption()
            {
                Encoding = Encoding.UTF8
            });
            return Content(rss, "text/xml");
        }

        /// <summary>
        /// RSS分类订阅
        /// </summary>
        /// <returns></returns>
        [Route("/cat/{id}/rss"), ResponseCache(Duration = 600)]
        public IActionResult CategoryRss(int id)
        {
            var time = DateTime.Today.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var category = CategoryService.GetById(id) ?? throw new NotFoundException("分类未找到");
            var posts = PostService.GetQueryNoTracking(p => p.CategoryId == id && p.Status == Status.Pended && p.ModifyDate >= time, p => p.ModifyDate, false).Select(p => new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email
                },
                Body = p.Content.GetSummary(300, 50),
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate,
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = p.Content.GetSummary(300, 50)
            }).Cacheable().ToList();
            var feed = new Feed()
            {
                Title = CommonHelper.SystemSettings["Domain"] + $":分类{category.Name}文章订阅",
                Description = category.Description,
                Link = new Uri(scheme + "://" + host + "/rss"),
                Copyright = CommonHelper.SystemSettings["Title"],
                Language = "zh-cn",
                Items = posts.ToArray()
            };
            var rss = feed.Serialize(new SerializeOption()
            {
                Encoding = Encoding.UTF8
            });
            return Content(rss, "text/xml");
        }

        /// <summary>
        /// RSS文章订阅
        /// </summary>
        /// <returns></returns>
        [Route("/{id}/rss"), ResponseCache(Duration = 600)]
        public IActionResult PostRss(int id)
        {
            string scheme = Request.Scheme;
            var host = Request.Host;
            var p = PostService.Get(p => p.Status == Status.Pended && p.Id == id) ?? throw new NotFoundException("文章未找到");
            var summary = p.Content.GetSummary(300, 50);
            var item = new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email
                },
                Body = summary,
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate,
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = summary
            };
            var feed = new Feed()
            {
                Title = CommonHelper.SystemSettings["Domain"] + $":文章【{p.Title}】更新订阅",
                Description = summary,
                Link = new Uri(scheme + "://" + host + "/rss/" + id),
                Copyright = CommonHelper.SystemSettings["Title"],
                Language = "zh-cn",
                Items = new List<Item>() { item }
            };
            var rss = feed.Serialize(new SerializeOption()
            {
                Encoding = Encoding.UTF8
            });
            return Content(rss, "text/xml");
        }

        /// <summary>
        /// RSS文章评论订阅
        /// </summary>
        /// <returns></returns>
        [Route("/{id}/comments/rss"), ResponseCache(Duration = 600)]
        public IActionResult CommentsRss(int id)
        {
            string scheme = Request.Scheme;
            var host = Request.Host;
            var post = PostService.Get(p => p.Status == Status.Pended && p.Id == id) ?? throw new NotFoundException("文章不存在");
            var start = DateTime.Today.AddDays(-7);
            var comments = CommentService.GetQuery(c => c.PostId == post.Id && c.CommentDate > start).Select(c => new Item()
            {
                Author = new Author
                {
                    Name = c.NickName
                },
                Body = c.Content,
                Categories = new List<string>
                {
                    c.Post.Title
                },
                Link = new Uri($"{scheme}://{host}/{post.Id}?cid={c.Id}#comment"),
                PublishDate = c.CommentDate,
                Title = c.NickName,
                Permalink = $"{scheme}://{host}/{post.Id}?cid={c.Id}#comment",
                Guid = c.Id.ToString(),
                FullHtmlContent = c.Content
            }).ToList();
            var feed = new Feed()
            {
                Title = CommonHelper.SystemSettings["Domain"] + $":文章【{post.Title}】文章评论更新订阅",
                Description = post.Content.GetSummary(300, 50),
                Link = new Uri(scheme + "://" + host + "/rss/" + id + "/comments"),
                Copyright = CommonHelper.SystemSettings["Title"],
                Language = "zh-cn",
                Items = comments.ToArray()
            };
            var rss = feed.Serialize(new SerializeOption()
            {
                Encoding = Encoding.UTF8
            });
            return Content(rss, "text/xml");
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
            Broadcast entity = BroadcastService.Get(b => b.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
            var guid = Guid.NewGuid();
            if (entity != null)
            {
                if (entity.Status == Status.Subscribed)
                {
                    return ResultData(null, false, "您已经订阅过了，无需再重复订阅！");
                }
                entity.ValidateCode = guid.ToString();
                entity.UpdateTime = DateTime.Now;
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
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客订阅：" + Request.Host, System.IO.File.ReadAllText(HostEnvironment.WebRootPath + "/template/subscribe.html").Replace("{{link}}", link), email));
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
            Broadcast c = BroadcastService.Get(b => b.Email.Equals(email) && b.Status == Status.Subscribed);
            if (c == null)
            {
                return Content("您输入的邮箱没有订阅本站更新，或者已经取消订阅了");
            }

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
            Broadcast entity = BroadcastService.Get(b => b.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) && b.ValidateCode.Equals(validate));
            if (entity == null)
            {
                return Content("该邮箱账户未使用邮件订阅！");
            }

            switch (act)
            {
                case "verify":
                    entity.Status = Status.Subscribed;
                    entity.ValidateCode = Guid.NewGuid().ToString();
                    entity.UpdateTime = DateTime.Now;
                    BroadcastService.SaveChanges();
                    return Content("订阅成功！");
                case "cancel":
                    entity.Status = Status.Canceled;
                    entity.UpdateTime = DateTime.Now;
                    BroadcastService.SaveChanges();
                    return Content("取消订阅成功，您将不会再接收到文章更新，如果您以后需要再次接收更新推送，可以到主站点重新进行订阅操作！");
                default: return RedirectToAction("Index", "Home");
            }
        }


        #region 管理端

        /// <summary>
        /// 保存订阅
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult Save(Broadcast model)
        {
            model.UpdateTime = DateTime.Now;
            var entry = BroadcastService.Get(c => c.Email.Equals(model.Email));
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
                b = BroadcastService.SaveChanges() > 0;
            }
            return ResultData(model, b, b ? "更新订阅成功！" : "更新订阅失败！");
        }

        /// <summary>
        /// 删除订阅
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
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
        [MyAuthorize]
        public ActionResult Change(int id)
        {
            Broadcast cast = BroadcastService.GetById(id);
            Status status = cast.Status;
            cast.UpdateTime = DateTime.Now;
            cast.Status = status == Status.Subscribed ? Status.Subscribing : Status.Subscribed;
            bool b = BroadcastService.SaveChanges() > 0;
            return ResultData(null, b, status == Status.Subscribed ? "订阅成功" : "取消订阅成功！");
        }

        /// <summary>
        /// 获取订阅
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult Get(int id) => ResultData(BroadcastService.GetById(id));

        /// <summary>
        /// 订阅分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult GetPageData(int page = 1, int size = 10, string search = "")
        {
            Expression<Func<Broadcast, bool>> where = b => true;
            if (!string.IsNullOrEmpty(search))
            {
                where = where.And(b => b.Email.Contains(search));
            }

            var list = BroadcastService.GetPagesFromCache(page, size, out var total, @where, b => b.UpdateTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return Ok(new PageDataModel(list, pageCount, total));
        }

        #endregion
    }
}