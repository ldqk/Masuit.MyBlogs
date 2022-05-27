using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class SearchDetailsRepository : BaseRepository<SearchDetails>, ISearchDetailsRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override SearchDetails AddEntity(SearchDetails t)
        {
            DataContext.Add(t);
            return t;
        }

        /// <summary>
        /// 热词统计
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public List<SearchRank> GetRanks(DateTime start)
        {
            return DataContext.SearchDetails.Where(s => s.SearchTime > start).Select(s => new { s.IP, s.Keywords }).Distinct().GroupBy(s => s.Keywords).Select(g => new SearchRank { Keywords = g.Key, Count = g.Count() }).OrderByDescending(s => s.Count).Take(30).ToList();
        }
    }
}
