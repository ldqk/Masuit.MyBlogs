using System.Data.Entity.Infrastructure;
using Masuit.Tools.Net;
using Models.Application;
using Models.Entity;

namespace BLL
{
    public partial class MenuBll
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DbRawSqlQuery<Menu> GetChildrenMenusByParentId(int id)
        {
            return WebExtension.GetDbContext<DataContext>().Database.SqlQuery<Menu>("exec sp_getChildrenMenusByParentId " + id);
        }
    }
}