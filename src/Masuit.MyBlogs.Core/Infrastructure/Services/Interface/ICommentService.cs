using Masuit.MyBlogs.Core.Models.Entity;
using System.Linq;

namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface
{
    public partial interface ICommentService : IBaseService<Comment>
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IQueryable<Comment> GetSelfAndAllChildrenCommentsByParentId(int id);

        /// <summary>
        /// 根据无级子级找顶级父级评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int GetParentCommentIdByChildId(int id);
    }
}