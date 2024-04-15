namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 导航菜单
/// </summary>
[Table("Menu")]
public class Menu : BaseEntity, ITree<Menu>, ITreeEntity<Menu, int>
{
    public Menu()
    {
        ParentId = 0;
        Status = Status.Available;
        Children = new List<Menu>();
    }

    /// <summary>
    /// 名字
    /// </summary>
    [Required(ErrorMessage = "菜单名不能为空！")]
    public string Name { get; set; }

    /// <summary>
    /// 父节点
    /// </summary>
    public Menu Parent { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public ICollection<Menu> Children { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// URL
    /// </summary>
    [Required(ErrorMessage = "菜单的URL不能为空！")]
    public string Url { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 父级ID
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 菜单类型
    /// </summary>
    public virtual MenuType MenuType { get; set; }

    /// <summary>
    /// 是否在新标签页打开
    /// </summary>
    public bool NewTab { get; set; }

    public string Path { get; set; }
}
