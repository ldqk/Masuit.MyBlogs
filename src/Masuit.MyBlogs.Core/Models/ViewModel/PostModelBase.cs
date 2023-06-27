namespace Masuit.MyBlogs.Core.Models.ViewModel;

public class PostModelBase
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 浏览次数
    /// </summary>
    public long ViewCount { get; set; }
}
