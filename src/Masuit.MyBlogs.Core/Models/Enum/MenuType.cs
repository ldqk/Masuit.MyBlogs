namespace Masuit.MyBlogs.Core.Models.Enum;

public enum MenuType
{
	/// <summary>
	/// 主菜单
	/// </summary>
	[Display(Name = "主菜单")] MainMenu,

	/// <summary>
	/// 子菜单
	/// </summary>
	[Display(Name = "子菜单")] SubMenu,

	/// <summary>
	/// 图片菜单
	/// </summary>
	[Display(Name = "图片菜单")] GalleryMenu,

	/// <summary>
	/// 图标菜单
	/// </summary>
	[Display(Name = "图标菜单")] IconMenu
}