using EFCoreSecondLevelCacheInterceptor;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Models;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Core;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 网站公告
/// </summary>
public sealed class NoticeController : BaseController
{
    /// <summary>
    /// 公告
    /// </summary>
    public INoticeService NoticeService { get; set; }

    public ImagebedClient ImagebedClient { get; set; }

    /// <summary>
    /// 公告列表
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    [Route("notice"), AllowAccessFirewall, Route("n", Order = 1), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size" }, VaryByHeader = "Cookie")]
    public ActionResult Index([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
    {
        var list = NoticeService.GetQuery(n => n.NoticeStatus == NoticeStatus.Normal, n => n.ModifyDate, false).ProjectDto().ToCachedPagedList(page, size);
        ViewData["page"] = new Pagination(page, size, list.TotalCount);
        foreach (var n in list.Data)
        {
            n.ModifyDate = n.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            n.PostDate = n.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            n.Content = ReplaceVariables(n.Content);
        }

        ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.ListItem, Request.Location());
        return CurrentUser.IsAdmin ? View("Index_Admin", list.Data) : View(list.Data);
    }

    /// <summary>
    /// 公告详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Route("notice/{id:int}"), AllowAccessFirewall, ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = "Cookie")]
    public async Task<ActionResult> Details(int id)
    {
        var notice = await NoticeService.GetByIdAsync(id) ?? throw new NotFoundException("页面未找到");
        if (!HttpContext.Session.TryGetValue("notice" + id, out _))
        {
            notice.ViewCount += 1;
            await NoticeService.SaveChangesAsync();
            HttpContext.Session.Set("notice" + id, notice.Title);
        }

        notice.ModifyDate = notice.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
        notice.PostDate = notice.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
        notice.Content = ReplaceVariables(notice.Content);
        ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, Request.Location());
        return View(notice);
    }

    /// <summary>
    /// 发布公告
    /// </summary>
    /// <param name="notice"></param>
    /// <returns></returns>
    [MyAuthorize]
    public async Task<ActionResult> Write([FromBodyOrDefault] Notice notice, CancellationToken cancellationToken)
    {
        notice.Content = await ImagebedClient.ReplaceImgSrc(await notice.Content.ClearImgAttributes(), cancellationToken);
        if (notice.StartTime.HasValue && notice.EndTime.HasValue && notice.StartTime >= notice.EndTime)
        {
            return ResultData(null, false, "开始时间不能小于结束时间");
        }

        notice.NoticeStatus = NoticeStatus.Normal;
        if (DateTime.Now < notice.StartTime)
        {
            notice.NoticeStatus = NoticeStatus.UnStart;
        }

        var e = NoticeService.AddEntitySaved(notice);
        return e != null ? ResultData(null, message: "发布成功") : ResultData(null, false, "发布失败");
    }

    /// <summary>
    /// 删除公告
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [MyAuthorize]
    public async Task<ActionResult> Delete(int id)
    {
        bool b = await NoticeService.DeleteByIdAsync(id) > 0;
        return ResultData(null, b, b ? "删除成功" : "删除失败");
    }

    /// <summary>
    /// 公告上下架
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [MyAuthorize]
    public async Task<ActionResult> ChangeState(int id)
    {
        var notice = await NoticeService.GetByIdAsync(id) ?? throw new NotFoundException("公告未找到");
        notice.NoticeStatus = notice.NoticeStatus == NoticeStatus.Normal ? NoticeStatus.Expired : NoticeStatus.Normal;
        var b = await NoticeService.SaveChangesAsync() > 0;
        return ResultData(null, b, notice.NoticeStatus == NoticeStatus.Normal ? $"【{notice.Title}】已上架！" : $"【{notice.Title}】已下架！");
    }

    /// <summary>
    /// 编辑公告
    /// </summary>
    /// <param name="notice"></param>
    /// <returns></returns>
    [MyAuthorize]
    public async Task<ActionResult> Edit([FromBodyOrDefault] NoticeDto notice, CancellationToken cancellationToken)
    {
        var entity = await NoticeService.GetByIdAsync(notice.Id) ?? throw new NotFoundException("公告已经被删除！");
        if (notice.StartTime.HasValue && notice.EndTime.HasValue && notice.StartTime >= notice.EndTime)
        {
            return ResultData(null, false, "开始时间不能小于结束时间");
        }

        if (DateTime.Now < notice.StartTime)
        {
            entity.NoticeStatus = NoticeStatus.UnStart;
        }

        entity.ModifyDate = DateTime.Now;
        entity.StartTime = notice.StartTime;
        entity.EndTime = notice.EndTime;
        entity.Title = notice.Title;
        entity.StrongAlert = notice.StrongAlert;
        entity.Content = await ImagebedClient.ReplaceImgSrc(await notice.Content.ClearImgAttributes(), cancellationToken);
        bool b = await NoticeService.SaveChangesAsync() > 0;
        return ResultData(null, b, b ? "修改成功" : "修改失败");
    }

    /// <summary>
    /// 公告分页数据
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="keywords"></param>
    /// <returns></returns>
    public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15, string keywords = null)
    {
        Expression<Func<Notice, bool>> where = n => true;

        var list = NoticeService.GetPagesNoTracking(page, size, where.AndIf(!string.IsNullOrWhiteSpace(keywords), n => n.Title.Contains(keywords) || n.Content.Contains(keywords)), n => n.ModifyDate, false);
        foreach (var n in list.Data)
        {
            n.ModifyDate = n.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            n.PostDate = n.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
        }

        return Ok(list);
    }

    /// <summary>
    /// 公告详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [MyAuthorize]
    public ActionResult Get(int id)
    {
        var notice = NoticeService.Get(n => n.Id == id).ToDto();
        if (notice != null)
        {
            notice.ModifyDate = notice.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            notice.PostDate = notice.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            notice.Content = ReplaceVariables(notice.Content);
        }

        return ResultData(notice);
    }

    /// <summary>
    /// 最近一条公告
    /// </summary>
    /// <returns></returns>
    [ResponseCache(Duration = 600, VaryByHeader = "Cookie"), AllowAccessFirewall]
    public async Task<ActionResult> Last()
    {
        var notice = await NoticeService.GetQuery(n => n.NoticeStatus == NoticeStatus.Normal && n.StrongAlert, n => n.ModifyDate, false).ProjectDto().Cacheable().FirstOrDefaultWithNoLockAsync();
        if (notice == null)
        {
            return ResultData(null, false);
        }

        if (Request.Cookies.TryGetValue("last-notice", out var id) && notice.Id.ToString() == id)
        {
            return ResultData(null, false);
        }

        await NoticeService.GetQuery(n => n.Id == notice.Id).ExecuteUpdateAsync(calls => calls.SetProperty(n => n.ViewCount, n => n.ViewCount + 1));
        Response.Cookies.Append("last-notice", notice.Id.ToString(), new CookieOptions()
        {
            Expires = DateTime.Now.AddYears(1),
            SameSite = SameSiteMode.Lax
        });
        return ResultData(notice);
    }
}