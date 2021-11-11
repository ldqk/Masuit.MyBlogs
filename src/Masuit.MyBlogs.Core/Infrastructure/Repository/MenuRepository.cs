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

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public override IOrderedQueryable<Menu> GetQuery<TS>(Expression<Func<Menu, bool>> @where, Expression<Func<Menu, TS>> @orderby, bool isAsc = true)
        {
            return isAsc ? DataContext.Menu.Include(m => m.Children).ThenInclude(m => m.Children).ThenInclude(m => m.Children).Where(where).OrderBy(orderby) : DataContext.Menu.Include(m => m.Children).ThenInclude(m => m.Children).ThenInclude(m => m.Children).Where(where).OrderByDescending(orderby);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public override List<Menu> GetQueryFromCache(Expression<Func<Menu, bool>> @where)
        {
            return DataContext.Menu.Include(m => m.Children).ThenInclude(m => m.Children).ThenInclude(m => m.Children).Where(where).Cacheable().ToList();
        }
    }
}