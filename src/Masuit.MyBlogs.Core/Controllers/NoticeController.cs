using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
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

        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly ImagebedClient _imagebedClient;

        /// <summary>
        /// 网站公告
        /// </summary>
        /// <param name="noticeService"></param>
        /// <param name="hostingEnvironment"></param>
        public NoticeController(INoticeService noticeService, IHostingEnvironment hostingEnvironment, IHttpClientFactory httpClientFactory)
        {
            NoticeService = noticeService;
            _hostingEnvironment = hostingEnvironment;
            _imagebedClient = new ImagebedClient(httpClientFactory.CreateClient());
        }

        /// <summary>
        /// 公告列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("notice"), ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "page", "size", "id" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Index(int page = 1, int size = 10, int id = 0)
        {
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
            List<NoticeOutputDto> list;
            int total;
            if (user != null && user.IsAdmin)
            {
                if (id != 0)
                {
                    Notice notice = NoticeService.GetById(id);
                    ViewBag.Total = 1;
                    return View("Index_Admin", new List<NoticeOutputDto>
                    {
                        notice.MapTo<NoticeOutputDto>()
                    });
                }
                list = NoticeService.LoadPageEntities<DateTime, NoticeOutputDto>(page, size, out total, n => n.Status == Status.Display, n => n.ModifyDate, false).ToList();
                ViewBag.Total = total;
                return View("Index_Admin", list);
            }
            list = NoticeService.LoadPageEntities<DateTime, NoticeOutputDto>(page, size, out total, n => n.Status == Status.Display, n => n.ModifyDate, false).ToList();
            ViewBag.Total = total;
            return View(list);
        }

        /// <summary>
        /// 公告详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("n/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Details(int id)
        {
            Notice notice = NoticeService.GetById(id);
            if (notice != null)
            {
                return View(notice);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 发布公告
        /// </summary>
        /// <param name="notice"></param>
        /// <returns></returns>
        [Authority]
        public async Task<ActionResult> Write(Notice notice)
        {
            notice.Content = await _imagebedClient.ReplaceImgSrc(notice.Content.ClearImgAttributes());
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

            var srcs = post.Content.MatchImgSrcs();
            foreach (var path in srcs)
            {
                if (path.StartsWith("/"))
                {
                    try
                    {
                        System.IO.File.Delete(_hostingEnvironment.WebRootPath + path);
                    }
                    catch
                    {
                    }
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
            Notice entity = NoticeService.GetById(notice.Id);
            entity.ModifyDate = DateTime.Now;
            entity.Title = notice.Title;
            entity.Content = await _imagebedClient.ReplaceImgSrc(notice.Content.ClearImgAttributes());
            bool b = NoticeService.UpdateEntitySaved(entity);
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
            List<Notice> list = NoticeService.LoadPageEntitiesNoTracking(page, size, out int total, n => true, n => n.ModifyDate, false).ToList();
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
            Notice notice = NoticeService.GetById(id);
            if (HttpContext.Session.Get("notice" + id) is null)
            {
                notice.ViewCount++;
                NoticeService.UpdateEntitySaved(notice);
                HttpContext.Session.Set("notice" + id, id.GetBytes());
            }
            return ResultData(notice.MapTo<NoticeOutputDto>());
        }

        /// <summary>
        /// 最近一条公告
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Last()
        {
            var notice = NoticeService.GetFirstEntity(n => n.Status == Status.Display, n => n.ModifyDate, false);
            if (notice != null)
            {
                if (HttpContext.Session.Get("notice" + notice.Id) is null)
                {
                    notice.ViewCount++;
                    NoticeService.UpdateEntitySaved(notice);
                    HttpContext.Session.Set("notice" + notice.Id, notice.Id.GetBytes());
                }
                return ResultData(notice.Mapper<NoticeOutputDto>());
            }
            return ResultData(null, false);
        }
    }
}