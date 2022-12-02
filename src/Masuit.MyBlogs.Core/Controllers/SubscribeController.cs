using Dispose.Scope;
using FreeRedis;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using WilderMinds.RssSyndication;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 订阅服务
/// </summary>
public sealed class SubscribeController : Controller
{
	public IPostService PostService { get; set; }

	public IAdvertisementService AdvertisementService { get; set; }
	public IRedisClient RedisClient { get; set; }

	/// <summary>
	/// RSS订阅
	/// </summary>
	/// <returns></returns>
	[Route("/rss"), ResponseCache(Duration = 3600)]
	public async Task<IActionResult> Rss()
	{
		if (CommonHelper.SystemSettings.GetOrAdd("EnableRss", "true") != "true")
		{
			throw new NotFoundException("不允许订阅");
		}

		var time = DateTime.Today.AddDays(-1);
		string scheme = Request.Scheme;
		var host = Request.Host;
		var raw = PostService.GetQuery(PostBaseWhere().And(p => p.Rss && p.ModifyDate >= time), p => p.ModifyDate, false).Include(p => p.Category).AsNoTracking().FromCache(new MemoryCacheEntryOptions()
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
		});
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
		var posts = data.ToPooledListScope();
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

	private void InsertAdvertisement(IList<Item> posts, int? cid = null, string keywords = "")
	{
		if (posts.Count > 2)
		{
			var ad = AdvertisementService.GetByWeightedPrice((AdvertiseType)(DateTime.Now.Second % 4 + 1), Request.Location(), cid, keywords);
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
					Guid = SnowFlake.NewId,
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
		if (CommonHelper.SystemSettings.GetOrAdd("EnableRss", "true") != "true")
		{
			throw new NotFoundException("不允许订阅");
		}

		var time = DateTime.Today.AddDays(-1);
		string scheme = Request.Scheme;
		var host = Request.Host;
		var category = await categoryService.GetByIdAsync(id) ?? throw new NotFoundException("分类未找到");
		var cids = category.Flatten().Select(c => c.Id).ToArray();
		var raw = PostService.GetQuery(PostBaseWhere().And(p => p.Rss && cids.Contains(p.CategoryId) && p.ModifyDate >= time), p => p.ModifyDate, false).Include(p => p.Category).AsNoTracking().FromCache(new MemoryCacheEntryOptions()
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
		});
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
		var posts = data.ToPooledListScope();
		InsertAdvertisement(posts, id, category.Name);
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
		if (CommonHelper.SystemSettings.GetOrAdd("EnableRss", "true") != "true")
		{
			throw new NotFoundException("不允许订阅");
		}

		var time = DateTime.Today.AddDays(-1);
		string scheme = Request.Scheme;
		var host = Request.Host;
		var seminar = await seminarService.GetByIdAsync(id) ?? throw new NotFoundException("专题未找到");
		var raw = PostService.GetQuery(PostBaseWhere().And(p => p.Rss && p.Seminar.Any(s => s.Id == id) && p.ModifyDate >= time), p => p.ModifyDate, false).Include(p => p.Category).AsNoTracking().FromCache(new MemoryCacheEntryOptions()
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
		});
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
		var posts = data.ToPooledListScope();
		InsertAdvertisement(posts, id, seminar.Title);
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
		if (CommonHelper.SystemSettings.GetOrAdd("EnableRss", "true") != "true")
		{
			throw new NotFoundException("不允许订阅");
		}

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
				post.Category.Path()
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

	private Expression<Func<Post, bool>> PostBaseWhere()
	{
		var ipLocation = Request.Location();
		var location = ipLocation + ipLocation.Coodinate + "|" + Request.Headers[HeaderNames.Referer] + "|" + Request.Headers[HeaderNames.UserAgent];
		return p => p.Status == Status.Published && p.LimitMode != RegionLimitMode.OnlyForSearchEngine
			&& (p.LimitMode == null || p.LimitMode == RegionLimitMode.All ? true :
				p.LimitMode == RegionLimitMode.AllowRegion ? Regex.IsMatch(location, p.Regions, RegexOptions.IgnoreCase) :
				p.LimitMode == RegionLimitMode.ForbidRegion ? !Regex.IsMatch(location, p.Regions, RegexOptions.IgnoreCase) :
				p.LimitMode == RegionLimitMode.AllowRegionExceptForbidRegion ? Regex.IsMatch(location, p.Regions, RegexOptions.IgnoreCase) && !Regex.IsMatch(location, p.ExceptRegions, RegexOptions.IgnoreCase) :
				!Regex.IsMatch(location, p.Regions, RegexOptions.IgnoreCase) || Regex.IsMatch(location, p.ExceptRegions, RegexOptions.IgnoreCase));
	}

	private void CheckPermission(Post post)
	{
		var ipLocation = Request.Location();
		var location = ipLocation + ipLocation.Coodinate + "|" + Request.Headers[HeaderNames.Referer] + "|" + Request.Headers[HeaderNames.UserAgent];
		switch (post.LimitMode)
		{
			case RegionLimitMode.OnlyForSearchEngine:
				Disallow(post);
				break;

			case RegionLimitMode.AllowRegion:
				if (!Regex.IsMatch(location, post.Regions, RegexOptions.IgnoreCase) && !Request.IsRobot())
				{
					Disallow(post);
				}

				break;

			case RegionLimitMode.ForbidRegion:
				if (Regex.IsMatch(location, post.Regions, RegexOptions.IgnoreCase) && !Request.IsRobot())
				{
					Disallow(post);
				}

				break;

			case RegionLimitMode.AllowRegionExceptForbidRegion:
				if (Regex.IsMatch(location, post.ExceptRegions, RegexOptions.IgnoreCase))
				{
					Disallow(post);
				}

				goto case RegionLimitMode.AllowRegion;
			case RegionLimitMode.ForbidRegionExceptAllowRegion:
				if (Regex.IsMatch(location, post.ExceptRegions, RegexOptions.IgnoreCase))
				{
					break;
				}

				goto case RegionLimitMode.ForbidRegion;
		}
	}

	private void Disallow(Post post)
	{
		RedisClient.IncrBy("interceptCount", 1);
		RedisClient.LPush("intercept", new IpIntercepter()
		{
			IP = HttpContext.Connection.RemoteIpAddress.ToString(),
			RequestUrl = $"//{Request.Host}/{post.Id}",
			Referer = Request.Headers[HeaderNames.Referer],
			Time = DateTime.Now,
			UserAgent = Request.Headers[HeaderNames.UserAgent],
			Remark = "无权限查看该文章",
			Address = Request.Location(),
			HttpVersion = Request.Protocol,
			Headers = new
			{
				Request.Protocol,
				Request.Headers
			}.ToJsonString()
		});
		throw new NotFoundException("文章未找到");
	}
}