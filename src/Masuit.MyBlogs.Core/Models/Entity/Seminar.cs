namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 锟斤拷锟斤拷专锟斤拷
/// </summary>
[Table("Seminar")]
public partial class Seminar : BaseEntity
{
	public Seminar()
	{
		this.Post = new HashSet<Post>();
	}

	/// <summary>
	/// 专锟斤拷锟斤拷
	/// </summary>
	[Required(ErrorMessage = "专锟斤拷锟斤拷锟狡诧拷锟斤拷为锟秸ｏ拷")]
	public string Title { get; set; }

	/// <summary>
	/// 专锟斤拷锟接憋拷锟斤拷
	/// </summary>
	public string SubTitle { get; set; }

	/// <summary>
	/// 专锟斤拷锟斤拷锟斤拷
	/// </summary>
	[Required(ErrorMessage = "专锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷为锟秸ｏ拷")]
	public string Description { get; set; }

	public virtual ICollection<Post> Post { get; set; }
	public virtual ICollection<PostHistoryVersion> PostHistoryVersion { get; set; }
}