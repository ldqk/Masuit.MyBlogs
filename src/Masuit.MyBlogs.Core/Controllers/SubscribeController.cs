using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
        [Route("/rss"), ResponseCache(Duration = 600)]
        public IActionResult Rss()
        {
            var time = DateTime.Today.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var posts = PostService.GetQueryNoTracking(p => p.Status == Status.Published && p.ModifyDate >= time, p => p.ModifyDate, false).Select(p => new Item()
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
                PublishDate = p.ModifyDate.ToUniversalTime(),
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = p.Content.GetSummary(300, 50)
            }).FromCache().ToList();
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
            var posts = PostService.GetQueryNoTracking(p => p.CategoryId == id && p.Status == Status.Published && p.ModifyDate >= time, p => p.ModifyDate, false).Select(p => new Item()
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
                PublishDate = p.ModifyDate.ToUniversalTime(),
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = p.Content.GetSummary(300, 50)
            }).FromCache().ToList();
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
        [Route("/{id}/rss"), ResponseCache(Duration = 600)]
        public IActionResult PostRss(int id)
        {
            string scheme = Request.Scheme;
            var host = Request.Host;
            var p = PostService.Get(p => p.Status == Status.Published && p.Id == id) ?? throw new NotFoundException("文章未找到");
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
                PublishDate = p.ModifyDate.ToUniversalTime(),
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
        public IActionResult CommentsRss(int id)
        {
            string scheme = Request.Scheme;
            var host = Request.Host;
            var post = PostService.Get(p => p.Status == Status.Published && p.Id == id) ?? throw new NotFoundException("文章不存在");
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
                PublishDate = c.CommentDate.ToUniversalTime(),
                Title = c.NickName,
                Permalink = $"{scheme}://{host}/{post.Id}?cid={c.Id}#comment",
                Guid = c.Id.ToString(),
                FullHtmlContent = c.Content
            }).ToList();
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