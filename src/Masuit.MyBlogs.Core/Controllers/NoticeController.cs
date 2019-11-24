using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("notice"), ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "page", "size", "id" }, VaryByHeader = "Cookie")]
        public ActionResult Index(int page = 1, int size = 10, int id = 0)
        {
            var list = NoticeService.GetPages<DateTime, NoticeOutputDto>(page, size, out var total, n => n.Status == Status.Display, n => n.ModifyDate, false).ToList();
            ViewBag.Total = total;
            if (!CurrentUser.IsAdmin)
            {
                return View(list);
            }

            if (id == 0)
            {
                return View("Index_Admin", list);
            }

            var notice = NoticeService.GetById(id);
            ViewBag.Total = 1;
            return View("Index_Admin", new List<NoticeOutputDto>
            {
                notice.MapTo<NoticeOutputDto>()
            });

        }

        /// <summary>
        /// 公告详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("n/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = "Cookie")]
        public ActionResult Details(int id)
        {
            var notice = NoticeService.GetById(id) ?? throw new NotFoundException("页面未找到");
            return View(notice);
        }

        /// <summary>
        /// 发布公告
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        [Authority]
        public async Task<ActionResult> Write(Notice notice)
        {
            notice.Content = await ImagebedClient.ReplaceImgSrc(notice.Content.ClearImgAttributes());
            Notice e = NoticeService.AddEntitySaved(notice);
            if (e != null)
            {
                return ResultData(null, message: "发布成功");
            }
            return ResultData(null, false, "发布失败");
        }

        /// <summary>
        /// 删除公告
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete(int id)
        {
            var post = NoticeService.GetById(id);
            if (post is null)
            {
                return ResultData(null, false, "公告已经被删除！");
            }

            var srcs = post.Content.MatchImgSrcs().Where(s => s.StartsWith("/"));
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

            bool b = NoticeService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 编辑公告
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        [Authority]
        public async Task<ActionResult> Edit(Notice notice)
        {
            var entity = NoticeService.GetById(notice.Id);
            entity.ModifyDate = DateTime.Now;
            entity.Title = notice.Title;
            entity.Content = await ImagebedClient.ReplaceImgSrc(notice.Content.ClearImgAttributes());
            bool b = NoticeService.SaveChanges() > 0;
            return ResultData(null, b, b ? "修改成功" : "修改失败");
        }

        /// <summary>
        /// 公告分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = NoticeService.GetPagesNoTracking(page, size, out int total, n => true, n => n.ModifyDate, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 公告详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Get(int id)
        {
            if (HttpContext.Session.Get("notice" + id) != null)
            {
                return ResultData(HttpContext.Session.Get<NoticeOutputDto>("notice" + id));
            }

            var notice = NoticeService.GetById(id);
            notice.ViewCount += 1;
            NoticeService.SaveChanges();
            var dto = notice.MapTo<NoticeOutputDto>();
            HttpContext.Session.Set("notice" + id, dto);
            return ResultData(dto);
        }

        /// <summary>
        /// 最近一条公告
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public ActionResult Last()
        {
            if (Request.Cookies.TryGetValue("last-notice", out var json))
            {
                var data = JsonConvert.DeserializeObject<NoticeOutputDto>(json);
                if (!NoticeService.Any(n => n.Id > data.Id))
                {
                    return ResultData(data);
                }
            }

            var notice = NoticeService.GetFromCache(n => n.Status == Status.Display, n => n.ModifyDate, false);
            if (notice == null)
            {
                return ResultData(null, false);
            }

            notice.ViewCount += 1;
            NoticeService.SaveChanges();
            var dto = notice.Mapper<NoticeOutputDto>();
            Response.Cookies.Append("last-notice", dto.ToJsonString(), new CookieOptions()
            {
                Expires = DateTime.Now.AddMonths(1)
            });
            return ResultData(dto);
        }
    }
}