using Masuit.Tools.Models;

namespace Masuit.MyBlogs.Core.Models.DTO;

/// <summary>
/// 文章分类输出模型
/// </summary>
public class CategoryDto : BaseDto, ITreeEntity<CategoryDto, int>
{
    /// <summary>
    /// 分类名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 分类描述
    /// </summary>
    public string Description { get; set; }

    public int? ParentId { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public ICollection<CategoryDto> Children { get; set; }
}
