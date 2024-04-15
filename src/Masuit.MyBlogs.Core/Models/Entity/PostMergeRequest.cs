namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 文章修改请求
/// </summary>
[Table("PostMergeRequest")]
public class PostMergeRequest : BaseEntity
{
	/// <summary>
	/// 文章id
	/// </summary>
	public int PostId { get; set; }

	/// <summary>
	/// 标题
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// 文章内容
	/// </summary>
	public string Content { get; set; }

	/// <summary>
	/// 修改人
	/// </summary>
	public string Modifier { get; set; }

	/// <summary>
	/// 修改人邮箱
	/// </summary>
	public string ModifierEmail { get; set; }

	/// <summary>
	/// 合并状态
	/// </summary>
	public MergeStatus MergeState { get; set; }

	/// <summary>
	/// 提交时间
	/// </summary>
	public DateTime SubmitTime { get; set; }

	[ForeignKey("PostId")]
	public virtual Post Post { get; set; }

	/// <summary>
	/// 提交人IP
	/// </summary>
	public string IP { get; set; }
}