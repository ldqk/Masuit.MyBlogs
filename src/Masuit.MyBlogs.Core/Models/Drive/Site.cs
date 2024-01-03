using Newtonsoft.Json;

namespace Masuit.MyBlogs.Core.Models.Drive;

public class Site
{
	[Key]
	public int Id { get; set; }
	public string Name { get; set; }
	public string SiteId { get; set; }
	public string NickName { get; set; }
	[JsonIgnore]
	public string[] HiddenFolders { get; set; }
}