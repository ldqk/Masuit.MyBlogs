using Masuit.Tools.Core.AspNetCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 友情链接
/// </summary>
[Table("Links")]
public class Links : BaseEntity
{
	public Links()
	{
		Status = Status.Available;
		Except = false;
		UpdateTime = DateTime.Now;
	}

	/// <summary>
	/// 名字
	/// </summary>
	[Required(ErrorMessage = "站点名不能为空！"), MaxLength(32, ErrorMessage = "站点名称最长限制32个字")]
	public string Name { get; set; }

	/// <summary>
	/// URL
	/// </summary>
	[Required(ErrorMessage = "站点的URL不能为空！"), MaxLength(64, ErrorMessage = "站点的URL限制64个字符")]
	public string Url { get; set; }

	/// <summary>
	/// 主页地址
	/// </summary>
	[Required(ErrorMessage = "站点的主页URL不能为空！"), MaxLength(64, ErrorMessage = "站点的主页URL限制64个字符")]
	public string UrlBase { get; set; }

	/// <summary>
	/// 是否检测白名单
	/// </summary>
	public bool Except { get; set; }

	/// <summary>
	/// 是否是推荐站点
	/// </summary>
	public bool Recommend { get; set; }

	/// <summary>
	/// 更新时间
	/// </summary>
	public DateTime UpdateTime { get; set; }

	[UpdateIgnore]
	public virtual ICollection<LinkLoopback> Loopbacks { get; set; }
}