namespace Masuit.MyBlogs.Core.Models.DTO;

/// <summary>
/// 文章分类输出模型
/// </summary>
public class CategoryDto_P : BaseDto, ITreeParent<CategoryDto_P>
{
    /// <summary>
    /// 分类名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 分类描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 父节点
    /// </summary>
    public CategoryDto_P Parent { get; set; }
}
