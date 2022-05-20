using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Command;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Models;
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
        public ActionResult GetCategories()
        {
            var list = CategoryService.GetQueryNoTracking(c => c.Status == Status.Available, c => c.Name).ToList().ToTree(c => c.Id, c => c.ParentId);
            return ResultData(list.Mapper<List<CategoryDto>>());
        }

        /// <summary>
        /// 获取分类详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Get(int id)
        {
            var model = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("分类不存在！");
            return ResultData(model.Mapper<CategoryDto>());
        }

        /// <summary>
        /// 保存分类
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Save([FromBodyOrDefault] CategoryCommand cmd)
        {
            var cat = await CategoryService.GetByIdAsync(cmd.Id);
            if (cat == null)
            {
                var b1 = await CategoryService.AddEntitySavedAsync(Mapper.Map<Category>(cmd)) > 0;
                return ResultData(null, b1, b1 ? "分类添加成功！" : "分类添加失败！");
            }

            cat.Name = cmd.Name;
            cat.Description = cmd.Description;
            cat.ParentId = cmd.ParentId;
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
