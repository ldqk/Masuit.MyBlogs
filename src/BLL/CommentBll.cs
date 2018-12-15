using Masuit.Tools.Net;
using Models.Application;
using Models.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace BLL
{
    public partial class CommentBll
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DbRawSqlQuery<Comment> GetSelfAndAllChildrenCommentsByParentId(int id)
        {
            return WebExtension.GetDbContext<DataContext>().Database.SqlQuery<Comment>("exec sp_getChildrenCommentByParentId " + id);
        }

        /// <summary>
        /// 根据无级子级找顶级父级评论id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetParentCommentIdByChildId(int id)
        {
            DbRawSqlQuery<int> raw = WebExtension.GetDbContext<DataContext>().Database.SqlQuery<int>("exec sp_getParentCommentIdByChildId " + id);
            if (raw.Any())
            {
                return raw.FirstOrDefault();
            }
            return 0;
        }
    }
}