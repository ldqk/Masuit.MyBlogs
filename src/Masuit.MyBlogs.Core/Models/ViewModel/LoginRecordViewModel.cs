namespace Masuit.MyBlogs.Core.Models.ViewModel;

/// <summary>
/// 用户登录记录输出模型
/// </summary>
public class LoginRecordViewModel : BaseDto
{
	/// <summary>
	/// 登录点IP
	/// </summary>
	public string IP { get; set; }

	/// <summary>
	/// 登录时间
	/// </summary>
	public DateTime LoginTime { get; set; }

	/// <summary>
	/// 所在省份
	/// </summary>
	public string Province { get; set; }

	/// <summary>
	/// 详细地理位置
	/// </summary>
	public string PhysicAddress { get; set; }

	/// <summary>
	/// 登录类型
	/// </summary>
	public LoginType LoginType { get; set; }

	/// <summary>
	/// 登陆者ID
	/// </summary>
	public int UserInfoId { get; set; }
}