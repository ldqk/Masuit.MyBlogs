using Common;
using EFSecondLevelCache.Core;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Linq;

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
        /// 文章分类
        /// </summary>
        /// <param name="categoryService"></param>
        public CategoryController(ICategoryService categoryService)
        {
            CategoryService = categoryService;
        }

        /// <summary>
        /// 获取所有分类
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult GetCategories()
        {
            var list = CategoryService.LoadEntities<string, CategoryOutputDto>(c => c.Status == Status.Available, c => c.Name).Cacheable().ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 获取分类详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Get(int id)
        {
            var model = CategoryService.GetById(id);
            return ResultData(model.Mapper<CategoryOutputDto>());
        }

        /// <summary>
        /// 添加分类
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Add(Category model)
        {
            bool exist = CategoryService.Any(c => c.Name.Equals(model.Name));
            if (exist)
            {
                return ResultData(null, false, $"分类{model.Name}已经存在！");
            }
            var cat = CategoryService.AddEntitySaved(model);
            if (cat != null)
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
        public ActionResult Edit(CategoryInputDto dto)
        {
            Category cat = CategoryService.GetById(dto.Id);
            cat.Name = dto.Name;
            cat.Description = dto.Description;
            bool b = CategoryService.UpdateEntitySaved(cat);
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