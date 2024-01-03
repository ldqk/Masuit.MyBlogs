namespace Masuit.MyBlogs.Core.Models.DTO;

/// <summary>
/// 导航菜单输出模型
/// </summary>
public class MenuDto : BaseDto, ITreeChildren<MenuDto>
{
	/// <summary>
	/// 名字
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// 子级
	/// </summary>
	public ICollection<MenuDto> Children { get; set; }

	/// <summary>
	/// 图标
	/// </summary>
	public string Icon { get; set; }

	/// <summary>
	/// URL
	/// </summary>
	public string Url { get; set; }

	/// <summary>
	/// 排序号
	/// </summary>
	public int Sort { get; set; }

	/// <summary>
	/// 父级ID
	/// </summary>
	public int ParentId { get; set; }

	/// <summary>
	/// 菜单类型
	/// </summary>
	public MenuType MenuType { get; set; }

	/// <summary>
	/// 是否在新标签页打开
	/// </summary>
	public bool NewTab { get; set; }
}