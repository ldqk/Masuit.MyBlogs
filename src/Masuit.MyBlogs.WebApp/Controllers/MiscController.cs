using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Common;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class MiscController : BaseController
    {
        public IMiscBll MiscBll { get; set; }
        public IDonateBll DonateBll { get; set; }

        public MiscController(IMiscBll miscBll, IDonateBll donateBll)
        {
            MiscBll = miscBll;
            DonateBll = donateBll;
        }

        [Route("misc/{id:int}")]
        public ActionResult Index(int id)
        {
            Misc misc = MiscBll.GetById(id);
            if (misc is null)
            {
                return RedirectToAction("Index", "Error");
            }
            return View(misc);
        }

        [Route("donate")]
        public ActionResult Donate()
        {
            var user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            if (user != null && user.IsAdmin)
            {
                return View("Donate_Admin");
            }
            return View();
        }

        [Route("donatelist")]
        public ActionResult DonateList(int page = 1, int size = 10)
        {
            var list = DonateBll.LoadPageEntitiesFromL2CacheNoTracking(page, size, out int total, d => true, d => d.DonateTime, false).Select(d => new
            {
                d.NickName,
                d.EmailDisplay,
                d.QQorWechatDisplay,
                d.DonateTime,
                d.Amount,
                d.Via
            }).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        [Route("about")]
        public ActionResult About()
        {
            return View();
        }

        [Route("disclaimer")]
        public ActionResult Disclaimer()
        {
            return View();
        }

        [ValidateInput(false), Authority]
        public ActionResult Write(Misc model)
        {
            model.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(model.Content?.Trim(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            var e = MiscBll.AddEntitySaved(model);
            if (e != null)
            {
                return ResultData(null, message: "发布成功");
            }
            return ResultData(null, false, "发布失败");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            var post = MiscBll.GetById(id);
            if (post is null)
            {
                return ResultData(null, false, "杂项页已经被删除！");
            }

            var mc = post.Content.MatchImgTags();
            foreach (Match m in mc)
            {
                string path = m.Groups[3].Value;
                if (path.StartsWith("/"))
                {
                    path = Path.Combine(Server.MapPath("/"), path);
                    try
                    {
                        System.IO.File.Delete(path);
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            bool b = MiscBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        [ValidateInput(false), Authority]
        public ActionResult Edit(Misc misc)
        {
            var entity = MiscBll.GetById(misc.Id);
            entity.ModifyDate = DateTime.Now;
            entity.Title = misc.Title;
            entity.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(misc.Content, @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            bool b = MiscBll.UpdateEntitySaved(entity);
            return ResultData(null, b, b ? "修改成功" : "修改失败");
        }

        [Authority]
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = MiscBll.LoadPageEntitiesNoTracking(page, size, out int total, n => true, n => n.ModifyDate, false).Select(m => new
            {
                m.Id,
                m.Title,
                m.ModifyDate,
                m.PostDate
            }).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        [Authority]
        public ActionResult Get(int id)
        {
            var notice = MiscBll.GetById(id);
            return ResultData(notice.MapTo<MiscOutputDto>());
        }
    }
}