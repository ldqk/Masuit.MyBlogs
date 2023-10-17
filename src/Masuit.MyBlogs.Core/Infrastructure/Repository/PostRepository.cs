using Collections.Pooled;
using Dispose.Scope;
using EFCoreSecondLevelCacheInterceptor;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository;

public sealed partial class PostRepository : BaseRepository<Post>, IPostRepository
{
	/// <summary>
	/// 获取第一条数据，优先从缓存读取
	/// </summary>
	/// <param name="where">查询条件</param>
	/// <returns>实体</returns>
	public override Task<Post> GetAsync(Expression<Func<Post, bool>> @where)
	{
		return EF.CompileAsyncQuery((DataContext ctx) => ctx.Post.Include(p => p.Category).Include(p => p.Seminar).FirstOrDefault(@where))(DataContext);
	}

	/// <summary>
	/// 基本查询方法，获取一个集合，优先从二级缓存读取
	/// </summary>
	/// <param name="where">查询条件</param>
	/// <returns>还未执行的SQL语句</returns>
	public override PooledList<Post> GetQueryFromCache(Expression<Func<Post, bool>> where)
	{
		return DataContext.Post.Include(p => p.Category).Where(where).Cacheable().ToPooledListScope();
	}

	/// <summary>
	/// 基本查询方法，获取一个集合
	/// </summary>
	/// <typeparam name="TS">排序</typeparam>
	/// <param name="where">查询条件</param>
	/// <param name="orderby">排序字段</param>
	/// <param name="isAsc">是否升序</param>
	/// <returns>还未执行的SQL语句</returns>
	public override IOrderedQueryable<Post> GetQuery<TS>(Expression<Func<Post, bool>> @where, Expression<Func<Post, TS>> @orderby, bool isAsc = true)
	{
		return isAsc ? DataContext.Post.Include(p => p.Category).Where(where).OrderBy(orderby) : DataContext.Post.Include(p => p.Category).Where(where).OrderByDescending(orderby);
	}
}
