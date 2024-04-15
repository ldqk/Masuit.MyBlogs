namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 系统锟斤拷锟斤拷
/// </summary>
[Table("SystemSetting")]
public class SystemSetting : BaseEntity
{
	public SystemSetting()
	{
		Status = Status.Available;
	}
	/// <summary>
	/// 锟斤拷锟斤拷锟斤拷锟斤拷
	/// </summary>
	[Required]
	public string Name { get; set; }

	/// <summary>
	/// 值
	/// </summary>
	public string Value { get; set; }

}