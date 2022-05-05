using EFCoreSecondLevelCacheInterceptor;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Command;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.AspNetCore.ModelBinder;
using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 文章分类
    /// </summary>
    public class CategoryController : BaseController
    {
        /// <summary>
        /// CategoryService
        /// </summary>
        public ICategoryService CategoryService { get; set; }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600)]
        public ActionResult GetCategories()
        {
            var list = CategoryService.GetQuery<string, CategoryDto>(c => c.Status == Status.Available && c.ParentId == null, c => c.Name).NotCacheable().ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 获取分类详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" })]
        public async Task<ActionResult> Get(int id)
        {
            var model = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("分类不存在！");
            return ResultData(model.Mapper<CategoryDto>());
        }

        /// <summary>
        /// 保存分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Save([FromBodyOrDefault] CategoryCommand dto)
        {
            var cat = await CategoryService.GetByIdAsync(dto.Id);
            if (cat == null)
            {
                var b1 = await CategoryService.AddEntitySavedAsync(Mapper.Map<Category>(dto)) > 0;
                return ResultData(null, b1, b1 ? "分类添加成功！" : "分类添加失败！");
            }

            cat.Name = dto.Name;
            cat.Description = dto.Description;
            bool b = await CategoryService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? "分类修改成功！" : "分类修改失败！");
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Delete(int id, int cid = 1)
        {
            bool b = await CategoryService.Delete(id, cid);
            return ResultData(null, b, b ? "分类删除成功" : "分类删除失败");
        }
    }
}
