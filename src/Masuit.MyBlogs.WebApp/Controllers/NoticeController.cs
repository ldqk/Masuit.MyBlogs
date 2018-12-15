using Common;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class NoticeController : BaseController
    {
        public INoticeBll NoticeBll { get; set; }

        public NoticeController(INoticeBll noticeBll)
        {
            NoticeBll = noticeBll;
        }

        [Route("notice")]
        public ActionResult Index(int page = 1, int size = 10, int id = 0)
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            List<NoticeOutputDto> list;
            int total;
            if (user != null && user.IsAdmin)
            {
                if (id != 0)
                {
                    Notice notice = NoticeBll.GetById(id);
                    ViewBag.Total = 1;
                    return View("Index_Admin", new List<NoticeOutputDto> { notice.MapTo<NoticeOutputDto>() });
                }
                list = NoticeBll.LoadPageEntitiesNoTracking<DateTime, NoticeOutputDto>(page, size, out total, n => n.Status == Status.Display, n => n.ModifyDate, false).ToList();
                ViewBag.Total = total;
                return View("Index_Admin", list);
            }
            list = NoticeBll.LoadPageEntitiesNoTracking<DateTime, NoticeOutputDto>(page, size, out total, n => n.Status == Status.Display, n => n.ModifyDate, false).ToList();
            ViewBag.Total = total;
            return View(list);
        }

        [Route("n/{id:int}")]
        public ActionResult Details(int id)
        {
            Notice notice = NoticeBll.GetById(id);
            if (notice != null)
            {
                return View(notice);
            }
            return RedirectToAction("Index");
        }

        [ValidateInput(false), Authority]
        public ActionResult Write(Notice notice)
        {
            notice.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(notice.Content, @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            Notice e = NoticeBll.AddEntitySaved(notice);
            if (e != null)
            {
                return ResultData(null, message: "发布成功");
            }
            return ResultData(null, false, "发布失败");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            var post = NoticeBll.GetById(id);
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
                        System.IO.File.Delete(Path.Combine(Server.MapPath("/"), path));
                    }
                    catch
                    {
                    }
                }
            }
            bool b = NoticeBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        [ValidateInput(false), Authority]
        public ActionResult Edit(Notice notice)
        {
            Notice entity = NoticeBll.GetById(notice.Id);
            entity.ModifyDate = DateTime.Now;
            entity.Title = notice.Title;
            entity.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(notice.Content, @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            bool b = NoticeBll.UpdateEntitySaved(entity);
            return ResultData(null, b, b ? "修改成功" : "修改失败");
        }

        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            List<Notice> list = NoticeBll.LoadPageEntitiesNoTracking(page, size, out int total, n => true, n => n.ModifyDate, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        public ActionResult Get(int id)
        {
            Notice notice = NoticeBll.GetById(id);
            if (Session["notice" + id] is null)
            {
                notice.ViewCount++;
                NoticeBll.UpdateEntitySaved(notice);
                Session["notice" + id] = id;
            }
            return ResultData(notice.MapTo<NoticeOutputDto>());
        }

        public ActionResult Last()
        {
            var notice = NoticeBll.GetFirstEntityFromL2Cache(n => n.Status == Status.Display, n => n.ModifyDate, false);
            if (notice != null)
            {
                if (Session["notice" + notice.Id] is null)
                {
                    notice.ViewCount++;
                    NoticeBll.UpdateEntitySaved(notice);
                    Session["notice" + notice.Id] = +notice.Id;
                }
                return ResultData(notice.Mapper<NoticeOutputDto>());
            }
            return ResultData(null, false);
        }
    }
}