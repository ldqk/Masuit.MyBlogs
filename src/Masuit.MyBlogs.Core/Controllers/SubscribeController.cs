using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WilderMinds.RssSyndication;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 订阅服务
    /// </summary>
    public class SubscribeController : Controller
    {
        /// <summary>
        /// 文章
        /// </summary>
        public IPostService PostService { get; set; }
        public ICategoryService CategoryService { get; set; }
        public ICommentService CommentService { get; set; }

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
        [Route("/rss"), ResponseCache(Duration = 3600)]
        public async Task<IActionResult> Rss()
        {
            var time = DateTime.Today.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var posts = await PostService.GetQueryNoTracking(p => p.Status == Status.Published && p.ModifyDate >= time, p => p.ModifyDate, false).Select(p => new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email.MaskEmail('*')
                },
                Body = p.Content.GetSummary(300, 50),
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone)),
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = p.Content.GetSummary(300, 50)
            }).FromCacheAsync(new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
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
        [Route("/cat/{id}/rss"), ResponseCache(Duration = 3600)]
        public async Task<IActionResult> CategoryRss(int id)
        {
            var time = DateTime.Today.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var category = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("分类未找到");
            var posts = await PostService.GetQueryNoTracking(p => p.CategoryId == id && p.Status == Status.Published && p.ModifyDate >= time, p => p.ModifyDate, false).Select(p => new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email.MaskEmail('*')
                },
                Body = p.Content.GetSummary(300, 50),
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone)),
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = p.Content.GetSummary(300, 50)
            }).FromCacheAsync(new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
            var feed = new Feed()
            {
                Title = Request.Host + $":分类{category.Name}文章订阅",
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
        [Route("/{id}/rss"), ResponseCache(Duration = 3600)]
        public async Task<IActionResult> PostRss(int id)
        {
            string scheme = Request.Scheme;
            var host = Request.Host;
            var p = await PostService.GetAsync(p => p.Status == Status.Published && p.Id == id) ?? throw new NotFoundException("文章未找到");
            var summary = p.Content.GetSummary(300, 50);
            var item = new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email.MaskEmail()
                },
                Body = summary,
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone)),
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = summary
            };
            var feed = new Feed()
            {
                Title = Request.Host + $":文章【{p.Title}】更新订阅",
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
        public async Task<IActionResult> CommentsRss(int id)
        {
            string scheme = Request.Scheme;
            var host = Request.Host;
            var post = await PostService.GetAsync(p => p.Status == Status.Published && p.Id == id) ?? throw new NotFoundException("文章不存在");
            var start = DateTime.Today.AddDays(-7);
            var comments = await CommentService.GetQuery(c => c.PostId == post.Id && c.CommentDate > start).Select(c => new Item()
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
                PublishDate = c.CommentDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone)),
                Title = c.NickName,
                Permalink = $"{scheme}://{host}/{post.Id}?cid={c.Id}#comment",
                Guid = c.Id.ToString(),
                FullHtmlContent = c.Content
            }).FromCacheAsync(new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
            var feed = new Feed()
            {
                Title = Request.Host + $":文章【{post.Title}】文章评论更新订阅",
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
    }
}