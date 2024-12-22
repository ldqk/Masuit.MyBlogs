using EFCoreSecondLevelCacheInterceptor;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository;

public static class QueryableExt
{
    /// <summary>
    /// 从二级缓存生成分页集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <param name="page">当前页</param>
    /// <param name="size">页大小</param>
    /// <returns></returns>
    public static PagedList<T> ToCachedPagedList<T>(this IQueryable<T> query, int page, int size)
    {
        page = Math.Max(1, page);
        var totalCount = query.Count();
        if (1L * page * size > totalCount)
        {
            page = (int)Math.Ceiling(totalCount / (size * 1.0));
        }

        if (page <= 0)
        {
            page = 1;
        }

        var list = query.Skip(size * (page - 1)).Take(size).Cacheable().ToList();
        return new PagedList<T>(list, page, size, totalCount);
    }
}