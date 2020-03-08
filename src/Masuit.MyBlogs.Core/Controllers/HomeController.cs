using EFSecondLevelCache.Core;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc;
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
        /// 快速分享
        /// </summary>
        public IFastShareService FastShareService { get; set; }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        [HttpGet, ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "orderBy" }, VaryByHeader = "Cookie")]
        public ActionResult Index()
        {
            ViewBag.Total = PostService.Count(p => p.Status == Status.Pended || CurrentUser.IsAdmin);
            var banners = AdsService.GetsByWeightedPrice(8, AdvertiseType.Banner).OrderBy(a => Guid.NewGuid()).ToList();
            var fastShares = FastShareService.GetAllFromCache(s => s.Sort).ToList();
            var postsQuery = PostService.GetQuery<PostDto>(p => (p.Status == Status.Pended || CurrentUser.IsAdmin)); //准备文章的查询
            var posts = postsQuery.Where(p => !p.IsFixedTop).OrderBy(OrderBy.ModifyDate.GetDisplay() + " desc").Take(15).Cacheable().ToList();
            var viewModel = GetIndexPageViewModel();
            viewModel.Banner = banners;
            viewModel.Posts = Enumerable.AsEnumerable(postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate)).Union(posts).ToList();
            ViewBag.FastShare = fastShares;
            ViewData["page"] = new Pagination(1, 15, OrderBy.ModifyDate);
            return View(viewModel);
        }

        /// <summary>
        /// 文章列表页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("p"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public ActionResult Post([Optional]OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")]int size = 15)
        {
            ViewBag.Total = PostService.Count(p => p.Status == Status.Pended || CurrentUser.IsAdmin && !p.IsFixedTop);
            var viewModel = GetIndexPageViewModel();
            var postsQuery = PostService.GetQuery<PostDto>(p => (p.Status == Status.Pended || CurrentUser.IsAdmin)); //准备文章的查询
            var posts = postsQuery.Where(p => !p.IsFixedTop).OrderBy((orderBy ?? OrderBy.ModifyDate).GetDisplay() + " desc").Skip(size * (page - 1)).Take(size).Cacheable().ToList();
            if (page == 1)
            {
                posts = Enumerable.AsEnumerable(postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate)).Union(posts).ToList();
            }

            viewModel.Posts = posts;
            ViewData["page"] = new Pagination(page, size, orderBy);
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
        [Route("tag/{id}/{page:int?}/{size:int?}/{orderBy:int?}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public ActionResult Tag(string id, [Optional]OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")]int size = 15)
        {
            var temp = PostService.GetQuery<PostDto>(p => p.Label.Contains(id) && (p.Status == Status.Pended || CurrentUser.IsAdmin));
            var posts = temp.OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").Skip(size * (page - 1)).Take(size).Cacheable().ToList();
            var viewModel = GetIndexPageViewModel();
            ViewBag.Total = temp.Count();
            ViewBag.Tag = id;
            viewModel.Posts = posts;
            ViewData["page"] = new Pagination(page, size, orderBy);
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
        [Route("author/{author}/{page:int?}/{size:int?}/{orderBy:int?}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "author", "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public ActionResult Author(string author, [Optional]OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")]int size = 15)
        {
            Expression<Func<Post, bool>> where = p => p.Author.Equals(author) || p.Modifier.Equals(author) || p.Email.Equals(author) || p.PostHistoryVersion.Any(v => v.Modifier.Equals(author) || v.ModifierEmail.Equals(author));
            where = where.And(p => p.Status == Status.Pended || CurrentUser.IsAdmin);
            var temp = PostService.GetQuery<PostDto>(where);
            var posts = temp.OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").Skip(size * (page - 1)).Take(size).Cacheable().ToList();
            var viewModel = GetIndexPageViewModel();
            ViewBag.Total = temp.Count();
            ViewBag.Author = author;
            viewModel.Posts = posts;
            ViewData["page"] = new Pagination(page, size, orderBy);
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
        [Route("cat/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        [Route("cat/{id:int}/{page:int?}/{size:int?}/{orderBy:int?}")]
        public async Task<ActionResult> Category(int id, [Optional]OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")]int size = 15)
        {
            var cat = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("文章分类未找到");
            var posts = PostService.GetQuery<PostDto>(p => p.CategoryId == cat.Id && (p.Status == Status.Pended || CurrentUser.IsAdmin));
            ViewBag.Total = posts.Count();
            var viewModel = GetIndexPageViewModel();
            viewModel.Posts = posts.OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").Skip(size * (page - 1)).Take(size).Cacheable().ToList();
            ViewBag.Category = cat;
            ViewData["page"] = new Pagination(page, size, orderBy);
            return View(viewModel);
        }

        /// <summary>
        /// 获取页面视图模型
        /// </summary>
        /// <returns></returns>
        private HomePageViewModel GetIndexPageViewModel()
        {
            var postsQuery = PostService.GetQuery<PostDto>(p => (p.Status == Status.Pended || CurrentUser.IsAdmin)); //准备文章的查询
            var notices = NoticeService.GetPagesFromCache<DateTime, NoticeDto>(1, 5, out _, n => (n.Status == Status.Display || CurrentUser.IsAdmin), n => n.ModifyDate, false).ToList(); //加载前5条公告
            var cats = CategoryService.GetQueryFromCache<string, CategoryDto>(c => c.Status == Status.Available, c => c.Name).ToList(); //加载分类目录
            var hotSearches = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Week").Take(10).ToList(); //热词统计
            var hot6Post = postsQuery.OrderBy((new Random().Next() % 3) switch
            {
                1 => nameof(OrderBy.VoteUpCount),
                2 => nameof(OrderBy.AverageViewCount),
                _ => nameof(OrderBy.TotalViewCount)
            } + " desc").Skip(0).Take(5).Cacheable().ToList(); //热门文章
            var newdic = new Dictionary<string, int>(); //标签云最终结果
            var tagdic = postsQuery.Where(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).Distinct().Cacheable().AsParallel().SelectMany(s => s.Split(',', '，')).GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count()); //统计标签

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
                Notices = notices,
                Tags = newdic,
                Top6Post = hot6Post,
                PostsQueryable = postsQuery,
                SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar),
                ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.PostList)
            };
        }
    }
}