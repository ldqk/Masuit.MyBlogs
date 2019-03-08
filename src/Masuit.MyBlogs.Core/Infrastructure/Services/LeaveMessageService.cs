using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class LeaveMessageService : BaseService<LeaveMessage>, ILeaveMessageService
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<LeaveMessage> GetSelfAndAllChildrenMessagesByParentId(int id) => SqlQuery<LeaveMessage>("exec sp_getChildrenLeaveMsgByParentId " + id);

        /// <summary>
        /// 根据无级子级找顶级父级留言id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetParentMessageIdByChildId(int id)
        {
            var raw = SqlQuery<int>("exec sp_getParentMessageIdByChildId " + id);
            if (raw.Any())
            {
                return raw.FirstOrDefault();
            }
            return 0;
        }

        public LeaveMessageService(IBaseRepository<LeaveMessage> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }
    }
}