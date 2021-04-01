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
    public partial class LeaveMessageRepository : BaseRepository<LeaveMessage>, ILeaveMessageRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override LeaveMessage AddEntity(LeaveMessage t)
        {
            DataContext.Add(t);
            return t;
        }

        /// <summary>
        /// 根据ID找实体(异步)
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public override Task<LeaveMessage> GetByIdAsync(object id)
        {
            var cid = (int)id;
            return DataContext.LeaveMessage.Include(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).FirstOrDefaultAsync(c => c.Id == cid);
        }

        /// <summary>
        /// 根据ID找实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public override LeaveMessage GetById(object id)
        {
            var cid = (int)id;
            return DataContext.LeaveMessage.Include(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).FirstOrDefault(c => c.Id == cid);
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
        public override Task<PagedList<LeaveMessage>> GetPagesAsync<TS>(int pageIndex, int pageSize, Expression<Func<LeaveMessage, bool>> @where, Expression<Func<LeaveMessage, TS>> @orderby, bool isAsc)
        {
            var temp = DataContext.LeaveMessage.Include(c => c.Children).ThenInclude(c => c.Children).ThenInclude(c => c.Children).Where(where);
            return isAsc ? temp.OrderBy(orderby).ToPagedListAsync(pageIndex, pageSize) : temp.OrderByDescending(orderby).ToPagedListAsync(pageIndex, pageSize);
        }
    }
}