using Common;
using EFSecondLevelCache;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class HomeController : BaseController
    {
        public IPostBll PostBll { get; set; }
        public ICategoryBll CategoryBll { get; set; }
        public ICommentBll CommentBll { get; set; }
        public ISearchDetailsBll SearchDetailsBll { get; set; }
        public INoticeBll NoticeBll { get; set; }
        public IPostAccessRecordBll PostAccessRecordBll { get; set; }
        public IFastShareBll FastShareBll { get; set; }
        public HomeController(IPostBll postBll, ICommentBll commentBll, ICategoryBll categoryBll, ISearchDetailsBll searchDetailsBll, INoticeBll noticeBll, IPostAccessRecordBll postAccessRecordBll, IFastShareBll fastShareBll)
        {
            CategoryBll = categoryBll;
            PostBll = postBll;
            CommentBll = commentBll;
            SearchDetailsBll = searchDetailsBll;
            NoticeBll = noticeBll;
            PostAccessRecordBll = postAccessRecordBll;
            FastShareBll = fastShareBll;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public ActionResult Index(OrderBy orderBy = OrderBy.ModifyDate)
        {
            ViewBag.Total = 0;
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var tops = PostBll.LoadEntitiesFromL2CacheNoTracking<DateTime, PostOutputDto>(t => t.Status == Status.Pended && t.IsBanner, p => p.ModifyDate, false).Select(p => new
            {
                p.Title,
                p.Description,
                p.Id,
                p.ImageUrl
            }).ToList();
            List<FastShare> fastShares = FastShareBll.GetAllFromL2CacheNoTracking(s => s.Sort).ToList();
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
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            ViewBag.Total = PostBll.LoadEntitiesFromL2CacheNoTracking<PostOutputDto>(p => p.Status == Status.Pended || user.IsAdmin && !p.IsFixedTop).Count(p => !p.IsFixedTop);
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
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var temp = PostBll.LoadEntitiesNoTracking<PostOutputDto>(p => p.Label.Contains(id) && (p.Status == Status.Pended || user.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
            switch (orderBy)
            {
                case OrderBy.CommentCount:
                    posts = temp.ThenByDescending(p => p.Comment.Count).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.PostDate:
                    posts = temp.ThenByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = temp.ThenByDescending(p => p.ViewCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = temp.ThenByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.AverageViewCount:
                    posts = temp.ThenByDescending(p => p.AverageViewCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                default:
                    posts = temp.ThenByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
            }
            var viewModel = GetIndexPageViewModel(0, 0, orderBy, user);
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
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var cat = await CategoryBll.GetByIdAsync(id).ConfigureAwait(true);
            if (cat is null) return RedirectToAction("Index", "Error");
            var posts = PostBll.LoadEntitiesNoTracking(p => p.CategoryId == cat.Id && (p.Status == Status.Pended || user.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
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
            var viewModel = GetIndexPageViewModel(0, 0, orderBy, user);
            ViewBag.CategoryName = cat.Name;
            ViewBag.Desc = cat.Description;
            viewModel.Posts = posts.Skip(size * (page - 1)).Take(size).ToList().Mapper<IList<PostOutputDto>>();
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
            IQueryable<PostOutputDto> postList = PostBll.LoadEntitiesNoTracking<PostOutputDto>(p => (p.Status == Status.Pended || user.IsAdmin)); //准备文章的查询
            var notices = NoticeBll.LoadPageEntitiesFromL2CacheNoTracking<DateTime, NoticeOutputDto>(1, 5, out int _, n => (n.Status == Status.Display || user.IsAdmin), n => n.ModifyDate, false).ToList(); //加载前5条公告
            var cats = CategoryBll.LoadEntitiesFromL2CacheNoTracking<string, CategoryOutputDto>(c => c.Status == Status.Available, c => c.Name); //加载分类目录
            var start = DateTime.Today.AddDays(-7);
            var hotSearches = SearchDetailsBll.LoadEntitiesNoTracking(s => s.SearchTime > start, s => s.SearchTime, false).GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(10).Select(g => new KeywordsRankOutputDto()
            {
                KeyWords = g.FirstOrDefault().KeyWords,
                SearchCount = g.Count()
            }).Cacheable().ToList(); //热词统计
            Expression<Func<PostOutputDto, double>> order = p => p.ViewCount;
            switch (new Random().Next() % 3)
            {
                case 0:
                    order = p => p.ViewCount;
                    break;
                case 1:
                    order = p => p.VoteUpCount;
                    break;
                case 2:
                    order = p => p.AverageViewCount;
                    break;
            }
            var hot6Post = postList.OrderByDescending(order).Skip(0).Take(5).Cacheable().ToList(); //热门文章
            var topPostWeek = PostBll.SqlQuery<SimplePostModel>("SELECT [Id],[Title] from Post WHERE Id in (SELECT top 10 PostId FROM [dbo].[PostAccessRecord] where DATEDIFF(week,AccessTime,getdate())<=1 GROUP BY PostId ORDER BY sum(ClickCount) desc)").ToList(); //文章周排行
            var topPostMonth = PostBll.SqlQuery<SimplePostModel>("SELECT [Id],[Title] from Post WHERE Id in (SELECT top 10 PostId FROM [dbo].[PostAccessRecord] where DATEDIFF(month,AccessTime,getdate())<=1 GROUP BY PostId ORDER BY sum(ClickCount) desc)").ToList(); //文章月排行
            var topPostYear = PostBll.SqlQuery<SimplePostModel>("SELECT [Id],[Title] from Post WHERE Id in (SELECT top 10 PostId FROM [dbo].[PostAccessRecord] where DATEDIFF(year,AccessTime,getdate())<=1 GROUP BY PostId ORDER BY sum(ClickCount) desc)").ToList(); //文章年度排行
            var tags = new List<string>(); //标签云
            var tagdic = new Dictionary<string, int>();
            var newdic = new Dictionary<string, int>(); //标签云最终结果
            postList.Select(p => p.Label).ToList().ForEach(m =>
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
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.Comment.Count).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.PostDate:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.ViewCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.AverageViewCount:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.AverageViewCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                default:
                    posts = postList.Where(p => !p.IsFixedTop).OrderByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
            }
            if (page == 1)
            {
                posts = postList.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).AsEnumerable().Union(posts).ToList();
            }
            return new IndexPageViewModel()
            {
                Categories = cats.ToList(),
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