namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// ����ר��
/// </summary>
[Table("Seminar")]
public partial class Seminar : BaseEntity
{
	public Seminar()
	{
		this.Post = new HashSet<Post>();
	}

	/// <summary>
	/// ר����
	/// </summary>
	[Required(ErrorMessage = "ר�����Ʋ���Ϊ�գ�")]
	public string Title { get; set; }

	/// <summary>
	/// ר���ӱ���
	/// </summary>
	public string SubTitle { get; set; }

	/// <summary>
	/// ר������
	/// </summary>
	[Required(ErrorMessage = "ר����������Ϊ�գ�")]
	public string Description { get; set; }

	public virtual ICollection<Post> Post { get; set; }
	public virtual ICollection<PostHistoryVersion> PostHistoryVersion { get; set; }
}