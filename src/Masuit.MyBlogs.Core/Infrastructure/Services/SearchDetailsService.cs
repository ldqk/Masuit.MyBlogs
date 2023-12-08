using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed partial class SearchDetailsService(IBaseRepository<SearchDetails> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher, ISearchDetailsRepository searchDetailsRepository) : BaseService<SearchDetails>(repository, searchEngine, searcher), ISearchDetailsService
{
    /// <summary>
    /// 搜索统计
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public List<SearchRank> GetRanks(DateTime start)
    {
        return searchDetailsRepository.GetRanks(start);
    }

    /// <summary>
    /// 搜索统计(搜索结果为0的热词)
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public List<SearchRank> WishRanks(DateTime start)
    {
        return searchDetailsRepository.WishRanks(start);
    }
}
