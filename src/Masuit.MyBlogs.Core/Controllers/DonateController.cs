using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 捐赠管理
    /// </summary>
    public class DonateController : AdminController
    {
        /// <summary>
        /// DonateService
        /// </summary>
        public IDonateService DonateService { get; set; }

        /// <summary>
        /// 捐赠管理
        /// </summary>
        /// <param name="donateService"></param>
        public DonateController(IDonateService donateService)
        {
            DonateService = donateService;
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = DonateService.LoadPageEntitiesFromL2CacheNoTracking(page, size, out int total, d => true, d => d.DonateTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Get(int id)
        {
            Donate donate = DonateService.GetById(id);
            return ResultData(donate);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="donate"></param>
        /// <returns></returns>
        public ActionResult Save(Donate donate)
        {
            var entry = DonateService.GetById(donate.Id);
            bool b;
            if (entry is null)
            {
                b = DonateService.AddEntitySaved(donate) != null;
            }
            else
            {
                entry.NickName = donate.NickName;
                entry.Amount = donate.Amount;
                entry.DonateTime = donate.DonateTime;
                entry.Email = donate.Email;
                entry.EmailDisplay = donate.EmailDisplay;
                entry.QQorWechat = donate.QQorWechat;
                entry.QQorWechatDisplay = donate.QQorWechatDisplay;
                entry.Via = donate.Via;
                b = DonateService.UpdateEntitySaved(entry);
            }
            return ResultData(null, b, b ? "保存成功！" : "保存失败！");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            bool b = DonateService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }
    }
}