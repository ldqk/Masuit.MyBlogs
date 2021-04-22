using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WilderMinds.RssSyndication;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 订阅服务
    /// </summary>
    public class SubscribeController : Controller
    {
        public IPostService PostService { get; set; }
        public IAdvertisementService AdvertisementService { get; set; }

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
            var data = await PostService.GetQueryFromCache(p => p.Status == Status.Published && p.ModifyDate >= time, p => p.ModifyDate, false).SelectAsync(async p => new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email.MaskEmail()
                },
                Body = await p.Content.GetSummary(300, 50),
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone)),
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = await p.Content.GetSummary(300, 50)
            });
            var posts = data.ToList();
            InsertAdvertisement(posts);
            var feed = new Feed()
            {
                Title = CommonHelper.SystemSettings["Title"],
                Description = CommonHelper.SystemSettings["Description"],
                Link = new Uri(scheme + "://" + host + "/rss"),
                Copyright = CommonHelper.SystemSettings["Title"],
                Language = "zh-cn",
                Items = posts
            };
            var rss = feed.Serialize(new SerializeOption()
            {
                Encoding = Encoding.UTF8
            });
            return Content(rss, "text/xml");
        }

        private void InsertAdvertisement(List<Item> posts, int? cid = null)
        {
            if (posts.Count > 2)
            {
                var ad = AdvertisementService.GetByWeightedPrice((AdvertiseType)(DateTime.Now.Second % 4 + 1), cid);
                if (ad is not null)
                {
                    posts.Insert(new Random().Next(1, posts.Count), new Item()
                    {
                        Author = new Author()
                        {
                            Name = ad.IndexId
                        },
                        Body = ad.Description,
                        Title = ad.Title,
                        FullHtmlContent = ad.Description,
                        Guid = ad.IndexId,
                        PublishDate = DateTime.UtcNow,
                        Link = new Uri(Url.ActionLink("Redirect", "Advertisement", new { id = ad.Id })),
                        Permalink = Url.ActionLink("Redirect", "Advertisement", new { id = ad.Id })
                    });
                }
            }
        }

        /// <summary>
        /// RSS分类订阅
        /// </summary>
        /// <returns></returns>
        [Route("/cat/{id}/rss"), ResponseCache(Duration = 3600)]
        public async Task<IActionResult> CategoryRss([FromServices] ICategoryService categoryService, int id)
        {
            var time = DateTime.Today.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var category = await categoryService.GetByIdAsync(id) ?? throw new NotFoundException("分类未找到");
            var data = await PostService.GetQueryFromCache(p => p.CategoryId == id && p.Status == Status.Published && p.ModifyDate >= time, p => p.ModifyDate, false).SelectAsync(async p => new Item()
            {
                Author = new Author
                {
                    Name = p.Author,
                    Email = p.Email.MaskEmail()
                },
                Body = await p.Content.GetSummary(300, 50),
                Categories = new List<string>
                {
                    p.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + p.Id),
                PublishDate = p.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone)),
                Title = p.Title,
                Permalink = scheme + "://" + host + "/" + p.Id,
                Guid = p.Id.ToString(),
                FullHtmlContent = await p.Content.GetSummary(300, 50)
            });
            var posts = data.ToList();
            InsertAdvertisement(posts, id);
            var feed = new Feed()
            {
                Title = Request.Host + $":分类{category.Name}文章订阅",
                Description = category.Description,
                Link = new Uri(scheme + "://" + host + "/rss"),
                Copyright = CommonHelper.SystemSettings["Title"],
                Language = "zh-cn",
                Items = posts
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
            var summary = await p.Content.GetSummary(300, 50);
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
    }
}