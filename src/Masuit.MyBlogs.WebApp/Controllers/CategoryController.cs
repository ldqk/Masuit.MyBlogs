using System.Linq;
using System.Web.Mvc;
using Common;
using IBLL;
using Models.DTO;
using Models.Entity;
using Models.Enum;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class CategoryController : BaseController
    {
        public ICategoryBll CategoryBll { get; set; }

        public CategoryController(ICategoryBll categoryBll)
        {
            CategoryBll = categoryBll;
        }

        public ActionResult GetCategories()
        {
            var list = CategoryBll.LoadEntitiesNoTracking<string, CategoryOutputDto>(c => c.Status == Status.Available, c => c.Name).Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Post.Count
            }).ToList();
            return ResultData(list);
        }

        public ActionResult Get(int id)
        {
            var model = CategoryBll.GetById(id);
            return ResultData(model.Mapper<CategoryOutputDto>());
        }

        public ActionResult Add(Category model)
        {
            bool exist = CategoryBll.Any(c => c.Name.Equals(model.Name));
            if (exist)
            {
                return ResultData(null, false, $"分类{model.Name}已经存在！");
            }
            var cat = CategoryBll.AddEntitySaved(model);
            if (cat != null)
            {
                return ResultData(null, true, "分类添加成功！");
            }
            return ResultData(null, false, "分类添加失败！");
        }

        public ActionResult Edit(CategoryInputDto dto)
        {
            Category cat = CategoryBll.GetById(dto.Id);
            cat.Name = dto.Name;
            cat.Description = dto.Description;
            bool b = CategoryBll.UpdateEntitySaved(cat);
            return ResultData(null, b, b ? "分类修改成功！" : "分类修改失败！");
        }

        public ActionResult Delete(int id, int cid = 1)
        {
            Category category = CategoryBll.GetById(id);
            Category moveCat = CategoryBll.GetById(cid);
            category.Post.ForEach(p =>
            {
                moveCat.Post.Add(p);
            });
            CategoryBll.UpdateEntity(moveCat);
            bool b = CategoryBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "分类删除成功" : "分类删除失败");
        }
    }
}