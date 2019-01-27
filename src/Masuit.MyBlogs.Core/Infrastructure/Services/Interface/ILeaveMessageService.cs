using Masuit.MyBlogs.Core.Models.Entity;
using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface
{
    public partial interface ILeaveMessageService : IBaseService<LeaveMessage>
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IEnumerable<LeaveMessage> GetSelfAndAllChildrenMessagesByParentId(int id);

        /// <summary>
        /// 根据无级子级找顶级父级留言
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int GetParentMessageIdByChildId(int id);
    }
}