namespace Masuit.MyBlogs.Core.Models.ViewModel;

/// <summary>
/// 文章数据模型
/// </summary>
public class PostDataModel : PostModelBase
{
	public string Status { get; set; }

	/// <summary>
	/// 作者
	/// </summary>
	public string Author { get; set; }

	/// <summary>
	/// 发表时间
	/// </summary>
	public DateTime PostDate { get; set; }

	/// <summary>
	/// 修改时间
	/// </summary>
	public DateTime ModifyDate { get; set; }

	/// <summary>
	/// 是否置顶
	/// </summary>
	public bool IsFixedTop { get; set; }

	/// <summary>
	/// 作者邮箱
	/// </summary>
	public string Email { get; set; }

	/// <summary>
	/// 标签
	/// </summary>
	public string Label { get; set; }

	/// <summary>
	/// 支持数
	/// </summary>
	public int VoteUpCount { get; set; }

	/// <summary>
	/// 反对数
	/// </summary>
	public int VoteDownCount { get; set; }

	/// <summary>
	/// 每日平均访问量
	/// </summary>
	public double AverageViewCount { get; set; }

	/// <summary>
	/// 分类id
	/// </summary>
	public int CategoryId { get; set; }

	/// <summary>
	/// 修改次数
	/// </summary>
	public int ModifyCount { get; set; }

	/// <summary>
	/// 禁止评论
	/// </summary>
	public bool DisableComment { get; set; }

	/// <summary>
	/// 禁止转载
	/// </summary>
	public bool DisableCopy { get; set; }

	/// <summary>
	/// 开启rss订阅
	/// </summary>
	public bool Rss { get; set; }

	/// <summary>
	/// 锁定编辑
	/// </summary>
	public bool Locked { get; set; }

	/// <summary>
	/// 同时浏览人数
	/// </summary>
	public int Online { get; set; }

	/// <summary>
	/// 限制描述
	/// </summary>
	public string LimitDesc { get; set; }

	/// <summary>
	/// 提交人IP地址
	/// </summary>
	public string IP { get; set; }

	/// <summary>
	/// 所属专题名
	/// </summary>
	public int[] Seminars { get; set; }

	/// <summary>
	/// 是否是不安全内容
	/// </summary>
	public bool IsNsfw { get; set; }
}