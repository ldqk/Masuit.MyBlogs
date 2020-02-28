using EFSecondLevelCache.Core;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.Tools;
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
        public ActionResult Index(int id)
        {
            var misc = MiscService.GetFromCache(m => m.Id == id) ?? throw new NotFoundException("页面未找到");
            return View(misc);
        }

        /// <summary>
        /// 打赏
        /// </summary>
        /// <returns></returns>
        [Route("donate")]
        public ActionResult Donate()
        {
            return CurrentUser.IsAdmin ? View("Donate_Admin") : View();
        }

        /// <summary>
        /// 打赏列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("donatelist")]
        public ActionResult DonateList(int page = 1, int size = 10)
        {
            var list = DonateService.GetPages(page, size, out int total, d => true, d => d.DonateTime, false).Select(d => new
            {
                d.NickName,
                d.EmailDisplay,
                d.QQorWechatDisplay,
                d.DonateTime,
                d.Amount,
                d.Via
            }).Cacheable().ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
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
        public ActionResult Delete(int id)
        {
            var post = MiscService.GetById(id) ?? throw new NotFoundException("杂项页已被删除！");
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

            bool b = MiscService.DeleteByIdSaved(id);
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
            var entity = MiscService.GetById(misc.Id) ?? throw new NotFoundException("杂项页未找到");
            entity.ModifyDate = DateTime.Now;
            entity.Title = misc.Title;
            entity.Content = await ImagebedClient.ReplaceImgSrc(misc.Content.ClearImgAttributes());
            bool b = MiscService.SaveChanges() > 0;
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
            var list = MiscService.GetPagesNoTracking(page, size, out int total, n => true, n => n.ModifyDate, false).Select(m => new
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
        [MyAuthorize]
        public ActionResult Get(int id)
        {
            var notice = MiscService.GetById(id);
            return ResultData(notice.MapTo<MiscOutputDto>());
        }
    }
}