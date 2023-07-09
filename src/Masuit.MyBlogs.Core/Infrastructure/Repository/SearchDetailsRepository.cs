using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository;

public sealed partial class SearchDetailsRepository : BaseRepository<SearchDetails>, ISearchDetailsRepository
{
    /// <summary>
    /// 热词统计
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public List<SearchRank> GetRanks(DateTime start)
    {
        return DataContext.SearchDetails.Where(s => s.SearchTime > start).Select(s => new { s.IP, s.Keywords }).Distinct().GroupBy(s => s.Keywords).Select(g => new SearchRank { Keywords = g.Key, Count = g.Count() }).OrderByDescending(s => s.Count).Take(30).ToList();
    }

    /// <summary>
    /// 热词统计(搜索结果为0的热词)
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public List<SearchRank> WishRanks(DateTime start)
    {
        return DataContext.SearchDetails.Where(s => s.SearchTime > start && s.ResultCount == 0).Select(s => new { s.IP, s.Keywords }).Distinct().GroupBy(s => s.Keywords).Select(g => new SearchRank { Keywords = g.Key, Count = g.Count() }).OrderByDescending(s => s.Count).Take(30).ToList();
    }
}
