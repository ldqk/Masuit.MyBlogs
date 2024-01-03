namespace Masuit.MyBlogs.Core.Models.Entity;

[Table("AdvertisementClickRecord")]
public class AdvertisementClickRecord : BaseEntity
{
	public int AdvertisementId { get; set; }

	public string IP { get; set; }

	public string Location { get; set; }

	public string Referer { get; set; }

	public DateTime Time { get; set; }
}