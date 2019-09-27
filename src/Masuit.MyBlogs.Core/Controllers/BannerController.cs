using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// banner
    /// </summary>
    [Route("[controller]/[action]")]
    public class BannerController : AdminController
    {
        /// <summary>
        /// bannerService
        /// </summary>
        public IBannerService BannerService { get; set; }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Get()
        {
            return ResultData(await BannerService.GetAllFromL2CacheNoTracking(b => b.Id, false).ToListAsync());
        }

        /// <summary>
        /// 保存banner
        /// </summary>
        /// <param name="banner"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Save(Banner banner)
        {
            var entity = BannerService.GetById(banner.Id);
            if (entity != null)
            {
                entity.Url = banner.Url;
                entity.Description = banner.Description;
                entity.ImageUrl = banner.ImageUrl;
                entity.Title = banner.Title;
                bool b1 = await BannerService.UpdateEntitySavedAsync(entity) > 0;
                return ResultData(null, b1, b1 ? "修改成功" : "修改失败");
            }

            bool b = BannerService.AddEntitySaved(banner) != null;
            return ResultData(null, b, b ? "添加成功" : "添加失败");
        }

        /// <summary>
        /// 删除banner
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}"), HttpGet("{id}")]
        public IActionResult Delete(int id)
        {
            bool b = BannerService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }
    }
}