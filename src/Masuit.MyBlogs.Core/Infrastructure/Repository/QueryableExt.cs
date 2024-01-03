using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFCoreSecondLevelCacheInterceptor;
using Masuit.LuceneEFCore.SearchEngine;

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
	public static PagedList<T> ToCachedPagedList<T>(this IOrderedQueryable<T> query, int page, int size) where T : LuceneIndexableBaseEntity
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

	/// <summary>
	/// 生成分页集合
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TDto"></typeparam>
	/// <param name="query"></param>
	/// <param name="page">当前页</param>
	/// <param name="size">页大小</param>
	/// <param name="mapper"></param>
	/// <returns></returns>
	public static PagedList<TDto> ToPagedList<T, TDto>(this IOrderedQueryable<T> query, int page, int size, MapperConfiguration mapper)
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

		var list = query.Skip(size * (page - 1)).Take(size).ProjectTo<TDto>(mapper).ToList();
		return new PagedList<TDto>(list, page, size, totalCount);
	}

	/// <summary>
	/// 生成分页集合
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TDto"></typeparam>
	/// <param name="query"></param>
	/// <param name="page">当前页</param>
	/// <param name="size">页大小</param>
	/// <param name="mapper"></param>
	/// <returns></returns>
	public static async Task<PagedList<TDto>> ToPagedListAsync<T, TDto>(this IOrderedQueryable<T> query, int page, int size, MapperConfiguration mapper)
	{
		page = Math.Max(1, page);
		var totalCount = await query.CountAsync();
		if (1L * page * size > totalCount)
		{
			page = (int)Math.Ceiling(totalCount / (size * 1.0));
		}

		if (page <= 0)
		{
			page = 1;
		}

		var list = await query.Skip(size * (page - 1)).Take(size).ProjectTo<TDto>(mapper).ToListAsync();
		return new PagedList<TDto>(list, page, size, totalCount);
	}

	/// <summary>
	/// 从二级缓存生成分页集合
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TDto"></typeparam>
	/// <param name="query"></param>
	/// <param name="page">当前页</param>
	/// <param name="size">页大小</param>
	/// <param name="mapper"></param>
	/// <returns></returns>
	public static PagedList<TDto> ToCachedPagedList<T, TDto>(this IOrderedQueryable<T> query, int page, int size, MapperConfiguration mapper) where TDto : class where T : LuceneIndexableBaseEntity
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

		var list = query.Skip(size * (page - 1)).Take(size).ProjectTo<TDto>(mapper).Cacheable().ToList();
		return new PagedList<TDto>(list, page, size, totalCount);
	}
}
