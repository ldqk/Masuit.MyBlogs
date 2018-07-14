using System.Collections.Generic;
using Models.DTO;

namespace IBLL
{
    public partial interface IPostBll
    {
        /// <summary>
        /// 移动分类
        /// </summary>
        /// <param name="pid">文章ID</param>
        /// <param name="cid">分类ID</param>
        /// <returns></returns>
        bool MoveToCategory(int pid, int cid);
    }

    public partial interface IMenuBll
    {
        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <returns></returns>
        IList<MenuOutputDto> GetMenus();
    }
}