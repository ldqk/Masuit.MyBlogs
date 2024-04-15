using Masuit.MyBlogs.Core.Models.Validation;
using Masuit.Tools.Core.Validator;

namespace Masuit.MyBlogs.Core.Models.Command;

/// <summary>
/// 评论表输入模型
/// </summary>
public class CommentCommand : BaseEntity
{
	public CommentCommand()
	{
		Status = Status.Pending;
	}

	/// <summary>
	/// 昵称
	/// </summary>
	[Required(ErrorMessage = "昵称不能为空！"), MaxLength(36, ErrorMessage = "昵称最多只能24个字符！"), MinLength(2, ErrorMessage = "昵称至少2个字！")]
	public string NickName { get; set; }

	/// <summary>
	/// 邮箱
	/// </summary>
	[IsEmail]
	public string Email { get; set; }

	/// <summary>
	/// 评论内容
	/// </summary>
	[Required(ErrorMessage = "评论内容不能为空！"), SubmitCheck(2, 500)]
	public string Content { get; set; }

	/// <summary>
	/// 父级ID
	/// </summary>
	public int? ParentId { get; set; }

	/// <summary>
	/// 文章ID
	/// </summary>
	public int PostId { get; set; }

	/// <summary>
	/// 浏览器版本
	/// </summary>
	[StringLength(255)]
	public string Browser { get; set; }

	/// <summary>
	/// 操作系统版本
	/// </summary>
	[StringLength(255)]
	public string OperatingSystem { get; set; }

	/// <summary>
	/// 验证码
	/// </summary>
	public string Code { get; set; }

	/// <summary>
	/// 是否已接受条款
	/// </summary>
	[AssignTrue(ErrorMessage = "请先同意接受本站的《评论须知》")]
	public bool Agree { get; set; }
}