namespace Masuit.MyBlogs.Core.Models.Entity;

[Table(nameof(FastShare))]
public class FastShare : BaseEntity
{
	public string Title { get; set; }
	public string Link { get; set; }
	public int Sort { get; set; }
}