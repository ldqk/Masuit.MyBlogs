using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Core;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 文章分类
/// </summary>
public sealed class CategoryController : BaseController
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
		var categories = CategoryService.GetQuery<string, CategoryCommand>(c => c.Status == Status.Available, c => c.Name).ToListWithNoLock();
		var list = Mapper.Map<List<CategoryDto>>(categories);
		return ResultData(list.ToTree());
	}

	/// <summary>
	/// 获取分类详情
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public async Task<ActionResult> Get(int id)
	{
		var model = await CategoryService.GetByIdAsync(id) ?? throw new NotFoundException("分类不存在！");
		return ResultData(Mapper.Map<CategoryDto>(model));
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
			var category = Mapper.Map<Category>(cmd);
			category.Path = cmd.ParentId > 0 ? (CategoryService[cmd.ParentId.Value].Path + "," + cmd.ParentId).Trim(',') : SnowFlake.NewId;
			var b1 = await CategoryService.AddEntitySavedAsync(category) > 0;
			return ResultData(null, b1, b1 ? "分类添加成功！" : "分类添加失败！");
		}

		cat.Name = cmd.Name;
		cat.Description = cmd.Description;
		cat.ParentId = cmd.ParentId;
		cat.Path = cmd.ParentId > 0 ? (CategoryService[cmd.ParentId.Value].Path + "," + cmd.ParentId).Trim(',') : SnowFlake.NewId;
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