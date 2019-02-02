using AutoMapper.QueryableExtensions;
using EFSecondLevelCache.Core;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// 搜索关键词推荐
        /// </summary>
        public ISearchDetailsService SearchDetailsService { get; set; }

        /// <summary>
        /// 网站公告
        /// </summary>
        public INoticeService NoticeService { get; set; }

        /// <summary>
        /// 文章访问统计
        /// </summary>
        public IPostAccessRecordService PostAccessRecordService { get; set; }

        /// <summary>
        /// 快速分享
        /// </summary>
        public IFastShareService FastShareService { get; set; }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="postService"></param>
        /// <param name="categoryService"></param>
        /// <param name="searchDetailsService"></param>
        /// <param name="noticeService"></param>
        /// <param name="postAccessRecordService"></param>
        /// <param name="fastShareService"></param>
        public HomeController(IPostService postService, ICategoryService categoryService, ISearchDetailsService searchDetailsService, INoticeService noticeService, IPostAccessRecordService postAccessRecordService, IFastShareService fastShareService)
        {
            CategoryService = categoryService;
            PostService = postService;
            SearchDetailsService = searchDetailsService;
            NoticeService = noticeService;
            PostAccessRecordService = postAccessRecordService;
            FastShareService = fastShareService;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public ActionResult Index(OrderBy orderBy = OrderBy.ModifyDate)
        {
            ViewBag.Total = 0;
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var tops = PostService.LoadEntitiesFromL2CacheNoTracking(t => t.Status == Status.Pended && t.IsBanner, p => p.ModifyDate, false).Select(p => new
            {
                p.Title,
                p.Description,
                p.Id,
                p.ImageUrl
            }).ToList();
            List<FastShare> fastShares = FastShareService.GetAllFromL2CacheNoTracking(s => s.Sort).ToList();
            ViewBag.FastShare = fastShares;
            var viewModel = GetIndexPageViewModel(1, 15, orderBy, user);
            var banner = new List<PostOutputDto>();
            tops.ForEach(t =>
            {
                banner.Add(t.MapTo<PostOutputDto>());
            });
            viewModel.Banner = banner;
            return View(viewModel);
        }

        /// <summary>
        /// 文章列表页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("p/{page:int?}/{size:int?}/{orderBy:int?}")]
        public ActionResult Post(int page = 1, int size = 15, OrderBy orderBy = OrderBy.ModifyDate)
        {
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            ViewBag.Total = PostService.LoadEntitiesFromL2Cache<PostOutputDto>(p => p.Status == Status.Pended || user.IsAdmin && !p.IsFixedTop).Count(p => !p.IsFixedTop);
            var viewModel = GetIndexPageViewModel(page, size, orderBy, user);
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
        [Route("tag/{id}/{page:int?}/{size:int?}/{orderBy:int?}")]
        public ActionResult Tag(string id, int page = 1, int size = 15, OrderBy orderBy = OrderBy.ModifyDate)
        {
            IList<PostOutputDto> posts;
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var temp = PostService.LoadEntities<PostOutputDto>(p => p.Label.Contains(id) && (p.Status == Status.Pended || user.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
            switch (orderBy)
            {
                case OrderBy.CommentCount:
                    posts = temp.ThenByDescending(p => p.Comment.Count).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
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
            var viewModel = GetIndexPageViewModel(1, 1, orderBy, user);
            ViewBag.Total = temp.Count();
            ViewBag.Tag = id;
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
        [Route("cat/{id:int}")]
        [Route("cat/{id:int}/{page:int?}/{size:int?}/{orderBy:int?}")]
        public async Task<ActionResult> Category(int id, int page = 1, int size = 15, OrderBy orderBy = OrderBy.ModifyDate)
        {
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var cat = await CategoryService.GetByIdAsync(id).ConfigureAwait(true);
            if (cat is null) return RedirectToAction("Index", "Error");
            var posts = PostService.LoadEntitiesNoTracking(p => p.CategoryId == cat.Id && (p.Status == Status.Pended || user.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
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
                    posts = posts.ThenByDescending(p => p.PostAccessRecord.Sum(r => r.ClickCount));
                    break;
                case OrderBy.VoteCount:
                    posts = posts.ThenByDescending(p => p.VoteUpCount);
                    break;
                case OrderBy.AverageViewCount:
                    posts = posts.ThenByDescending(p => p.PostAccessRecord.Average(r => r.ClickCount));
                    break;
                default:
                    posts = posts.ThenByDescending(p => p.ModifyDate);
                    break;
            }
            var viewModel = GetIndexPageViewModel(1, 1, orderBy, user);
            ViewBag.CategoryName = cat.Name;
            ViewBag.Desc = cat.Description;
            viewModel.Posts = posts.Skip(size * (page - 1)).Take(size).ProjectTo<PostOutputDto>().Cacheable().ToList();
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
        private IndexPageViewModel GetIndexPageViewModel(int page, int size, OrderBy orderBy, UserInfoOutputDto user)
        {
            IQueryable<PostOutputDto> postList = PostService.LoadEntities<PostOutputDto>(p => (p.Status == Status.Pended || user.IsAdmin)); //准备文章的查询
            var notices = NoticeService.LoadPageEntitiesFromL2Cache<DateTime, NoticeOutputDto>(1, 5, out int _, n => (n.Status == Status.Display || user.IsAdmin), n => n.ModifyDate, false).ToList(); //加载前5条公告
            var cats = CategoryService.LoadEntitiesFromL2Cache<string, CategoryOutputDto>(c => c.Status == Status.Available, c => c.Name).ToList(); //加载分类目录
            var start = DateTime.Today.AddDays(-7);
            var hotSearches = SearchDetailsService.LoadEntitiesNoTracking(s => s.SearchTime > start, s => s.SearchTime, false).GroupBy(s => s.KeyWords).OrderByDescending(g => g.Count()).Take(10).Select(g => new KeywordsRankOutputDto()
            {
                KeyWords = g.FirstOrDefault().KeyWords,
                SearchCount = g.Count()
            }).Cacheable().ToList(); //热词统计
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
            var hot6Post = postList.OrderByDescending(order).Skip(0).Take(5).Cacheable().ToList(); //热门文章
            var topPostWeek = PostService.SqlQuery<SimplePostModel>("SELECT [Id],[Title] from Post WHERE Id in (SELECT top 10 PostId FROM [dbo].[PostAccessRecord] where DATEDIFF(week,AccessTime,getdate())<=1 GROUP BY PostId ORDER BY sum(ClickCount) desc)").ToList(); //文章周排行
            var topPostMonth = PostService.SqlQuery<SimplePostModel>("SELECT [Id],[Title] from Post WHERE Id in (SELECT top 10 PostId FROM [dbo].[PostAccessRecord] where DATEDIFF(month,AccessTime,getdate())<=1 GROUP BY PostId ORDER BY sum(ClickCount) desc)").ToList(); //文章月排行
            var topPostYear = PostService.SqlQuery<SimplePostModel>("SELECT [Id],[Title] from Post WHERE Id in (SELECT top 10 PostId FROM [dbo].[PostAccessRecord] where DATEDIFF(year,AccessTime,getdate())<=1 GROUP BY PostId ORDER BY sum(ClickCount) desc)").ToList(); //文章年度排行
            var tags = new List<string>(); //标签云
            var tagdic = new Dictionary<string, int>();
            var newdic = new Dictionary<string, int>(); //标签云最终结果
            postList.Select(p => p.Label).Cacheable().ToList().ForEach(m =>
            {
                if (!string.IsNullOrEmpty(m))
                {
                    tags.AddRange(m.Split(',', '，'));
                }
            }); //统计标签
            tags.GroupBy(s => s).ForEach(g =>
            {
                tagdic.Add(g.Key, g.Count());
            }); //将标签分组
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
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.Comment.Count).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.PostDate:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.TotalViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                case OrderBy.AverageViewCount:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.AverageViewCount).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
                default:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).Cacheable().ToList();
                    break;
            }
            if (page == 1)
            {
                //posts = postList.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).AsEnumerable().Union(posts).ToList();
            }
            return new IndexPageViewModel()
            {
                Categories = cats,
                HotSearch = hotSearches,
                Notices = notices,
                Posts = posts,
                Tags = newdic,
                Top6Post = hot6Post,
                TopPostByMonth = topPostMonth,
                TopPostByWeek = topPostWeek,
                TopPostByYear = topPostYear,
                PostsQueryable = postList
            };
        }
    }
}