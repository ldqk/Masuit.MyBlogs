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
using Masuit.Tools.Core.Net;
using Masuit.Tools.Linq;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
        [HttpGet, ResponseCache(Duration = 600, VaryByHeader = "Cookie", Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult> Index([FromServices] IFastShareService fastShareService)
        {
            var banners = AdsService.GetsByWeightedPrice(8, AdvertiseType.Banner, Request.Location()).OrderBy(a => Guid.NewGuid()).ToList();
            var fastShares = await fastShareService.GetAllFromCacheAsync(s => s.Sort);
            var postsQuery = PostService.GetQuery(p => p.Status == Status.Published); //准备文章的查询
            var posts = await postsQuery.Where(p => !p.IsFixedTop).OrderBy(OrderBy.ModifyDate.GetDisplay() + " desc").ToCachedPagedListAsync<Post, PostDto>(1, 15, MapperConfig);
            posts.Data.InsertRange(0, postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).ProjectTo<PostDto>(MapperConfig).ToList());
            CheckPermission(posts.Data);
            var viewModel = await GetIndexPageViewModel();
            viewModel.Banner = banners;
            viewModel.Posts = posts;
            ViewBag.FastShare = fastShares;
            viewModel.PageParams = new Pagination(1, 15, posts.TotalCount, OrderBy.ModifyDate);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.PostList, Request.Location());
            return View(viewModel);
        }

        /// <summary>
        /// 文章列表页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("posts"), Route("p", Order = 1), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Post([Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var viewModel = await GetIndexPageViewModel();
            var postsQuery = PostService.GetQuery(p => p.Status == Status.Published); //准备文章的查询
            var posts = await postsQuery.Where(p => !p.IsFixedTop).OrderBy((orderBy ?? OrderBy.ModifyDate).GetDisplay() + " desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig);
            if (page == 1)
            {
                posts.Data.InsertRange(0, postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).ProjectTo<PostDto>(MapperConfig).ToList());
            }

            CheckPermission(posts.Data);
            viewModel.Posts = posts;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.PostList, Request.Location());
            return View(viewModel);
        }

        /// <summary>
        /// 标签文章页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("tag/{id}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Tag(string id, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var posts = await PostService.GetQuery(p => p.Label.Contains(id) && p.Status == Status.Published).OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig);
            CheckPermission(posts.Data);
            var viewModel = await GetIndexPageViewModel();
            ViewBag.Tag = id;
            viewModel.Posts = posts;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.PostList, Request.Location());
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
        [Route("author/{author}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Author(string author, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            Expression<Func<Post, bool>> where = p => p.Author.Equals(author) || p.Modifier.Equals(author) || p.Email.Equals(author) || p.PostHistoryVersion.Any(v => v.Modifier.Equals(author) || v.ModifierEmail.Equals(author));
            where = where.And(p => p.Status == Status.Published);
            var posts = await PostService.GetQuery(where).OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig);
            CheckPermission(posts.Data);
            var viewModel = await GetIndexPageViewModel();
            ViewBag.Author = author;
            ViewBag.Total = posts.TotalCount;
            viewModel.Posts = posts;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.PostList, Request.Location());
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
        [Route("cat/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Category(int id, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var cat = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("文章分类未找到");
            var posts = await PostService.GetQuery(p => p.CategoryId == cat.Id && p.Status == Status.Published).OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig);
            CheckPermission(posts.Data);
            var viewModel = await GetIndexPageViewModel();
            viewModel.Posts = posts;
            ViewBag.Category = cat;
            viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
            viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location(), id);
            viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.PostList, Request.Location(), id);
            return View(viewModel);
        }

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

        private void CheckPermission(List<PostDto> posts)
        {
            var location = Request.Location() + "|" + Request.Headers[HeaderNames.UserAgent];
            posts.RemoveAll(p =>
            {
                switch (p.LimitMode)
                {
                    case RegionLimitMode.AllowRegion:
                        return !location.Contains(p.Regions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid && !Request.IsRobot();
                    case RegionLimitMode.ForbidRegion:
                        return location.Contains(p.Regions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid && !Request.IsRobot();
                    case RegionLimitMode.AllowRegionExceptForbidRegion:
                        if (location.Contains(p.ExceptRegions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid)
                        {
                            return true;
                        }

                        goto case RegionLimitMode.AllowRegion;
                    case RegionLimitMode.ForbidRegionExceptAllowRegion:
                        if (location.Contains(p.ExceptRegions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid)
                        {
                            return false;
                        }

                        goto case RegionLimitMode.ForbidRegion;
                    default:
                        return false;
                }
            });
            foreach (var item in posts)
            {
                item.PostDate = item.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }
        }

        /// <summary>
        /// 获取页面视图模型
        /// </summary>
        /// <returns></returns>
        private async Task<HomePageViewModel> GetIndexPageViewModel()
        {
            var postsQuery = PostService.GetQuery<PostDto>(p => p.Status == Status.Published); //准备文章的查询
            var notices = await NoticeService.GetPagesFromCacheAsync<DateTime, NoticeDto>(1, 5, n => n.NoticeStatus == NoticeStatus.Normal, n => n.ModifyDate, false); //加载前5条公告
            var cats = await CategoryService.GetQueryFromCacheAsync<string, CategoryDto>(c => c.Status == Status.Available, c => c.Name); //加载分类目录
            var hotSearches = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Week").Take(10).ToList(); //热词统计
            var hot6Post = postsQuery.OrderBy((new Random().Next() % 3) switch
            {
                1 => nameof(OrderBy.VoteUpCount),
                2 => nameof(OrderBy.AverageViewCount),
                _ => nameof(OrderBy.TotalViewCount)
            } + " desc").Skip(0).Take(5).Cacheable().ToList(); //热门文章
            CheckPermission(hot6Post);
            var newdic = new Dictionary<string, int>(); //标签云最终结果
            var tagdic = postsQuery.Where(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).Distinct().Cacheable().ToList().SelectMany(s => s.Split(',', '，')).GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count()); //统计标签

            if (tagdic.Any())
            {
                var min = tagdic.Values.Min();
                foreach (var (key, value) in tagdic)
                {
                    var fontsize = (int)Math.Floor(value * 1.0 / (min * 1.0) + 12.0);
                    newdic.Add(key, fontsize >= 36 ? 36 : fontsize);
                }
            }

            return new HomePageViewModel()
            {
                Categories = cats,
                HotSearch = hotSearches,
                Notices = notices.Data,
                Tags = newdic,
                Top6Post = hot6Post,
                PostsQueryable = postsQuery
            };
        }
    }
}