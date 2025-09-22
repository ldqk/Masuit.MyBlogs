using Masuit.MyBlogs.Core.Models.Validation;
using Masuit.Tools.Core.Validator;

namespace Masuit.MyBlogs.Core.Models.DTO;

/// <summary>
/// 文章输入模型
/// </summary>
public class PostCommand : BaseEntity
{
    public PostCommand()
    {
        Status = Status.Pending;
    }

    /// <summary>
    /// 标题
    /// </summary>
    [Required(ErrorMessage = "文章标题不能为空！"), MaxLength(128, ErrorMessage = "文章标题最长支持128个字符！"), MinLength(4, ErrorMessage = "文章标题最少4个字符！")]
    public string Title { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [Required, MaxLength(24, ErrorMessage = "作者名最长支持24个字符！"), MinLength(2, ErrorMessage = "作者名最少2个字符！")]
    public string Author { get; set; }

    public string Modifier { get; set; }

    public string ModifierEmail { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required(ErrorMessage = "文章内容不能为空！"), SubmitCheck(20, 1000000, false)]
    public string Content { get; set; }

    /// <summary>
    /// 文章关键词
    /// </summary>
    [StringLength(256, ErrorMessage = "文章关键词最大允许255个字符")]
    public string Keyword { get; set; }

    /// <summary>
    /// 受保护的内容
    /// </summary>
    public string ProtectContent { get; set; }

    /// <summary>
    /// 受保护内容模式
    /// </summary>
    public ProtectContentMode ProtectContentMode { get; set; }

    /// <summary>
    /// 分类id
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// 作者邮箱
    /// </summary>
    [Required(ErrorMessage = "作者邮箱不能为空！"), MinLength(6, ErrorMessage = "邮箱格式不正确！"), IsEmail]
    public string Email { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    [StringLength(255, ErrorMessage = "标签最大允许255个字符")]
    public string Label { get; set; }

    /// <summary>
    /// 专题
    /// </summary>
    public int[] Seminars { get; set; }

    /// <summary>
    /// 禁止评论
    /// </summary>
    public bool DisableComment { get; set; }

    /// <summary>
    /// 禁止转载
    /// </summary>
    public bool DisableCopy { get; set; }

    /// <summary>
    /// 限制模式
    /// </summary>
    public RegionLimitMode? LimitMode { get; set; }

    /// <summary>
    /// 限制地区，竖线分隔
    /// </summary>
    public string Regions { get; set; }

    /// <summary>
    /// 限制排除地区，竖线分隔
    /// </summary>
    public string ExceptRegions { get; set; }

    /// <summary>
    /// 限制模式
    /// </summary>
    public RegionLimitMode? ProtectContentLimitMode { get; set; }

    /// <summary>
    /// 限制地区，竖线分隔
    /// </summary>
    public string ProtectContentRegions { get; set; }

    /// <summary>
    /// 保护密码
    /// </summary>
    public string ProtectPassword { get; set; }

    /// <summary>
    /// 跳转到第三方链接
    /// </summary>
    public string Redirect { get; set; }

    /// <summary>
    /// 保留历史版本
    /// </summary>
    public bool Reserve { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpireAt { get; set; }

    /// <summary>
    /// 是否是不安全内容
    /// </summary>
    public bool IsNsfw { get; set; }
}