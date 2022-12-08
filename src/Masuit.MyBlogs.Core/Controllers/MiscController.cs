using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.AspNetCore.ModelBinder;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 杂项页
/// </summary>
public sealed class MiscController : BaseController
{
	/// <summary>
	/// MiscService
	/// </summary>
	public IMiscService MiscService { get; set; }

	public IWebHostEnvironment HostEnvironment { get; set; }

	public ImagebedClient ImagebedClient { get; set; }

	/// <summary>
	/// 杂项页
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[Route("misc/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = "Cookie")]
	public async Task<ActionResult> Index(int id)
	{
		var misc = await MiscService.GetFromCacheAsync(m => m.Id == id) ?? throw new NotFoundException("页面未找到");
		misc.ModifyDate = misc.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
		misc.PostDate = misc.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
		misc.Content = ReplaceVariables(misc.Content);
		return View(misc);
	}

	/// <summary>
	/// 打赏
	/// </summary>
	/// <returns></returns>
	[Route("donate")]
	public async Task<ActionResult> Donate()
	{
		var ads = AdsService.GetsByWeightedPrice(2, AdvertiseType.InPage, Request.Location());
		if (bool.Parse(CommonHelper.SystemSettings.GetOrAdd("EnableDonate", "true")))
		{
			ViewBag.Ads = ads;
			var text = await new FileInfo(Path.Combine(HostEnvironment.WebRootPath, "template", "donate.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8);
			return CurrentUser.IsAdmin ? View("Donate_Admin", text) : View(model: text);
		}

		return Redirect(ads.FirstOrDefault()?.Url ?? "/");
	}

	/// <summary>
	/// 打赏列表
	/// </summary>
	/// <param name="donateService"></param>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <returns></returns>
	[Route("donatelist")]
	public ActionResult DonateList([FromServices] IDonateService donateService, int page = 1, int size = 10)
	{
		if (bool.Parse(CommonHelper.SystemSettings.GetOrAdd("EnableDonate", "true")))
		{
			var list = donateService.GetPagesFromCache<DateTime, DonateDto>(page, size, d => true, d => d.DonateTime, false);
			if (!CurrentUser.IsAdmin)
			{
				foreach (var item in list.Data.Where(item => !(item.QQorWechat + item.Email).Contains("匿名")))
				{
					item.QQorWechat = item.QQorWechat?.Mask();
					item.Email = item.Email?.MaskEmail();
				}
			}

			return Ok(list);
		}

		return Ok();
	}

	/// <summary>
	/// 关于
	/// </summary>
	/// <returns></returns>
	[Route("about"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
	public async Task<ActionResult> About()
	{
		var text = await new FileInfo(Path.Combine(HostEnvironment.WebRootPath, "template", "about.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8);
		return View(model: text);
	}

	/// <summary>
	/// 评论及留言须知
	/// </summary>
	/// <returns></returns>
	[Route("agreement"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
	public async Task<ActionResult> Agreement()
	{
		var text = await new FileInfo(Path.Combine(HostEnvironment.WebRootPath, "template", "agreement.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8);
		return View(model: text);
	}

	/// <summary>
	/// 声明
	/// </summary>
	/// <returns></returns>
	[Route("disclaimer"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
	public async Task<ActionResult> Disclaimer()
	{
		var text = await new FileInfo(Path.Combine(HostEnvironment.WebRootPath, "template", "disclaimer.html")).ShareReadWrite().ReadAllTextAsync(Encoding.UTF8);
		return View(model: text);
	}

	/// <summary>
	/// 创建页面
	/// </summary>
	/// <param name="model"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Write([FromBodyOrDefault] Misc model, CancellationToken cancellationToken)
	{
		model.Content = await ImagebedClient.ReplaceImgSrc(await model.Content.Trim().ClearImgAttributes(), cancellationToken);
		var e = MiscService.AddEntitySaved(model);
		return e != null ? ResultData(null, message: "发布成功") : ResultData(null, false, "发布失败");
	}

	/// <summary>
	/// 删除页面
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Delete(int id)
	{
		bool b = await MiscService.DeleteByIdAsync(id) > 0;
		return ResultData(null, b, b ? "删除成功" : "删除失败");
	}

	/// <summary>
	/// 编辑页面
	/// </summary>
	/// <param name="misc"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Edit([FromBodyOrDefault] Misc misc, CancellationToken cancellationToken)
	{
		var entity = await MiscService.GetByIdAsync(misc.Id) ?? throw new NotFoundException("杂项页未找到");
		entity.ModifyDate = DateTime.Now;
		entity.Title = misc.Title;
		entity.Content = await ImagebedClient.ReplaceImgSrc(await misc.Content.ClearImgAttributes(), cancellationToken);
		bool b = await MiscService.SaveChangesAsync() > 0;
		return ResultData(null, b, b ? "修改成功" : "修改失败");
	}

	/// <summary>
	/// 分页数据
	/// </summary>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <returns></returns>
	[MyAuthorize]
	public ActionResult GetPageData(int page = 1, int size = 10)
	{
		var list = MiscService.GetPages(page, size, n => true, n => n.ModifyDate, false);
		foreach (var item in list.Data)
		{
			item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
			item.PostDate = item.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
		}

		return Ok(list);
	}

	/// <summary>
	/// 详情
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Get(int id)
	{
		var misc = await MiscService.GetByIdAsync(id);
		if (misc != null)
		{
			misc.ModifyDate = misc.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
			misc.PostDate = misc.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
		}

		return ResultData(Mapper.Map<MiscDto>(misc));
	}
}