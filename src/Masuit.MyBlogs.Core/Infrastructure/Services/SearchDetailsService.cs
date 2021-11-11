using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class SearchDetailsService : BaseService<SearchDetails>, ISearchDetailsService
    {
        private readonly ISearchDetailsRepository _searchDetailsRepository;

        public SearchDetailsService(IBaseRepository<SearchDetails> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher, ISearchDetailsRepository searchDetailsRepository) : base(repository, searchEngine, searcher)
        {
            _searchDetailsRepository = searchDetailsRepository;
        }

        /// <summary>
        /// 搜索统计
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public List<SearchRank> GetRanks(DateTime start)
        {
            return _searchDetailsRepository.GetRanks(start);
        }
    }
}