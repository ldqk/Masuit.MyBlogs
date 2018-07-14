using System.Data.Entity.Infrastructure;
using System.Linq;
using Masuit.Tools.Net;
using Models.Application;
using Models.Entity;

namespace BLL
{
    public partial class LeaveMessageBll
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DbRawSqlQuery<LeaveMessage> GetSelfAndAllChildrenMessagesByParentId(int id) => WebExtension.GetDbContext<DataContext>().Database.SqlQuery<LeaveMessage>("exec sp_getChildrenLeaveMsgByParentId " + id);

        /// <summary>
        /// 根据无级子级找顶级父级留言id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetParentMessageIdByChildId(int id)
        {
            var raw = WebExtension.GetDbContext<DataContext>().Database.SqlQuery<int>("exec sp_getParentMessageIdByChildId " + id);
            if (raw.Any())
            {
                return raw.FirstOrDefault();
            }
            return 0;
        }
    }
}