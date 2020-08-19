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
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        /// 打赏
        /// </summary>
        public IDonateService DonateService { get; set; }

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
            return View(misc);
        }

        /// <summary>
        /// 打赏
        /// </summary>
        /// <returns></returns>
        [Route("donate")]
        public ActionResult Donate()
        {
            ViewBag.Ads = AdsService.GetsByWeightedPrice(2, AdvertiseType.InPage);
            return CurrentUser.IsAdmin ? View("Donate_Admin") : View();
        }

        /// <summary>
        /// 打赏列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("donatelist")]
        public async Task<ActionResult> DonateList(int page = 1, int size = 10)
        {
            var list = await DonateService.GetPagesFromCacheAsync<DateTime, DonateDto>(page, size, d => true, d => d.DonateTime, false);
            if (!CurrentUser.IsAdmin)
            {
                foreach (var item in list.Data.Where(item => !(item.QQorWechat + item.Email).Contains("匿名")))
                {
                    item.QQorWechat = item.QQorWechat.Mask();
                    item.Email = item.Email.MaskEmail();
                }
            }

            return Ok(list);
        }

        /// <summary>
        /// 关于
        /// </summary>
        /// <returns></returns>
        [Route("about"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 评论及留言须知
        /// </summary>
        /// <returns></returns>
        [Route("agreement"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public ActionResult Agreement()
        {
            return View();
        }

        /// <summary>
        /// 声明
        /// </summary>
        /// <returns></returns>
        [Route("disclaimer"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public ActionResult Disclaimer()
        {
            return View();
        }

        /// <summary>
        /// 创建页面
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Write(Misc model)
        {
            model.Content = await ImagebedClient.ReplaceImgSrc(model.Content.Trim().ClearImgAttributes());
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
            var post = await MiscService.GetByIdAsync(id) ?? throw new NotFoundException("杂项页已被删除！");
            var srcs = post.Content.MatchImgSrcs().Where(s => s.StartsWith("/"));
            foreach (var path in srcs)
            {
                try
                {
                    System.IO.File.Delete(Path.Combine(HostEnvironment.WebRootPath + path));
                }
                catch (IOException)
                {
                }
            }

            bool b = await MiscService.DeleteByIdSavedAsync(id) > 0;
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 编辑页面
        /// </summary>
        /// <param name="misc"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Edit(Misc misc)
        {
            var entity = await MiscService.GetByIdAsync(misc.Id) ?? throw new NotFoundException("杂项页未找到");
            entity.ModifyDate = DateTime.Now;
            entity.Title = misc.Title;
            entity.Content = await ImagebedClient.ReplaceImgSrc(misc.Content.ClearImgAttributes());
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

            return ResultData(misc.MapTo<MiscDto>());
        }
    }
}