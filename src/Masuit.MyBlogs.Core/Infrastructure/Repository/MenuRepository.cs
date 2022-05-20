using EFCoreSecondLevelCacheInterceptor;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Z.EntityFramework.Plus;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class MenuRepository : BaseRepository<Menu>, IMenuRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Menu AddEntity(Menu t)
        {
            DataContext.Add(t);
            return t;
        }
    }
}
