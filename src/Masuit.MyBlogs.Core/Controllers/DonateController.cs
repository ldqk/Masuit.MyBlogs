using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 打赏管理
    /// </summary>
    public class DonateController : AdminController
    {
        /// <summary>
        /// DonateService
        /// </summary>
        public IDonateService DonateService { get; set; }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = DonateService.GetPagesFromCache(page, size, d => true, d => d.DonateTime, false);
            return Ok(list);
        }

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Get(int id)
        {
            Donate donate = await DonateService.GetByIdAsync(id) ?? throw new NotFoundException("条目不存在！");
            return ResultData(donate);
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="donate"></param>
        /// <returns></returns>
        public async Task<ActionResult> Save(Donate donate)
        {
            var entry = await DonateService.GetByIdAsync(donate.Id);
            bool b;
            if (entry is null)
            {
                b = await DonateService.AddEntitySavedAsync(donate) > 0;
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
                b = await DonateService.SaveChangesAsync() > 0;
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