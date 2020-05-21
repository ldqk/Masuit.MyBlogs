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
        public CommentService(IBaseRepository<Comment> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }

        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Comment> GetSelfAndAllChildrenCommentsByParentId(int id)
        {
            var c = GetById(id);
            if (c != null)
            {
                var comments = new List<Comment>() { c };
                GetSelfAndAllChildrenCommentsByParentId(c, comments);
                return comments;
            }

            return new List<Comment>();
        }

        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private void GetSelfAndAllChildrenCommentsByParentId(Comment comment, List<Comment> list)
        {
            var comments = GetQuery(x => x.ParentId == comment.Id).ToList();
            if (comments.Any())
            {
                list.AddRange(comments);
                foreach (var c in comments)
                {
                    GetSelfAndAllChildrenCommentsByParentId(c, list);
                }
            }
        }

        /// <summary>
        /// 根据无级子级找顶级父级评论id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetParentCommentIdByChildId(int id)
        {
            Comment comment = GetById(id);
            if (comment != null)
            {
                return GetParentCommentIdByChildId(comment);
            }
            return 0;
        }

        /// <summary>
        /// 根据无级子级找顶级父级评论id
        /// </summary>
        /// <param name="com"></param>
        /// <returns></returns>
        private int GetParentCommentIdByChildId(Comment com)
        {
            Comment comment = GetNoTracking(c => c.Id == com.ParentId);
            if (comment != null)
            {
                return GetParentCommentIdByChildId(comment);
            }
            return com.Id;
        }
    }
}