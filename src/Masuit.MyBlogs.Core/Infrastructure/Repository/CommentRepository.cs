using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.Tools.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Comment AddEntity(Comment t)
        {
            DataContext.Add(t);
            return t;
        }

        /// <summary>
        /// 根据ID找实体(异步)
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public override Task<Comment> GetByIdAsync(object id)
        {
            var cid = (int)id;
            return DataContext.Comment.Include(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).FirstOrDefaultAsync(c => c.Id == cid);
        }

        /// <summary>
        /// 根据ID找实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public override Comment GetById(object id)
        {
            var cid = (int)id;
            return DataContext.Comment.Include(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).FirstOrDefault(c => c.Id == cid);
        }

        /// <summary>
        /// 高效分页查询方法
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public override Task<PagedList<Comment>> GetPagesAsync<TS>(int pageIndex, int pageSize, Expression<Func<Comment, bool>> @where, Expression<Func<Comment, TS>> @orderby, bool isAsc)
        {
            var temp = DataContext.Comment.Include(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).Where(where);
            return isAsc ? temp.OrderBy(orderby).ToPagedListAsync(pageIndex, pageSize) : temp.OrderByDescending(orderby).ToPagedListAsync(pageIndex, pageSize);
        }
    }
}