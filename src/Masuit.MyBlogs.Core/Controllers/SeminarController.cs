using Dispose.Scope;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Models;
using Masuit.Tools.AspNetCore.ModelBinder;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 专题页
/// </summary>
public sealed class SeminarController : BaseController
{
	/// <summary>
	/// 专题
	/// </summary>
	public ISeminarService SeminarService { get; set; }

	/// <summary>
	/// 文章
	/// </summary>
	public IPostService PostService { get; set; }

	/// <summary>
	/// 专题页
	/// </summary>
	/// <param name="id"></param>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <param name="orderBy"></param>
	/// <returns></returns>
	[Route("special/{id:int}"), Route("c/{id:int}", Order = 1), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
	public async Task<ActionResult> Index(int id, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		var s = await SeminarService.GetByIdAsync(id) ?? throw new NotFoundException("专题未找到");
		var h24 = DateTime.Today.AddDays(-1);
		var posts = orderBy switch
		{
			OrderBy.Trending => await PostService.GetQuery(PostBaseWhere().And(p => p.Seminar.Any(x => x.Id == id))).OrderByDescending(p => p.PostVisitRecordStats.Where(e => e.Date >= h24).Sum(e => e.Count)).ToPagedListAsync<Post, PostDto>(page, size, MapperConfig),
			_ => await PostService.GetQuery(PostBaseWhere().And(p => p.Seminar.Any(x => x.Id == id))).OrderBy($"{nameof(Post.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToPagedListAsync<Post, PostDto>(page, size, MapperConfig)
		};
		ViewBag.Id = s.Id;
		ViewBag.Title = s.Title;
		ViewBag.Desc = s.Description;
		ViewBag.SubTitle = s.SubTitle;
		ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location(), keywords: s.Title);
		ViewData["page"] = new Pagination(page, size, posts.TotalCount, orderBy);
		PostService.SolvePostsCategory(posts.Data);
		foreach (var item in posts.Data)
		{
			item.PostDate = item.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
			item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
		}

		return View(posts);
	}

	#region 管理端

	/// <summary>
	/// 保存专题
	/// </summary>
	/// <param name="seminar"></param>
	/// <returns></returns>
	[MyAuthorize]
	public ActionResult Save([FromBodyOrDefault] Seminar seminar)
	{
		if (seminar.Id > 0 ? SeminarService.Any(s => s.Id != seminar.Id && s.Title == seminar.Title) : SeminarService.Any(s => s.Title == seminar.Title))
		{
			return ResultData(null, false, $"{seminar.Title} 已经存在了");
		}

		var entry = SeminarService.GetById(seminar.Id);
		bool b;
		if (entry is null)
		{
			b = SeminarService.AddEntitySaved(seminar) != null;
		}
		else
		{
			entry.Description = seminar.Description;
			entry.Title = seminar.Title;
			entry.SubTitle = seminar.SubTitle;
			b = SeminarService.SaveChanges() > 0;
		}

		return ResultData(null, b, b ? "保存成功" : "保存失败");
	}

	/// <summary>
	/// 删除专题
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Delete(int id)
	{
		bool b = await SeminarService.DeleteByIdAsync(id) > 0;
		return ResultData(null, b, b ? "删除成功" : "删除失败");
	}

	/// <summary>
	/// 获取专题详情
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> Get(int id)
	{
		Seminar seminar = await SeminarService.GetByIdAsync(id);
		return ResultData(Mapper.Map<SeminarDto>(seminar));
	}

	/// <summary>
	/// 专题分页列表
	/// </summary>
	/// <param name="page"></param>
	/// <param name="size"></param>
	/// <returns></returns>
	[MyAuthorize]
	public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
	{
		var list = SeminarService.GetPages<int, SeminarDto>(page, size, s => true, s => s.Id, false);
		return Ok(list);
	}

	/// <summary>
	/// 获取所有专题
	/// </summary>
	/// <returns></returns>
	[MyAuthorize]
	public ActionResult GetAll()
	{
		var list = SeminarService.GetAll<string, SeminarDto>(s => s.Title).ToPooledListScope();
		return ResultData(list);
	}

	/// <summary>
	/// 给专题添加文章
	/// </summary>
	/// <param name="id"></param>
	/// <param name="pid"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> AddPost(int id, int pid)
	{
		Seminar seminar = await SeminarService.GetByIdAsync(id);
		Post post = await PostService.GetByIdAsync(pid);
		seminar.Post.Add(post);
		bool b = await SeminarService.SaveChangesAsync() > 0;
		return ResultData(null, b, b ? $"已成功将【{post.Title}】添加到专题【{seminar.Title}】" : "添加失败！");
	}

	/// <summary>
	/// 移除文章
	/// </summary>
	/// <param name="id"></param>
	/// <param name="pid"></param>
	/// <returns></returns>
	[MyAuthorize]
	public async Task<ActionResult> RemovePost(int id, int pid)
	{
		Seminar seminar = await SeminarService.GetByIdAsync(id);
		Post post = await PostService.GetByIdAsync(pid);

		//bool b = await seminarPostService.DeleteEntitySavedAsync(s => s.SeminarId == id && s.PostId == pid) > 0;
		seminar.Post.Remove(post);
		var b = await SeminarService.SaveChangesAsync() > 0;
		return ResultData(null, b, b ? $"已成功将【{post.Title}】从专题【{seminar.Title}】移除" : "添加失败！");
	}

	#endregion 管理端
}