using Masuit.Tools.Core.Validator;

namespace Masuit.MyBlogs.Core.Models.Command;

/// <summary>
/// 文章修改请求
/// </summary>
public class PostMergeRequestCommand : PostMergeRequestCommandBase
{
	/// <summary>
	/// 文章id
	/// </summary>
	public int PostId { get; set; }

	/// <summary>
	/// 修改人
	/// </summary>
	[Required, MaxLength(36, ErrorMessage = "修改人名字最长支持36个字符！"), MinLength(2, ErrorMessage = "修改人名字最少2个字符！")]
	public string Modifier { get; set; }

	/// <summary>
	/// 修改人邮箱
	/// </summary>
	[Required(ErrorMessage = "邮箱不能为空！"), MinLength(6, ErrorMessage = "邮箱格式不正确！"), IsEmail]
	public string ModifierEmail { get; set; }
	/// <summary>
	/// 验证码
	/// </summary>
	[Required(ErrorMessage = "验证码不能为空！")]
	public string Code { get; set; }
}