namespace Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

public partial interface ISearchDetailsRepository : IBaseRepository<SearchDetails>
{
    /// <summary>
    /// 搜索统计
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    List<SearchRank> GetRanks(DateTime start);

    /// <summary>
    /// 热词统计(搜索结果为0的热词)
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public List<SearchRank> WishRanks(DateTime start);
}
