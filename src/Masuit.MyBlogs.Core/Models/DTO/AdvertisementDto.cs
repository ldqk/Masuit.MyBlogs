namespace Masuit.MyBlogs.Core.Models.DTO;

public class AdvertisementDto : BaseDto
{
	/// <summary>
	/// 标题
	/// </summary>
	[Required(ErrorMessage = "标题不能为空"), MinLength(10, ErrorMessage = "标题建议至少设置为10字"), MaxLength(128, ErrorMessage = "标题不能超过128字")]
	public string Title { get; set; }

	/// <summary>
	/// 宣传图片
	/// </summary>
	public string ImageUrl { get; set; }

	/// <summary>
	/// 小宣传图片
	/// </summary>
	public string ThumbImgUrl { get; set; }

	/// <summary>
	/// 描述
	/// </summary>
	[Required(ErrorMessage = "描述文字不能为空"), MinLength(40, ErrorMessage = "描述文字建议至少设置为40字"), MaxLength(300, ErrorMessage = "描述文字不能超过300字")]
	public string Description { get; set; }

	/// <summary>
	/// 广告url
	/// </summary>
	[Required(ErrorMessage = "推广链接不能为空")]
	public string Url { get; set; }

	/// <summary>
	/// 价格
	/// </summary>
	public decimal Price { get; set; }

	/// <summary>
	/// 推广区域
	/// </summary>
	[Required(ErrorMessage = "推广区域不能为空，至少需要选择一个推广区域")]
	public string Types { get; set; }

	public string CategoryIds { get; set; }

	/// <summary>
	/// 到期时间
	/// </summary>
	public DateTime? ExpireTime { get; set; }

	/// <summary>
	/// 地区模式
	/// </summary>
	public RegionLimitMode RegionMode { get; set; }

	/// <summary>
	/// 地区，逗号或竖线分隔
	/// </summary>
	public string Regions { get; set; }

	/// <summary>
	/// 广告商
	/// </summary>
	public string Merchant { get; set; }
}