namespace Masuit.MyBlogs.Core.Models.DTO;

/// <summary>
/// 文章修改请求
/// </summary>
public class PostMergeRequestDto : PostMergeRequestDtoBase
{
	/// <summary>
	/// 文章内容
	/// </summary>
	public string Content { get; set; }
}