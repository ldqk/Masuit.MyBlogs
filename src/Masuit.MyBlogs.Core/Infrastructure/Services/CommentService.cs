using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class CommentService : BaseService<Comment>, ICommentService
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IQueryable<Comment> GetSelfAndAllChildrenCommentsByParentId(int id)
        {
            return SqlQuery("exec sp_getChildrenCommentByParentId " + id);
        }

        /// <summary>
        /// 根据无级子级找顶级父级评论id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetParentCommentIdByChildId(int id)
        {
            IEnumerable<int> raw = SqlQuery<int>("exec sp_getParentCommentIdByChildId " + id);
            if (raw.Any())
            {
                return raw.FirstOrDefault();
            }
            return 0;
        }

        public CommentService(IBaseRepository<Comment> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }
    }
}