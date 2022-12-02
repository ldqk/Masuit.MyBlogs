using Newtonsoft.Json;

namespace Masuit.MyBlogs.Core.Models.ViewModel;

public class FileList
{
	[JsonProperty("name")]
	public string name { get; set; }
	[JsonProperty("rights")]
	public string rights { get; set; }
	[JsonProperty("size")]
	public long size { get; set; }
	[JsonProperty("date")]
	public string date { get; set; }
	[JsonProperty("type")]
	public string type { get; set; }
}