namespace Masuit.MyBlogs.Core.Models.Enum;

/// <summary>
/// 文章合并状态
/// </summary>
public enum MergeStatus
{
	/// <summary>
	/// 待合并
	/// </summary>
	Pending,

	/// <summary>
	/// 已合并
	/// </summary>
	Merged,

	/// <summary>
	/// 拒绝
	/// </summary>
	Reject,

	/// <summary>
	/// 阻止恶意修改
	/// </summary>
	Block
}