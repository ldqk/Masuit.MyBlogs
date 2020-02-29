using EFSecondLevelCache.Core;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Masuit.MyBlogs.Core.Models.Command;

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
            var list = CategoryService.GetQuery<string, CategoryDto>(c => c.Status == Status.Available, c => c.Name).Cacheable().ToList();
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
        /// 添加分类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ActionResult> Add(Category model)
        {
            bool exist = CategoryService.Any(c => c.Name.Equals(model.Name));
            if (exist)
            {
                return ResultData(null, false, $"分类{model.Name}已经存在！");
            }
            var b = await CategoryService.AddEntitySavedAsync(model) > 0;
            if (b)
            {
                return ResultData(null, true, "分类添加成功！");
            }

            return ResultData(null, false, "分类添加失败！");
        }

        /// <summary>
        /// 编辑分类
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<ActionResult> Edit(CategoryCommand dto)
        {
            Category cat = await CategoryService.GetByIdAsync(dto.Id) ?? throw new NotFoundException("分类不存在！");
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
        public ActionResult Delete(int id, int cid = 1)
        {
            bool b = CategoryService.Delete(id, cid);
            return ResultData(null, b, b ? "分类删除成功" : "分类删除失败");
        }
    }
}