using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 网站公告
    /// </summary>
    public class NoticeController : BaseController
    {
        /// <summary>
        /// 公告
        /// </summary>
        public INoticeService NoticeService { get; set; }

        public IWebHostEnvironment HostEnvironment { get; set; }

        public ImagebedClient ImagebedClient { get; set; }

        /// <summary>
        /// 公告列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("notice"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Index([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var list = await NoticeService.GetPagesFromCacheAsync<DateTime, NoticeDto>(page, size, n => n.Status == Status.Display, n => n.ModifyDate, false);
            ViewData["page"] = new Pagination(page, size, list.TotalCount);
            foreach (var n in list.Data)
            {
                n.ModifyDate = n.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                n.PostDate = n.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.PostList);
            return CurrentUser.IsAdmin ? View("Index_Admin", list.Data) : View(list.Data);
        }

        /// <summary>
        /// 公告详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("n/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = "Cookie")]
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
            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage);
            return View(notice);
        }

        /// <summary>
        /// 发布公告
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Write(Notice notice)
        {
            notice.Content = await ImagebedClient.ReplaceImgSrc(notice.Content.ClearImgAttributes());
            Notice e = NoticeService.AddEntitySaved(notice);
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
            var notice = await NoticeService.GetByIdAsync(id) ?? throw new NotFoundException("公告已经被删除！");
            var srcs = notice.Content.MatchImgSrcs().Where(s => s.StartsWith("/"));
            foreach (var path in srcs)
            {
                try
                {
                    System.IO.File.Delete(HostEnvironment.WebRootPath + path);
                }
                catch
                {
                }
            }

            bool b = await NoticeService.DeleteByIdSavedAsync(id) > 0;
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 编辑公告
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Edit(Notice notice)
        {
            var entity = await NoticeService.GetByIdAsync(notice.Id) ?? throw new NotFoundException("公告已经被删除！");
            entity.ModifyDate = DateTime.Now;
            entity.Title = notice.Title;
            entity.Content = await ImagebedClient.ReplaceImgSrc(notice.Content.ClearImgAttributes());
            bool b = await NoticeService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? "修改成功" : "修改失败");
        }

        /// <summary>
        /// 公告分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var list = NoticeService.GetPagesNoTracking(page, size, n => true, n => n.ModifyDate, false);
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
            var notice = NoticeService.Get<NoticeDto>(n => n.Id == id);
            if (notice != null)
            {
                notice.ModifyDate = notice.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                notice.PostDate = notice.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return ResultData(notice);
        }

        /// <summary>
        /// 最近一条公告
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Last()
        {
            var notice = await NoticeService.GetAsync(n => n.Status == Status.Display, n => n.ModifyDate, false);
            if (notice == null)
            {
                return ResultData(null, false);
            }

            if (Request.Cookies.TryGetValue("last-notice", out var id) && notice.Id.ToString() == id)
            {
                return ResultData(null, false);
            }

            notice.ViewCount += 1;
            await NoticeService.SaveChangesAsync();
            var dto = notice.Mapper<NoticeDto>();
            Response.Cookies.Append("last-notice", dto.Id.ToString(), new CookieOptions()
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            return ResultData(dto);
        }
    }
}