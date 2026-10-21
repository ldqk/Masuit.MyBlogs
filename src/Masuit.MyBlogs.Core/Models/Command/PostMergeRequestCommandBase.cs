using Masuit.MyBlogs.Core.Models.Validation;
using System.Net;

namespace Masuit.MyBlogs.Core.Models.Command;

/// <summary>
/// 文章修改请求
/// </summary>
public class PostMergeRequestCommandBase : BaseDto,IValidatableObject
{
    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "文章标题不能为空！"), MaxLength(128, ErrorMessage = "文章标题最长支持128个字符！"), MinLength(4, ErrorMessage = "文章标题最少4个字符！")]
    public string Title { get; set; }

    /// <summary>
    /// 文章内容
    /// </summary>
    [Required(ErrorMessage = "文章内容不能为空！"), SubmitCheck(20, 1000000, false)]
    public string Content { get; set; }

    /// <summary>Determines whether the specified object is valid.</summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection that holds failed-validation information.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        Title= WebUtility.HtmlEncode(Title);
        yield break;
    }
}