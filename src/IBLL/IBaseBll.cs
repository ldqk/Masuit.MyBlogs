
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Threading.Tasks;
using EFSecondLevelCache;
using Models.Entity;
namespace IBLL
{
    public interface IBaseBll<T>
    {
        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> GetAll();
        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> GetAllNoTracking();

        /// <summary>
        /// 从一级缓存获取所有实体
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> GetAllFromCache(int timespan = 20);

        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> GetAllFromCacheNoTracking(int timespan = 20);
        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<T>> GetAllFromCacheNoTrackingAsync(int timespan = 20);

        /// <summary>
        /// 从一级缓存获取所有实体（异步）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<T>> GetAllFromCacheAsync(int timespan = 20);

        /// <summary>
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        EFCachedQueryable<T> GetAllFromL2Cache();

        /// <summary>
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        EFCachedQueryable<T> GetAllFromL2CacheNoTracking();

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> GetAll<TDto>();

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> GetAllNoTracking<TDto>();

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromCache<TDto>(int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<TDto>> GetAllFromCacheAsync<TDto>(int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromCacheNoTracking<TDto>(int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<TDto>> GetAllFromCacheNoTrackingAsync<TDto>(int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromL2Cache<TDto>();

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromL2CacheNoTracking<TDto>();

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> GetAll<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> GetAllNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> GetAllFromCache<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<T>> GetAllFromCacheAsync<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> GetAllFromCacheNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<T>> GetAllFromCacheNoTrackingAsync<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> GetAllFromL2Cache<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> GetAllFromL2CacheNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> GetAll<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> GetAllNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromCache<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<TDto>> GetAllFromCacheAsync<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromCacheNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<TDto>> GetAllFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromL2Cache<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class;

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> GetAllFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> LoadEntities(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> LoadEntities<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> LoadEntities<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        /// <returns></returns>
        IQueryable<TDto> LoadEntities<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadEntitiesFromCache(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns></returns>
        IEnumerable<T> LoadEntitiesFromCache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadEntitiesFromCache<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadEntitiesFromCache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadEntitiesFromL2Cache(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadEntitiesFromL2Cache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadEntitiesFromL2Cache<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadEntitiesFromL2Cache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IQueryable<T>> LoadEntitiesAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IOrderedQueryable<T>> LoadEntitiesAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IQueryable<TDto>> LoadEntitiesAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IQueryable<TDto>> LoadEntitiesAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<T>> LoadEntitiesFromCacheAsync(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<T>> LoadEntitiesFromCacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<TDto>> LoadEntitiesFromCacheAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<TDto>> LoadEntitiesFromCacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> LoadEntitiesNoTracking(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IOrderedQueryable<T> LoadEntitiesNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> LoadEntitiesNoTracking<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> LoadEntitiesNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        IEnumerable<T> LoadEntitiesFromCacheNoTracking(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadEntitiesFromCacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        IEnumerable<TDto> LoadEntitiesFromCacheNoTracking<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadEntitiesFromCacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        IEnumerable<T> LoadEntitiesFromL2CacheNoTracking(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadEntitiesFromL2CacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        IEnumerable<TDto> LoadEntitiesFromL2CacheNoTracking<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadEntitiesFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个集合（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        Task<IQueryable<T>> LoadEntitiesNoTrackingAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个集合（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IQueryable<T>> LoadEntitiesNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        Task<IQueryable<TDto>> LoadEntitiesNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IQueryable<TDto>> LoadEntitiesNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        ///  基本查询方法，获取一个集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        Task<IEnumerable<T>> LoadEntitiesFromCacheNoTrackingAsync(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<T>> LoadEntitiesFromCacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        ///  基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        Task<IEnumerable<TDto>> LoadEntitiesFromCacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<IEnumerable<TDto>> LoadEntitiesFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        ///  基本查询方法，获取一个集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheNoTrackingAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        ///  基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        T GetFirstEntity(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        T GetFirstEntity<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        TDto GetFirstEntity<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        TDto GetFirstEntity<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        T GetFirstEntityFromCache(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>映射实体</returns>
        T GetFirstEntityFromCache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        TDto GetFirstEntityFromCache<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>映射实体</returns>
        TDto GetFirstEntityFromCache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        T GetFirstEntityFromL2Cache(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        T GetFirstEntityFromL2Cache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        TDto GetFirstEntityFromL2Cache<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        TDto GetFirstEntityFromL2Cache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        Task<TDto> GetFirstEntityAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        Task<TDto> GetFirstEntityAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityFromCacheAsync(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityFromCacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        Task<TDto> GetFirstEntityFromCacheAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取(异步)
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        Task<TDto> GetFirstEntityFromCacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityFromL2CacheAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        ///  获取第一条数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityFromL2CacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        Task<TDto> GetFirstEntityFromL2CacheAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        Task<TDto> GetFirstEntityFromL2CacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        T GetFirstEntityNoTracking(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条数据（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        T GetFirstEntityNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        TDto GetFirstEntityNoTracking<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（不跟踪实体）
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        TDto GetFirstEntityNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        T GetFirstEntityFromCacheNoTracking(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        T GetFirstEntityFromCacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        TDto GetFirstEntityFromCacheNoTracking<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>映射实体</returns>
        TDto GetFirstEntityFromCacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        T GetFirstEntityFromL2CacheNoTracking(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        T GetFirstEntityFromL2CacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);


        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        TDto GetFirstEntityFromL2CacheNoTracking<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        TDto GetFirstEntityFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityNoTrackingAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条数据（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        Task<TDto> GetFirstEntityNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        Task<TDto> GetFirstEntityNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityFromCacheNoTrackingAsync(Expression<Func<T, bool>> @where, int timespan = 30);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>映射实体</returns>
        Task<T> GetFirstEntityFromCacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        Task<TDto> GetFirstEntityFromCacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>映射实体</returns>
        Task<TDto> GetFirstEntityFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityFromL2CacheNoTrackingAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        Task<T> GetFirstEntityFromL2CacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>映射实体</returns>
        Task<TDto> GetFirstEntityFromL2CacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        Task<TDto> GetFirstEntityFromL2CacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 根据ID找实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        T GetById(object id);

        /// <summary>
        /// 根据ID找实体(异步)
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        Task<T> GetByIdAsync(object id);

        /// <summary>
        /// 高效分页查询方法
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> LoadPageEntities<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc = true);


        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> LoadPageEntities<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc = true);

        /// <summary>
        /// 高效分页查询方法，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadPageEntitiesFromCache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc = true, int timespan = 30);


        /// <summary>
        /// 高效分页查询方法，优先从缓存读取，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadPageEntitiesFromCache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadPageEntitiesFromL2Cache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadPageEntitiesFromL2Cache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 高效分页查询方法（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<T> LoadPageEntitiesNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IQueryable<TDto> LoadPageEntitiesNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 高效分页查询方法，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadPageEntitiesFromCacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30);

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadPageEntitiesFromCacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class;

        /// <summary>
        /// 高效分页查询方法，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<T> LoadPageEntitiesFromL2CacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        IEnumerable<TDto> LoadPageEntitiesFromL2CacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true);

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        bool DeleteById(object id);

        /// <summary>
        /// 根据ID删除实体并保存
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        bool DeleteByIdSaved(object id);

        /// <summary>
        /// 根据ID删除实体并保存（异步）
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        Task<int> DeleteByIdSavedAsync(object id);

        /// <summary>
        /// 删除实体并保存
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        bool DeleteEntity(T t);

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        int DeleteEntity(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        int DeleteEntitySaved(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        Task<int> DeleteEntitySavedAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 根据条件删除实体（异步）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        Task<int> DeleteEntityAsync(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 删除实体并保存
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        bool DeleteEntitySaved(T t);

        /// <summary>
        /// 删除实体并保存（异步）
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        Task<int> DeleteEntitySavedAsync(T t);

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        bool UpdateEntity(T t);

        /// <summary>
        /// 更新实体并保存
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        bool UpdateEntitySaved(T t);

        /// <summary>
        /// 更新实体并保存（异步）
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        Task<int> UpdateEntitySavedAsync(T t);

        /// <summary>
        /// 根据条件更新实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        int UpdateEntity(Expression<Func<T, bool>> @where, T t);

        /// <summary>
        /// 根据条件更新实体（异步）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        Task<int> UpdateEntityAsync(Expression<Func<T, bool>> @where, T t);

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        T AddEntity(T t);

        /// <summary>
        /// 添加实体并保存
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        T AddEntitySaved(T t);

        /// <summary>
        /// 添加实体并保存（异步）
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        Task<int> AddEntitySavedAsync(T t);

        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="exp">更新条件</param>
        /// <param name="entities">实体集合</param>
        void AddOrUpdate(Expression<Func<T, object>> exp, params T[] entities);

        /// <summary>
        /// 添加或更新并保存
        /// </summary>
        /// <param name="exp">更新条件</param>
        /// <param name="entities">实体集合</param>
        int AddOrUpdateSaved(Expression<Func<T, object>> exp, params T[] entities);

        /// <summary>
        /// 添加或更新并保存（异步）
        /// </summary>
        /// <param name="exp">更新条件</param>
        /// <param name="entities">实体集合</param>
        Task<int> AddOrUpdateSavedAsync(Expression<Func<T, object>> exp, params T[] entities);

        /// <summary>
        /// 统一保存的方法
        /// </summary>
        /// <returns>受影响的行数</returns>
        int SaveChanges();

        /// <summary>
        /// 统一保存数据
        /// </summary>
        /// <returns>受影响的行数</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 判断实体是否在数据库中存在
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>是否存在</returns>
        bool Any(Expression<Func<T, bool>> @where);

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        bool DeleteEntities(IEnumerable<T> list);

        /// <summary>
        /// 删除多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        bool DeleteEntitiesSaved(IEnumerable<T> list);

        /// <summary>
        /// 删除多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        Task<int> DeleteEntitiesSavedAsync(IEnumerable<T> list);

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        bool UpdateEntities(IEnumerable<T> list);

        /// <summary>
        /// 更新多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        bool UpdateEntitiesSaved(IEnumerable<T> list);

        /// <summary>
        /// 更新多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        Task<int> UpdateEntitiesSavedAsync(IEnumerable<T> list);

        /// <summary>
        /// 添加多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>添加成功</returns>
        IEnumerable<T> AddEntities(IList<T> list);

        /// <summary>
        /// 添加多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>添加成功</returns>
        Task<IEnumerable<T>> AddEntitiesAsync(IList<T> list);

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">参数</param>
        /// <returns>泛型集合</returns>
        DbRawSqlQuery<TS> SqlQuery<TS>(string sql, params SqlParameter[] parameters);

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">参数</param>
        DbRawSqlQuery SqlQuery(Type t, string sql, params SqlParameter[] parameters);

        /// <summary>
        /// 执行DML语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        void ExecuteSql(string sql, params SqlParameter[] parameters);
        
        /// <summary>
        /// 批量添加实体
        /// </summary>
        /// <param name="list">需要添加的实体</param>
        void BulkInsert(IEnumerable<T> list);

        /// <summary>
        /// 统一批量保存数据
        /// </summary>
        void BulkSaveChanges();
    }
	
	public partial interface IBroadcastBll : IBaseBll<Broadcast>{}   
	
	public partial interface ICategoryBll : IBaseBll<Category>{}   
	
	public partial interface ICommentBll : IBaseBll<Comment>{}   
	
	public partial interface IContactsBll : IBaseBll<Contacts>{}   
	
	public partial interface IDonateBll : IBaseBll<Donate>{}   
	
	public partial interface IInternalMessageBll : IBaseBll<InternalMessage>{}   
	
	public partial interface IInterviewBll : IBaseBll<Interview>{}   
	
	public partial interface IInterviewDetailBll : IBaseBll<InterviewDetail>{}   
	
	public partial interface IIssueBll : IBaseBll<Issue>{}   
	
	public partial interface ILeaveMessageBll : IBaseBll<LeaveMessage>{}   
	
	public partial interface ILinksBll : IBaseBll<Links>{}   
	
	public partial interface ILoginRecordBll : IBaseBll<LoginRecord>{}   
	
	public partial interface IMenuBll : IBaseBll<Menu>{}   
	
	public partial interface IMiscBll : IBaseBll<Misc>{}   
	
	public partial interface INoticeBll : IBaseBll<Notice>{}   
	
	public partial interface IPostBll : IBaseBll<Post>{}   
	
	public partial interface IPostAccessRecordBll : IBaseBll<PostAccessRecord>{}   
	
	public partial interface IPostHistoryVersionBll : IBaseBll<PostHistoryVersion>{}   
	
	public partial interface ISearchDetailsBll : IBaseBll<SearchDetails>{}   
	
	public partial interface ISeminarBll : IBaseBll<Seminar>{}   
	
	public partial interface ISystemSettingBll : IBaseBll<SystemSetting>{}   
	
	public partial interface IUserInfoBll : IBaseBll<UserInfo>{}   
	
}