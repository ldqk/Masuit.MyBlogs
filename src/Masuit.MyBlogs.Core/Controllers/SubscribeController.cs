using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
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
            var raw = PostService.GetQueryFromCache(PostBaseWhere().And(p => p.Rss && p.Status == Status.Published && p.ModifyDate >= time), p => p.ModifyDate, false).ToList();
            var data = await raw.SelectAsync(async p =>
            {
                var summary = await p.Content.GetSummary(300, 50);
                return new Item()
                {
                    Author = new Author
                    {
                        Name = p.Modifier
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
            return Content(rss, ContentType.Xml);
        }

        private void InsertAdvertisement(List<Item> posts, int? cid = null)
        {
            if (posts.Count > 2)
            {
                var ad = AdvertisementService.GetByWeightedPrice((AdvertiseType)(DateTime.Now.Second % 4 + 1), Request.Location(), cid);
                if (ad is not null)
                {
                    posts.Insert(new Random().Next(1, posts.Count), new Item()
                    {
                        Author = new Author()
                        {
                            Name = ad.Title
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
            var raw = PostService.GetQueryFromCache(PostBaseWhere().And(p => p.Rss && p.CategoryId == id && p.Status == Status.Published && p.ModifyDate >= time), p => p.ModifyDate, false).ToList();
            var data = await raw.SelectAsync(async p =>
            {
                var summary = await p.Content.GetSummary(300, 50);
                return new Item()
                {
                    Author = new Author
                    {
                        Name = p.Modifier
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
            return Content(rss, ContentType.Xml);
        }

        /// <summary>
        /// RSS专题订阅
        /// </summary>
        /// <returns></returns>
        [Route("/special/{id}/rss"), ResponseCache(Duration = 3600)]
        public async Task<IActionResult> SeminarRss([FromServices] ISeminarService seminarService, int id)
        {
            var time = DateTime.Today.AddDays(-1);
            string scheme = Request.Scheme;
            var host = Request.Host;
            var seminar = await seminarService.GetByIdAsync(id) ?? throw new NotFoundException("专题未找到");
            var raw = PostService.GetQueryFromCache(PostBaseWhere().And(p => p.Rss && p.Seminar.Any(s => s.Id == id) && p.Status == Status.Published && p.ModifyDate >= time), p => p.ModifyDate, false).ToList();
            var data = await raw.SelectAsync(async p =>
            {
                var summary = await p.Content.GetSummary(300, 50);
                return new Item()
                {
                    Author = new Author
                    {
                        Name = p.Modifier
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
            });
            var posts = data.ToList();
            InsertAdvertisement(posts, id);
            var feed = new Feed()
            {
                Title = Request.Host + $":专题{seminar.Title}文章订阅",
                Description = seminar.Description,
                Link = new Uri(scheme + "://" + host + "/rss"),
                Copyright = CommonHelper.SystemSettings["Title"],
                Language = "zh-cn",
                Items = posts
            };
            var rss = feed.Serialize(new SerializeOption()
            {
                Encoding = Encoding.UTF8
            });
            return Content(rss, ContentType.Xml);
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
            var post = await PostService.GetAsync(p => p.Rss && p.Status == Status.Published && p.Id == id) ?? throw new NotFoundException("文章未找到");
            CheckPermission(post);
            var summary = await post.Content.GetSummary(300, 50);
            var item = new Item()
            {
                Author = new Author
                {
                    Name = post.Modifier
                },
                Body = summary,
                Categories = new List<string>
                {
                    post.Category.Name
                },
                Link = new Uri(scheme + "://" + host + "/" + post.Id),
                PublishDate = post.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone)),
                Title = post.Title,
                Permalink = scheme + "://" + host + "/" + post.Id,
                Guid = post.Id.ToString(),
                FullHtmlContent = summary
            };
            var feed = new Feed()
            {
                Title = Request.Host + $":文章【{post.Title}】更新订阅",
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
            return Content(rss, ContentType.Xml);
        }

        protected Expression<Func<Post, bool>> PostBaseWhere()
        {
            var location = Request.Location() + "|" + Request.Headers[HeaderNames.Referer] + "|" + Request.Headers[HeaderNames.UserAgent];
            return p => p.LimitMode == null || p.LimitMode == RegionLimitMode.All ? true :
                   p.LimitMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, p.Regions) :
                   p.LimitMode == RegionLimitMode.ForbidRegion ? !Regex.IsMatch(location, p.Regions) :
                   p.LimitMode == RegionLimitMode.AllowRegionExceptForbidRegion ? Regex.IsMatch(location, p.Regions) && !Regex.IsMatch(location, p.ExceptRegions) : !Regex.IsMatch(location, p.Regions) || Regex.IsMatch(location, p.ExceptRegions);

        }

        private void CheckPermission(Post post)
        {
            var location = Request.Location() + "|" + Request.Headers[HeaderNames.Referer] + "|" + Request.Headers[HeaderNames.UserAgent];
            switch (post.LimitMode)
            {
                case RegionLimitMode.AllowRegion:
                    if (!Regex.IsMatch(location, post.Regions) && !Request.IsRobot())
                    {
                        Disallow(post);
                    }

                    break;
                case RegionLimitMode.ForbidRegion:
                    if (Regex.IsMatch(location, post.Regions) && !Request.IsRobot())
                    {
                        Disallow(post);
                    }

                    break;
                case RegionLimitMode.AllowRegionExceptForbidRegion:
                    if (Regex.IsMatch(location, post.ExceptRegions))
                    {
                        Disallow(post);
                    }

                    goto case RegionLimitMode.AllowRegion;
                case RegionLimitMode.ForbidRegionExceptAllowRegion:
                    if (Regex.IsMatch(location, post.ExceptRegions))
                    {
                        break;
                    }

                    goto case RegionLimitMode.ForbidRegion;
            }
        }

        private void Disallow(Post post)
        {
            RedisHelper.IncrBy("interceptCount");
            RedisHelper.LPush("intercept", new IpIntercepter()
            {
                IP = HttpContext.Connection.RemoteIpAddress.ToString(),
                RequestUrl = $"//{Request.Host}/{post.Id}",
                Referer = Request.Headers[HeaderNames.Referer],
                Time = DateTime.Now,
                UserAgent = Request.Headers[HeaderNames.UserAgent],
                Remark = "无权限查看该文章",
                Address = Request.Location(),
                HttpVersion = Request.Protocol,
                Headers = Request.Headers.ToJsonString()
            });
            throw new NotFoundException("文章未找到");
        }

    }
}