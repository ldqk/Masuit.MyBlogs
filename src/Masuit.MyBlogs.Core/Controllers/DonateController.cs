using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
        public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")]int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")]int size = 15)
        {
            var list = DonateService.GetPages(page, size, d => true, d => d.DonateTime, false);
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
            var b = await DonateService.AddOrUpdateSavedAsync(d => d.Id, donate) > 0;
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