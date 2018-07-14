using System.Data.Entity.Infrastructure;
using Models.Entity;

namespace IBLL
{
    public partial interface IMenuBll
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        DbRawSqlQuery<Menu> GetChildrenMenusByParentId(int id);

    }
}