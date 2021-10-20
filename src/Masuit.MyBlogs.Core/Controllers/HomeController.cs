using AngleSharp;
using AutoMapper.QueryableExtensions;
using EFCoreSecondLevelCacheInterceptor;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Repository;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Linq;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 首页
    /// </summary>
    public class HomeController : BaseController
    {
        /// <summary>
        /// 文章
        /// </summary>
        public IPostService PostService { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public ICategoryService CategoryService { get; set; }

        /// <summary>
        /// 网站公告
        /// </summary>
        public INoticeService NoticeService { get; set; }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        [HttpGet, ResponseCache(Duration = 600, VaryByHeader = nameof(HeaderNames.Cookie))]
        public async Task<ActionResult> Index([FromServices] IFastShareService fastShareService)
        {
            var banners = AdsService.GetsByWeightedPrice(8, AdvertiseType.Banner, Request.Location()).OrderByRandom().ToList();
            var fastShares = await fastShareService.GetAllFromCacheAsync(s => s.Sort);
            var postsQuery = PostService.GetQuery(PostBaseWhere().And(p => p.Status == Status.Published)); //准备文章的查询
            var posts = await postsQuery.Where(p => !p.IsFixedTop).OrderBy(OrderBy.ModifyDate.GetDisplay() + " desc").ToCachedPagedListAsync<Post, PostDto>(1, 15, MapperConfig);
            posts.Data.InsertRange(0, postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).ProjectTo<PostDto>(MapperConfig).Cacheable().ToList());
            var viewModel = await GetIndexPageViewModel();
            viewModel.Banner = banners;
            viewModel.Posts = posts;
            ViewBag.FastShare = fastShares;
            viewModel.PageParams = new Pagination(1, 15, posts.TotalCount, OrderBy.ModifyDate);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
            foreach (var item in posts.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return View(viewModel);
        }

        /// <summary>
        /// 文章列表页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("posts"), Route("p", Order = 1), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = nameof(HeaderNames.Cookie))]
        public async Task<ActionResult> Post([Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var viewModel = await GetIndexPageViewModel();
            var postsQuery = PostService.GetQuery(PostBaseWhere().And(p => p.Status == Status.Published)); //准备文章的查询
            var h24 = DateTime.Now.AddDays(-1);
            var posts = orderBy switch
            {
                OrderBy.Trending => await postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.PostVisitRecords.Count(e => e.Time >= h24)).ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig),
                _ => await postsQuery.Where(p => !p.IsFixedTop).OrderBy((orderBy ?? OrderBy.ModifyDate).GetDisplay() + " desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig)
            };
            if (page == 1)
            {
                posts.Data.InsertRange(0, postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).ProjectTo<PostDto>(MapperConfig).ToList());
            }

            viewModel.Posts = posts;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
            foreach (var item in posts.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return View(viewModel);
        }

        /// <summary>
        /// 标签文章页
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("tag/{tag}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = nameof(HeaderNames.Cookie))]
        public async Task<ActionResult> Tag(string tag, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            Expression<Func<Post, bool>> where = PostBaseWhere().And(p => p.Status == Status.Published);
            var queryable = PostService.GetQuery(tag.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Aggregate(@where, (current, s) => current.And(p => Regex.IsMatch(p.Label, s))));
            var h24 = DateTime.Now.AddDays(-1);
            var posts = orderBy switch
            {
                OrderBy.Trending => await queryable.OrderByDescending(p => p.PostVisitRecords.Count(e => e.Time >= h24)).ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig),
                _ => await queryable.OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig)
            };
            var viewModel = await GetIndexPageViewModel();
            ViewBag.Tag = tag;
            viewModel.Posts = posts;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
            foreach (var item in posts.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return View(viewModel);
        }

        /// <summary>
        /// 作者文章页
        /// </summary>
        /// <param name="author"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("author/{author}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = nameof(HeaderNames.Cookie))]
        public async Task<ActionResult> Author(string author, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            Expression<Func<Post, bool>> where = p => p.Author.Equals(author) || p.Modifier.Equals(author) || p.Email.Equals(author) || p.PostHistoryVersion.Any(v => v.Modifier.Equals(author) || v.ModifierEmail.Equals(author));
            where = where.And(p => p.Status == Status.Published).And(PostBaseWhere());
            var h24 = DateTime.Now.AddDays(-1);
            var posts = orderBy switch
            {
                OrderBy.Trending => await PostService.GetQuery(where).OrderByDescending(p => p.PostVisitRecords.Count(e => e.Time >= h24)).ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig),
                _ => await PostService.GetQuery(where).OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig)
            };
            var viewModel = await GetIndexPageViewModel();
            ViewBag.Author = author;
            viewModel.Posts = posts;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
            foreach (var item in posts.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return View(viewModel);
        }

        /// <summary>
        /// 分类文章页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("cat/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = nameof(HeaderNames.Cookie))]
        public async Task<ActionResult> Category(int id, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var cat = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("文章分类未找到");
            var h24 = DateTime.Now.AddDays(-1);
            var posts = orderBy switch
            {
                OrderBy.Trending => await PostService.GetQuery(PostBaseWhere().And(p => p.CategoryId == cat.Id && p.Status == Status.Published)).OrderByDescending(p => p.PostVisitRecords.Count(e => e.Time >= h24)).ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig),
                _ => await PostService.GetQuery(PostBaseWhere().And(p => p.CategoryId == cat.Id && p.Status == Status.Published)).OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig)
            };
            var viewModel = await GetIndexPageViewModel();
            viewModel.Posts = posts;
            ViewBag.Category = cat;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location(), id);
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location(), id);
            foreach (var item in posts.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return View(viewModel);
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [Route("lang/{lang}")]
        public ActionResult SetLang(string lang)
        {
            Response.Cookies.Append("lang", lang, new CookieOptions()
            {
                Expires = DateTime.Now.AddYears(1),
            });
            var referer = Request.Headers[HeaderNames.Referer].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
        }

        /// <summary>
        /// 站点地图
        /// </summary>
        /// <param name="env"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        [HttpGet("/sitemaps.{ext}")]
        public async Task<ActionResult> Sitemap([FromServices] IWebHostEnvironment env, string ext)
        {
            var sitemap = Path.Combine(env.WebRootPath, "sitemap." + ext);
            if (System.IO.File.Exists(sitemap))
            {
                var fs = new FileInfo(sitemap).ShareReadWrite();
                switch (ext)
                {
                    case "txt":
                        return Content((await fs.ReadAllLinesAsync(Encoding.UTF8)).Select(s => Request.Scheme + "://" + Request.Host.Host + new Uri(s).GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped)).Join("\r\n"), ContentType.Txt);
                    case "html":
                        var context = BrowsingContext.New(Configuration.Default);
                        var doc = await context.OpenAsync(req => req.Content(fs.ReadAllText(Encoding.UTF8)));
                        foreach (var e in doc.Body.QuerySelectorAll("li a"))
                        {
                            e.SetAttribute("href", Request.Scheme + "://" + Request.Host.Host + new Uri(e.GetAttribute("href")).GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped));
                        }

                        return Content(doc.DocumentElement.OuterHtml, ContentType.Html);
                }
                return File("/sitemap." + ext, new MimeMapper().GetMimeFromExtension("." + ext));
            }

            return NotFound();
        }

        /// <summary>
        /// 获取页面视图模型
        /// </summary>
        /// <returns></returns>
        private async Task<HomePageViewModel> GetIndexPageViewModel()
        {
            var postsQuery = PostService.GetQuery<PostDto>(PostBaseWhere().And(p => p.Status == Status.Published)); //准备文章的查询
            var notices = await NoticeService.GetPagesFromCacheAsync<DateTime, NoticeDto>(1, 5, n => n.NoticeStatus == NoticeStatus.Normal, n => n.ModifyDate, false); //加载前5条公告
            var cats = await CategoryService.GetQueryFromCacheAsync<string, CategoryDto>(c => c.Status == Status.Available, c => c.Name); //加载分类目录
            var hotSearches = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Week").Take(10).ToList(); //热词统计
            var hot5Post = postsQuery.OrderBy((new Random().Next() % 3) switch
            {
                1 => nameof(OrderBy.VoteUpCount),
                2 => nameof(OrderBy.AverageViewCount),
                _ => nameof(OrderBy.TotalViewCount)
            } + " desc").Skip(0).Take(5).Cacheable().ToList(); //热门文章
            var tagdic = PostService.GetTags().OrderByRandom().Take(20).ToDictionary(x => x.Key, x => x.Value + 12 >= 32 ? 32 : x.Value + 12); //统计标签
            return new HomePageViewModel()
            {
                Categories = cats,
                HotSearch = hotSearches,
                Notices = notices.Data,
                Tags = tagdic,
                Top5Post = hot5Post,
                PostsQueryable = postsQuery
            };
        }
    }
}