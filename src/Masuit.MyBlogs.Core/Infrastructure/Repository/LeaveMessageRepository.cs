using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

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
    }
}
