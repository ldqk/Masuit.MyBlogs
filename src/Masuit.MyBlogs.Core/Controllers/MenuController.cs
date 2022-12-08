using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 菜单管理
/// </summary>
public sealed class MenuController : AdminController
{
	/// <summary>
	/// 菜单数据服务
	/// </summary>
	public IMenuService MenuService { get; set; }

	/// <summary>
	/// 获取菜单
	/// </summary>
	/// <returns></returns>
	public ActionResult GetMenus()
	{
		var list = MenuService.GetAllNoTracking(m => m.Sort).ToList();
		var menus = list.ToTree(m => m.Id, m => m.ParentId);
		return ResultData(Mapper.Map<List<MenuDto>>(menus));
	}

	/// <summary>
	/// 获取菜单类型
	/// </summary>
	/// <returns></returns>
	[ResponseCache(Duration = 86400)]
	public ActionResult GetMenuType()
	{
		var array = Enum.GetValues(typeof(MenuType));
		var list = new List<object>();
		foreach (Enum e in array)
		{
			list.Add(new
			{
				e,
				name = e.GetDisplay()
			});
		}
		return ResultData(list);
	}

	/// <summary>
	/// 删除菜单
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public async Task<ActionResult> Delete(int id)
	{
		var menus = MenuService[id].Flatten();
		bool b = await MenuService.DeleteEntitiesSavedAsync(menus) > 0;
		return ResultData(null, b, b ? "删除成功" : "删除失败");
	}

	/// <summary>
	/// 保持菜单
	/// </summary>
	/// <param name="model"></param>
	/// <returns></returns>
	public async Task<ActionResult> Save([FromBodyOrDefault] MenuCommand model)
	{
		if (string.IsNullOrEmpty(model.Icon) || !model.Icon.Contains("/"))
		{
			model.Icon = null;
		}
		var m = await MenuService.GetByIdAsync(model.Id);
		if (m == null)
		{
			var menu = Mapper.Map<Menu>(model);
			menu.Path = model.ParentId > 0 ? (MenuService[model.ParentId.Value].Path + "," + model.ParentId).Trim(',') : SnowFlake.NewId;
			return await MenuService.AddEntitySavedAsync(menu) > 0 ? ResultData(model, true, "添加成功") : ResultData(null, false, "添加失败");
		}

		Mapper.Map(model, m);
		m.Path = model.ParentId > 0 ? (MenuService[model.ParentId.Value].Path + "," + model.ParentId).Trim(',') : SnowFlake.NewId;
		bool b = await MenuService.SaveChangesAsync() > 0;
		QueryCacheManager.ExpireType<Menu>();
		return ResultData(null, b, b ? "修改成功" : "修改失败");
	}
}