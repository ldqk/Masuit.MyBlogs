using Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 杂项页
    /// </summary>
    public class MiscController : BaseController
    {
        /// <summary>
        /// MiscService
        /// </summary>
        public IMiscService MiscService { get; set; }

        /// <summary>
        /// 捐赠
        /// </summary>
        public IDonateService DonateService { get; set; }

        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// 杂项页
        /// </summary>
        /// <param name="miscService"></param>
        /// <param name="donateService"></param>
        /// <param name="hostingEnvironment"></param>
        public MiscController(IMiscService miscService, IDonateService donateService, IHostingEnvironment hostingEnvironment)
        {
            MiscService = miscService;
            DonateService = donateService;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 杂项页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("misc/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Index(int id)
        {
            Misc misc = MiscService.GetById(id);
            if (misc is null)
            {
                return RedirectToAction("Index", "Error");
            }
            return View(misc);
        }

        /// <summary>
        /// 捐赠
        /// </summary>
        /// <returns></returns>
        [Route("donate")]
        public ActionResult Donate()
        {
            var user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            if (user != null && user.IsAdmin)
            {
                return View("Donate_Admin");
            }
            return View();
        }

        /// <summary>
        /// 捐赠列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("donatelist")]
        public ActionResult DonateList(int page = 1, int size = 10)
        {
            var list = DonateService.LoadPageEntitiesFromL2CacheNoTracking(page, size, out int total, d => true, d => d.DonateTime, false).Select(d => new
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

        /// <summary>
        /// 关于
        /// </summary>
        /// <returns></returns>
        [Route("about"), ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 声明
        /// </summary>
        /// <returns></returns>
        [Route("disclaimer"), ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Disclaimer()
        {
            return View();
        }

        /// <summary>
        /// 创建页面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Write(Misc model)
        {
            model.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(model.Content?.Trim(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            var e = MiscService.AddEntitySaved(model);
            if (e != null)
            {
                return ResultData(null, message: "发布成功");
            }
            return ResultData(null, false, "发布失败");
        }

        /// <summary>
        /// 删除页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete(int id)
        {
            var post = MiscService.GetById(id);
            if (post is null)
            {
                return ResultData(null, false, "杂项页已经被删除！");
            }

            var srcs = post.Content.MatchImgSrcs();
            foreach (var path in srcs)
            {
                if (path.StartsWith("/"))
                {
                    try
                    {
                        System.IO.File.Delete(Path.Combine(_hostingEnvironment.WebRootPath + path));
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            bool b = MiscService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 编辑页面
        /// </summary>
        /// <param name="misc"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Edit(Misc misc)
        {
            var entity = MiscService.GetById(misc.Id);
            entity.ModifyDate = DateTime.Now;
            entity.Title = misc.Title;
            entity.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(misc.Content, @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            bool b = MiscService.UpdateEntitySaved(entity);
            return ResultData(null, b, b ? "修改成功" : "修改失败");
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = MiscService.LoadPageEntitiesNoTracking(page, size, out int total, n => true, n => n.ModifyDate, false).Select(m => new
            {
                m.Id,
                m.Title,
                m.ModifyDate,
                m.PostDate
            }).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Get(int id)
        {
            var notice = MiscService.GetById(id);
            return ResultData(notice.MapTo<MiscOutputDto>());
        }
    }
}