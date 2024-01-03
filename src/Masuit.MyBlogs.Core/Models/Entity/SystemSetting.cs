namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// ϵͳ����
/// </summary>
[Table("SystemSetting")]
public class SystemSetting : BaseEntity
{
	public SystemSetting()
	{
		Status = Status.Available;
	}
	/// <summary>
	/// ��������
	/// </summary>
	[Required]
	public string Name { get; set; }

	/// <summary>
	/// ֵ
	/// </summary>
	public string Value { get; set; }

}