using AngleSharp;
using AutoMapper.QueryableExtensions;
using Dispose.Scope;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Models;
using Masuit.Tools.Mime;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using EFCoreSecondLevelCacheInterceptor;
using Configuration = AngleSharp.Configuration;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 首页
/// </summary>
public sealed class HomeController : BaseController
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
	[ResponseCache(Duration = 600, VaryByHeader = nameof(HeaderNames.Cookie))]
	public async Task<ActionResult> Index([FromServices] IFastShareService fastShareService)
	{
		var banners = AdsService.GetsByWeightedPrice(8, AdvertiseType.Banner, Request.Location()).OrderByRandom().ToPooledListScope();
		var fastShares = fastShareService.GetAllFromCache(s => s.Sort);
		var postsQuery = PostService.GetQuery(PostBaseWhere()); //准备文章的查询
		var posts = await postsQuery.Where(p => !p.IsFixedTop).OrderBy(OrderBy.ModifyDate.GetDisplay() + " desc").ToPagedListAsync<Post, PostDto>(1, 15, MapperConfig);
		posts.Data.InsertRange(0, postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).ProjectTo<PostDto>(MapperConfig).Cacheable().ToPooledListScope());
		var viewModel = GetIndexPageViewModel();
		viewModel.Banner = banners;
		viewModel.Posts = posts;
		ViewBag.FastShare = fastShares;
		viewModel.PageParams = new Pagination(1, 15, posts.TotalCount, OrderBy.ModifyDate);
		viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
		viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
		PostService.SolvePostsCategory(posts.Data);
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
	public async Task<ActionResult> Post([Optional] OrderBy? orderBy, int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		page = Math.Max(1, page);
		var viewModel = GetIndexPageViewModel();
		var postsQuery = PostService.GetQuery(PostBaseWhere()); //准备文章的查询
		var h24 = DateTime.Today.AddDays(-1);
		var posts = orderBy switch
		{
			OrderBy.Trending => await postsQuery.Where(p => !p.IsFixedTop).OrderByDescending(p => p.PostVisitRecordStats.Where(e => e.Date >= h24).Sum(t => t.Count)).ToPagedListAsync<Post, PostDto>(page, size, MapperConfig),
			_ => await postsQuery.Where(p => !p.IsFixedTop).OrderBy((orderBy ?? OrderBy.ModifyDate).GetDisplay() + " desc").ToPagedListAsync<Post, PostDto>(page, size, MapperConfig)
		};
		if (page == 1)
		{
			posts.Data.InsertRange(0, postsQuery.Where(p => p.IsFixedTop).OrderByDescending(p => p.ModifyDate).ProjectTo<PostDto>(MapperConfig));
		}

		viewModel.Posts = posts;
		viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
		viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
		viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
		PostService.SolvePostsCategory(posts.Data);
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
	public async Task<ActionResult> Tag(string tag, [Optional] OrderBy? orderBy, int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		page = Math.Max(1, page);
		if (string.IsNullOrWhiteSpace(tag))
		{
			throw new NotFoundException("");
		}

		var where = PostBaseWhere();
		var queryable = PostService.GetQuery(tag.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(s => Regex.Escape(s.Trim())).Aggregate(where, (current, s) => current.And(p => Regex.IsMatch(p.Label, s, RegexOptions.IgnoreCase))));
		var h24 = DateTime.Today.AddDays(-1);
		var posts = orderBy switch
		{
			OrderBy.Trending => await queryable.OrderByDescending(p => p.PostVisitRecordStats.Where(e => e.Date >= h24).Sum(e => e.Count)).ToPagedListAsync<Post, PostDto>(page, size, MapperConfig),
			_ => await queryable.OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToPagedListAsync<Post, PostDto>(page, size, MapperConfig)
		};
		var viewModel = GetIndexPageViewModel();
		ViewBag.Tag = tag;
		viewModel.Posts = posts;
		viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
		viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location(), keywords: tag);
		viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location(), keywords: tag);
		PostService.SolvePostsCategory(posts.Data);
		foreach (var item in posts.Data)
		{
			item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
		}

		return View(viewModel);
	}

	/// <summary>
	/// 存档文章页
	/// </summary>
	/// <param name="yyyy"></param>
	/// <param name="mm"></param>
	/// <param name="dd"></param>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <param name="orderBy"></param>
	/// <returns></returns>
	[Route("{yyyy:int}/{mm:int}/{dd:int}/{mode}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = nameof(HeaderNames.Cookie))]
	public async Task<ActionResult> Archieve([Range(2010, 2099)] int yyyy, [Range(1, 12)] int mm, [Range(1, 31)] int dd, [Optional] OrderBy? orderBy, int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15, string mode = nameof(Models.Entity.Post.ModifyDate))
	{
		page = Math.Max(1, page);
		if (!DateTime.TryParse(yyyy + "-" + mm + "-" + dd, out var date))
		{
			date = DateTime.Today;
		}

		var where = mode switch
		{
			nameof(Models.Entity.Post.PostDate) => PostBaseWhere().And(p => p.PostDate.Date == date),
			_ => PostBaseWhere().And(p => p.ModifyDate.Date == date),
		};
		var queryable = PostService.GetQuery(where);
		var h24 = DateTime.Today.AddDays(-1);
		var posts = orderBy switch
		{
			OrderBy.Trending => await queryable.OrderByDescending(p => p.PostVisitRecordStats.Where(e => e.Date >= h24).Sum(e => e.Count)).ToPagedListAsync<Post, PostDto>(page, size, MapperConfig),
			_ => await queryable.OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToPagedListAsync<Post, PostDto>(page, size, MapperConfig)
		};
		var viewModel = GetIndexPageViewModel();
		viewModel.Posts = posts;
		viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
		viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
		viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
		PostService.SolvePostsCategory(posts.Data);
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
	public async Task<ActionResult> Author(string author, [Optional] OrderBy? orderBy, int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		page = Math.Max(1, page);
		Expression<Func<Post, bool>> where = PostBaseWhere().And(p => p.Author.Equals(author) || p.Modifier.Equals(author) || p.Email.Equals(author) || p.PostHistoryVersion.Any(v => v.Modifier.Equals(author) || v.ModifierEmail.Equals(author)));
		var h24 = DateTime.Today.AddDays(-1);
		var posts = orderBy switch
		{
			OrderBy.Trending => await PostService.GetQuery(where).OrderByDescending(p => p.PostVisitRecordStats.Where(e => e.Date >= h24).Sum(e => e.Count)).ToPagedListAsync<Post, PostDto>(page, size, MapperConfig),
			_ => await PostService.GetQuery(where).OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToPagedListAsync<Post, PostDto>(page, size, MapperConfig)
		};
		var viewModel = GetIndexPageViewModel();
		ViewBag.Author = author;
		viewModel.Posts = posts;
		viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
		viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location());
		viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
		PostService.SolvePostsCategory(posts.Data);
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
	public async Task<ActionResult> Category(int id, [Optional] OrderBy? orderBy, int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		page = Math.Max(1, page);
		var cat = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("文章分类未找到");
		var cids = cat.Flatten().Select(c => c.Id).ToArray();
		var h24 = DateTime.Today.AddDays(-1);
		var posts = orderBy switch
		{
			OrderBy.Trending => await PostService.GetQuery(PostBaseWhere().And(p => cids.Contains(p.CategoryId))).OrderByDescending(p => p.PostVisitRecordStats.Where(e => e.Date >= h24).Sum(e => e.Count)).ToPagedListAsync<Post, PostDto>(page, size, MapperConfig),
			_ => await PostService.GetQuery(PostBaseWhere().And(p => cids.Contains(p.CategoryId))).OrderBy($"{nameof(PostDto.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToPagedListAsync<Post, PostDto>(page, size, MapperConfig)
		};
		var viewModel = GetIndexPageViewModel();
		viewModel.Posts = posts;
		ViewBag.Category = cat;
		viewModel.PageParams = new Pagination(page, size, posts.TotalCount, orderBy);
		viewModel.SidebarAds = AdsService.GetsByWeightedPrice(2, AdvertiseType.SideBar, Request.Location(), id);
		viewModel.ListAdvertisement = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location(), id);
		PostService.SolvePostsCategory(posts.Data);
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
	private HomePageViewModel GetIndexPageViewModel()
	{
		var postsQuery = PostService.GetQuery<PostDto>(PostBaseWhere()); //准备文章的查询
		var notices = NoticeService.GetPagesFromCache<DateTime, NoticeDto>(1, 5, n => n.NoticeStatus == NoticeStatus.Normal, n => n.ModifyDate, false); //加载前5条公告
		var cats = CategoryService.GetQuery(c => c.Status == Status.Available && c.Post.Count > 0).Include(c => c.Parent).OrderBy(c => c.Name).ThenBy(c => c.Path).AsNoTracking().Cacheable().ToPooledListScope(); //加载分类目录
		var hotSearches = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Week").AsNotNull().Take(10).ToPooledListScope(); //热词统计
		var hot5Post = postsQuery.OrderBy((new Random().Next() % 3) switch
		{
			1 => nameof(OrderBy.VoteUpCount),
			2 => nameof(OrderBy.AverageViewCount),
			_ => nameof(OrderBy.TotalViewCount)
		} + " desc").Skip(0).Take(5).Cacheable().ToPooledListScope(); //热门文章
		var tagdic = PostService.GetTags().OrderByRandom().Take(20).ToDictionary(x => x.Key, x => Math.Min(x.Value + 12, 32)); //统计标签
		return new HomePageViewModel
		{
			Categories = Mapper.Map<List<CategoryDto_P>>(cats.ToTree(c => c.Id, c => c.ParentId).Flatten()),
			HotSearch = hotSearches,
			Notices = notices.Data,
			Tags = tagdic,
			Top5Post = hot5Post,
			PostsQueryable = postsQuery
		};
	}
}
