using Masuit.MyBlogs.Core.Models.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 网站公告
/// </summary>
[Table("Notice")]
public class Notice : BaseEntity
{
	public Notice()
	{
		PostDate = DateTime.Now;
		ModifyDate = DateTime.Now;
		Status = Status.Display;
	}

	/// <summary>
	/// 标题
	/// </summary>
	[Required(ErrorMessage = "公告标题不能为空！")]
	public string Title { get; set; }

	/// <summary>
	/// 内容
	/// </summary>
	[Required(ErrorMessage = "公告内容不能为空！"), SubmitCheck(3000, false)]
	public string Content { get; set; }

	/// <summary>
	/// 发表时间
	/// </summary>
	public DateTime PostDate { get; set; }

	/// <summary>
	/// 修改时间
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	/// 浏览人数
	/// </summary>
	public int ViewCount { get; set; }

	/// <summary>
	/// 生效时间
	/// </summary>
	public DateTime? StartTime { get; set; }

	/// <summary>
	/// 失效时间
	/// </summary>
	public DateTime? EndTime { get; set; }

	/// <summary>
	/// 公告状态
	/// </summary>
	public NoticeStatus NoticeStatus { get; set; }

	/// <summary>
	/// 是否弹窗提示
	/// </summary>
	public bool StrongAlert { get; set; }
}

public enum NoticeStatus
{
	UnStart,
	Normal,
	Expired,
}