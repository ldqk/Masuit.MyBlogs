namespace Masuit.MyBlogs.Core.Models.Drive;

public class Setting
{
	[Key]
	public int id { get; set; }
	[Required]
	public string Key { get; set; }
	[Required]
	public string Value { get; set; }
}