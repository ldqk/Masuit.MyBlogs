using Masuit.Tools.Core.Validator;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 文章历史版本
/// </summary>
[Table("PostHistoryVersion")]
public class PostHistoryVersion : BaseEntity
{
	public PostHistoryVersion()
	{
		ModifyDate = DateTime.Now;
		Status = Status.Pending;
		Seminar = new HashSet<Seminar>();
	}

	/// <summary>
	/// 标题
	/// </summary>
	[Required, StringLength(128)]
	public string Title { get; set; }

	/// <summary>
	/// 内容
	/// </summary>
	[Required]
	public string Content { get; set; }

	/// <summary>
	/// 受保护的内容
	/// </summary>
	public string ProtectContent { get; set; }

	/// <summary>
	/// 浏览次数
	/// </summary>
	[DefaultValue(0)]
	public int ViewCount { get; set; }

	/// <summary>
	/// 修改时间
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	/// 分类id
	/// </summary>
	[ForeignKey("Category")]
	public int CategoryId { get; set; }

	/// <summary>
	/// 文章id
	/// </summary>
	[ForeignKey("Post")]
	public int PostId { get; set; }

	/// <summary>
	/// 作者邮箱
	/// </summary>
	[StringLength(255), IsEmail]
	public string Email { get; set; }

	/// <summary>
	/// 修改人名字
	/// </summary>
	public string Modifier { get; set; }

	/// <summary>
	/// 修改人邮箱
	/// </summary>
	public string ModifierEmail { get; set; }

	/// <summary>
	/// 标签
	/// </summary>
	[StringLength(255)]
	public string Label { get; set; }

	/// <summary>
	/// 分类
	/// </summary>
	public virtual Category Category { get; set; }

	/// <summary>
	/// 新文章
	/// </summary>
	public virtual Post Post { get; set; }

	/// <summary>
	/// 专题
	/// </summary>
	public virtual ICollection<Seminar> Seminar { get; set; }
}