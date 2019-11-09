using AutoMapper.QueryableExtensions;
using EFSecondLevelCache.Core;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
            ViewBag.FastShare = fastShares;
            var viewModel = GetIndexPageViewModel(1, 15, OrderBy.ModifyDate, CurrentUser);
            viewModel.Banner = banners;
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
            var viewModel = GetIndexPageViewModel(page, size, orderBy, CurrentUser);
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
            IList<PostOutputDto> posts;
            var temp = PostService.GetQuery<PostOutputDto>(p => p.Label.Contains(id) && (p.Status == Status.Pended || CurrentUser.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
            switch (orderBy)
            {
                case OrderBy.CommentCount:
                    posts = temp.ThenByDescending(p => p.CommentCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.PostDate:
                    posts = temp.ThenByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = temp.ThenByDescending(p => p.TotalViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = temp.ThenByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.AverageViewCount:
                    posts = temp.ThenByDescending(p => p.AverageViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                default:
                    posts = temp.ThenByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
            }
            var viewModel = GetIndexPageViewModel(1, 1, orderBy, CurrentUser);
            ViewBag.Total = temp.Count();
            ViewBag.Tag = id;
            viewModel.Posts = posts;
            return View(viewModel);
        }

        /// <summary>
        /// 作者文章页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("author/{author}/{page:int?}/{size:int?}/{orderBy:int?}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "author", "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public ActionResult Author(string author, [Optional]OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")]int size = 15)
        {
            IList<PostOutputDto> posts;
            Expression<Func<Post, bool>> where = p => p.Author.Equals(author) || p.Modifier.Equals(author) || p.Email.Equals(author) || p.PostHistoryVersion.Any(v => v.Modifier.Equals(author) || v.ModifierEmail.Equals(author));
            where = where.And(p => p.Status == Status.Pended || CurrentUser.IsAdmin);
            var temp = PostService.GetQuery<PostOutputDto>(where).OrderByDescending(p => p.IsFixedTop);
            switch (orderBy)
            {
                case OrderBy.CommentCount:
                    posts = temp.ThenByDescending(p => p.CommentCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.PostDate:
                    posts = temp.ThenByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = temp.ThenByDescending(p => p.TotalViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = temp.ThenByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.AverageViewCount:
                    posts = temp.ThenByDescending(p => p.AverageViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                default:
                    posts = temp.ThenByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
            }
            var viewModel = GetIndexPageViewModel(1, 1, orderBy, CurrentUser);
            ViewBag.Total = temp.Count();
            ViewBag.Author = author;
            viewModel.Posts = posts;
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
            var posts = PostService.GetQueryNoTracking(p => p.CategoryId == cat.Id && (p.Status == Status.Pended || CurrentUser.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
            ViewBag.Total = posts.Count();
            switch (orderBy)
            {
                case OrderBy.CommentCount:
                    posts = posts.ThenByDescending(p => p.Comment.Count);
                    break;
                case OrderBy.PostDate:
                    posts = posts.ThenByDescending(p => p.PostDate);
                    break;
                case OrderBy.ViewCount:
                    posts = posts.ThenByDescending(p => p.TotalViewCount);
                    break;
                case OrderBy.VoteCount:
                    posts = posts.ThenByDescending(p => p.VoteUpCount);
                    break;
                case OrderBy.AverageViewCount:
                    posts = posts.ThenByDescending(p => p.AverageViewCount);
                    break;
                default:
                    posts = posts.ThenByDescending(p => p.ModifyDate);
                    break;
            }
            var viewModel = GetIndexPageViewModel(1, 1, orderBy, CurrentUser);
            ViewBag.CategoryName = cat.Name;
            ViewBag.Desc = cat.Description;
            viewModel.Posts = posts.Skip(size * (page - 1)).Take(size).ProjectTo<PostOutputDto>(MapperConfig).Cacheable().ToList();
            return View(viewModel);
        }

        /// <summary>
        /// 获取页面视图模型
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private IndexPageViewModel GetIndexPageViewModel(int page, int size, OrderBy? orderBy, UserInfoOutputDto user)
        {
            var postsQuery = PostService.GetQuery<PostOutputDto>(p => (p.Status == Status.Pended || user.IsAdmin)); //准备文章的查询
            var notices = NoticeService.GetPagesFromCache<DateTime, NoticeOutputDto>(1, 5, out int _, n => (n.Status == Status.Display || user.IsAdmin), n => n.ModifyDate, false).ToList(); //加载前5条公告
            var cats = CategoryService.GetQueryFromCache<string, CategoryOutputDto>(c => c.Status == Status.Available, c => c.Name).ToList(); //加载分类目录
            var hotSearches = RedisHelper.Get<List<KeywordsRankOutputDto>>("SearchRank:Week").Take(10).ToList(); //热词统计
            Expression<Func<PostOutputDto, double>> order = p => p.TotalViewCount;
            switch (new Random().Next() % 3)
            {
                case 1:
                    order = p => p.VoteUpCount;
                    break;
                case 2:
                    order = p => p.AverageViewCount;
                    break;
            }
            var hot6Post = postsQuery.OrderByDescending(order).Skip(0).Take(5).Cacheable().ToList(); //热门文章
            var newdic = new Dictionary<string, int>(); //标签云最终结果
            var tagdic = postsQuery.Where(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).Cacheable().AsEnumerable().SelectMany(s => s.Split(',', '，')).GroupBy(s => s).ToDictionary(g => g.Key, g => g.Count()); //统计标签

            if (tagdic.Any())
            {
                int min = tagdic.Values.Min();
                tagdic.ForEach(kv =>
                {
                    var fontsize = (int)Math.Floor(kv.Value * 1.0 / (min * 1.0) + 12.0);
                    newdic.Add(kv.Key, fontsize >= 36 ? 36 : fontsize);
                });
            }
            IList<PostOutputDto> posts;
            switch (orderBy) //文章排序
            {
                case OrderBy.CommentCount:
                    posts = postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.CommentCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.PostDate:
                    posts = postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.TotalViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.AverageViewCount:
                    posts = postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.AverageViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                default:
                    posts = postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
            }
            if (page == 1)
            {
                posts = postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).AsEnumerable().Union(posts).ToList();
            }
            return new IndexPageViewModel()
            {
                Categories = cats,
                HotSearch = hotSearches,
                Notices = notices,
                Posts = posts,
                Tags = newdic,
                Top6Post = hot6Post,
                PostsQueryable = postsQuery,
                SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar),
                ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.PostList)
            };
        }
    }
}