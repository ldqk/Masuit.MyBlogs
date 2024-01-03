using Collections.Pooled;

namespace Masuit.MyBlogs.Core.Models.ViewModel;

/// <summary>
/// 首页视图模型
/// </summary>
public class HomePageViewModel
{
	/// <summary>
	/// 文章列表
	/// </summary>
	public PagedList<PostDto> Posts { get; set; }

	/// <summary>
	/// 网站公告列表
	/// </summary>
	public List<NoticeDto> Notices { get; set; }

	/// <summary>
	/// 分类列表
	/// </summary>
	public List<CategoryDto_P> Categories { get; set; }

	/// <summary>
	/// 标签列表
	/// </summary>
	public IDictionary<string, int> Tags { get; set; }

	/// <summary>
	/// 近期热搜
	/// </summary>
	public PooledList<KeywordsRank> HotSearch { get; set; }

	/// <summary>
	/// 热门文章
	/// </summary>
	public PooledList<PostDto> Top5Post { get; set; }

	/// <summary>
	/// 文章列表查询
	/// </summary>
	public IQueryable<PostDto> PostsQueryable { get; set; }

	/// <summary>
	/// banner文章
	/// </summary>
	public PooledList<AdvertisementDto> Banner { get; set; }

	/// <summary>
	/// 边栏广告
	/// </summary>
	public List<AdvertisementDto> SidebarAds { get; set; }

	/// <summary>
	/// 列表内广告
	/// </summary>
	public AdvertisementDto ListAdvertisement { get; set; }

	/// <summary>
	/// 分页参数
	/// </summary>
	public Pagination PageParams { get; set; }
}