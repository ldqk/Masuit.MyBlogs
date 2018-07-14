using System.Collections.Generic;
using System.Linq;
using Models.DTO;
using Models.Entity;
using Models.Enum;

namespace BLL
{
    public partial class PostBll
    {
        public bool MoveToCategory(int pid, int cid)
        {
            Post post = GetById(pid);
            post.CategoryId = cid;
            return UpdateEntitySaved(post);
        }
    }
    public partial class MenuBll
    {
        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <returns></returns>
        public IList<MenuOutputDto> GetMenus()
        {
            return BaseDal.LoadEntitiesFromCacheNoTracking<MenuOutputDto>(m => m.Status == Status.Available).ToList();
        }
    }
}