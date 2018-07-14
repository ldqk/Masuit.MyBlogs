using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using EFSecondLevelCache;
using IBLL;
using IDAL;

namespace BLL
{
    /// <summary>
    /// 业务层基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseBll<T> : IBaseBll<T> where T : class, new()
    {
        public virtual IBaseDal<T> BaseDal { get; set; }// = Factory.CreateInstance<IBaseDal<T>>("DAL." + typeof(T).Name + "Dal");

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetAll()
        {
            return BaseDal.GetAll();
        }

        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetAllNoTracking()
        {
            return BaseDal.GetAllNoTracking();
        }

        /// <summary>
        /// 从一级缓存获取所有实体
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCache(int timespan = 20)
        {
            return BaseDal.GetAllFromCache(timespan);
        }

        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCacheNoTracking(int timespan = 20)
        {
            return BaseDal.GetAllFromCacheNoTracking(timespan);
        }

        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheNoTrackingAsync(int timespan = 20)
        {
            return await BaseDal.GetAllFromCacheNoTrackingAsync(timespan);
        }

        /// <summary>
        /// 从一级缓存获取所有实体（异步）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheAsync(int timespan = 20)
        {
            return await BaseDal.GetAllFromCacheAsync(timespan);
        }

        /// <summary>
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual EFCachedQueryable<T> GetAllFromL2Cache()
        {
            return BaseDal.GetAllFromL2Cache();
        }

        /// <summary>
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual EFCachedQueryable<T> GetAllFromL2CacheNoTracking()
        {
            return BaseDal.GetAllFromL2CacheNoTracking();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAll<TDto>()
        {
            return BaseDal.GetAll<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAllNoTracking<TDto>()
        {
            return BaseDal.GetAllNoTracking<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCache<TDto>(int timespan = 20) where TDto : class
        {
            return BaseDal.GetAllFromCache<TDto>(timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheAsync<TDto>(int timespan = 20) where TDto : class
        {
            return await BaseDal.GetAllFromCacheAsync<TDto>(timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCacheNoTracking<TDto>(int timespan = 20) where TDto : class
        {
            return BaseDal.GetAllFromCacheNoTracking<TDto>(timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheNoTrackingAsync<TDto>(int timespan = 20) where TDto : class
        {
            return await BaseDal.GetAllFromCacheNoTrackingAsync<TDto>(timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2Cache<TDto>()
        {
            return BaseDal.GetAllFromL2Cache<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2CacheNoTracking<TDto>()
        {
            return BaseDal.GetAllFromL2CacheNoTracking<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetAll<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAll(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetAllNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAllNoTracking(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCache<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20)
        {
            return BaseDal.GetAllFromCache(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheAsync<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20)
        {
            return await BaseDal.GetAllFromCacheAsync(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCacheNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20)
        {
            return BaseDal.GetAllFromCacheNoTracking(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheNoTrackingAsync<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20)
        {
            return await BaseDal.GetAllFromCacheNoTrackingAsync(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromL2Cache<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAllFromL2Cache(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromL2CacheNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAllFromL2CacheNoTracking(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAll<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAll<TS, TDto>(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAllNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAllNoTracking<TS, TDto>(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCache<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class
        {
            return BaseDal.GetAllFromCache<TS, TDto>(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheAsync<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class
        {
            return await BaseDal.GetAllFromCacheAsync<TS, TDto>(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCacheNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class
        {
            return BaseDal.GetAllFromCacheNoTracking<TS, TDto>(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20) where TDto : class
        {
            return await BaseDal.GetAllFromCacheNoTrackingAsync<TS, TDto>(orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2Cache<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetAllFromL2Cache<TS, TDto>(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetAllFromL2CacheNoTracking<TS, TDto>(orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> LoadEntities(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntities(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> LoadEntities<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntities(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> LoadEntities<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntities<TDto>(where);
        }

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
        public virtual IQueryable<TDto> LoadEntities<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntities<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromCache(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return BaseDal.LoadEntitiesFromCache(where, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns></returns>
        public virtual IEnumerable<T> LoadEntitiesFromCache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return BaseDal.LoadEntitiesFromCache(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromCache<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return BaseDal.LoadEntitiesFromCache<TDto>(where);
        }

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
        public virtual IEnumerable<TDto> LoadEntitiesFromCache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return BaseDal.LoadEntitiesFromCache<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2Cache(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntitiesFromL2Cache(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2Cache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntitiesFromL2Cache(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromL2Cache<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntitiesFromL2Cache<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromL2Cache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntitiesFromL2Cache<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<T>> LoadEntitiesAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesAsync(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IOrderedQueryable<T>> LoadEntitiesAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<TDto>> LoadEntitiesAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesAsync<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<TDto>> LoadEntitiesAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return await BaseDal.LoadEntitiesFromCacheAsync(where, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return await BaseDal.LoadEntitiesFromCacheAsync(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return await BaseDal.LoadEntitiesFromCacheAsync<TDto>(where, timespan);
        }

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
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return await BaseDal.LoadEntitiesFromCacheAsync<TS, TDto>(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesFromL2CacheAsync(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesFromL2CacheAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesFromL2CacheAsync<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesFromL2CacheAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> LoadEntitiesNoTracking(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntitiesNoTracking(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> LoadEntitiesNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntitiesNoTracking(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> LoadEntitiesNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntitiesNoTracking<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> LoadEntitiesNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntitiesNoTracking<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<T> LoadEntitiesFromCacheNoTracking(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return BaseDal.LoadEntitiesFromCacheNoTracking(where, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromCacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return BaseDal.LoadEntitiesFromCacheNoTracking(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromCacheNoTracking<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return BaseDal.LoadEntitiesFromCacheNoTracking<TDto>(where, timespan);
        }

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
        public virtual IEnumerable<TDto> LoadEntitiesFromCacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return BaseDal.LoadEntitiesFromCacheNoTracking<TS, TDto>(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2CacheNoTracking(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntitiesFromL2CacheNoTracking(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2CacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntitiesFromL2CacheNoTracking(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromL2CacheNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.LoadEntitiesFromL2CacheNoTracking<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadEntitiesFromL2CacheNoTracking<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IQueryable<T>> LoadEntitiesNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesNoTrackingAsync(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<T>> LoadEntitiesNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesNoTrackingAsync(@where, @orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IQueryable<TDto>> LoadEntitiesNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesNoTrackingAsync<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<TDto>> LoadEntitiesNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesNoTrackingAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        ///  基本查询方法，获取一个集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheNoTrackingAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return await BaseDal.LoadEntitiesFromCacheNoTrackingAsync(where, timespan);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return await BaseDal.LoadEntitiesFromCacheNoTrackingAsync(where, orderby, isAsc, timespan);
        }

        /// <summary>
        ///  基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return await BaseDal.LoadEntitiesFromCacheNoTrackingAsync<TDto>(where, timespan);
        }

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
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return await BaseDal.LoadEntitiesFromCacheNoTrackingAsync<TS, TDto>(where, orderby, isAsc, timespan);
        }

        /// <summary>
        ///  基本查询方法，获取一个集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesFromL2CacheNoTrackingAsync(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesFromL2CacheNoTrackingAsync(where, orderby, isAsc);
        }

        /// <summary>
        ///  基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.LoadEntitiesFromL2CacheNoTrackingAsync<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">输出类型</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.LoadEntitiesFromL2CacheNoTrackingAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntity(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntity(where);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntity<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntity(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntity<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntity<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        public virtual TDto GetFirstEntity<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntity<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromCache(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return BaseDal.GetFirstEntityFromCache(where, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>映射实体</returns>
        public virtual T GetFirstEntityFromCache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return BaseDal.GetFirstEntityFromCache(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromCache<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return BaseDal.GetFirstEntityFromCache<TDto>(where, timespan);
        }

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
        public virtual TDto GetFirstEntityFromCache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return BaseDal.GetFirstEntityFromCache<TS, TDto>(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromL2Cache(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntityFromL2Cache(where);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        public virtual T GetFirstEntityFromL2Cache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntityFromL2Cache(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromL2Cache<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntityFromL2Cache<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        public virtual TDto GetFirstEntityFromL2Cache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntityFromL2Cache<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityAsync<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        public virtual async Task<TDto> GetFirstEntityAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromCacheAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return await BaseDal.GetFirstEntityFromCacheAsync(where, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromCacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return await BaseDal.GetFirstEntityFromCacheAsync(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromCacheAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return await BaseDal.GetFirstEntityFromCacheAsync<TDto>(where, timespan);
        }

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
        public virtual async Task<TDto> GetFirstEntityFromCacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return await BaseDal.GetFirstEntityFromCacheAsync<TS, TDto>(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromL2CacheAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityFromL2CacheAsync(where);
        }

        /// <summary>
        ///  获取第一条数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromL2CacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityFromL2CacheAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromL2CacheAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityFromL2CacheAsync<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromL2CacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityFromL2CacheAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityAsync(where);
        }

        /// <summary>
        /// 获取第一条数据（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityNoTracking(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntityNoTracking(where);
        }

        /// <summary>
        /// 获取第一条数据（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntityNoTracking(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntityNoTracking<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（不跟踪实体）
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntityNoTracking<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromCacheNoTracking(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return BaseDal.GetFirstEntityFromCacheNoTracking(where, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromCacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return BaseDal.GetFirstEntityFromCacheNoTracking(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromCacheNoTracking<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return BaseDal.GetFirstEntityFromCacheNoTracking<TDto>(where, timespan);
        }

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
        public virtual TDto GetFirstEntityFromCacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return BaseDal.GetFirstEntityFromCacheNoTracking<TS, TDto>(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromL2CacheNoTracking(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntityFromL2CacheNoTracking(where);
        }

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromL2CacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntityFromL2CacheNoTracking(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromL2CacheNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFirstEntityFromL2CacheNoTracking<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        public virtual TDto GetFirstEntityFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFirstEntityFromL2CacheNoTracking<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityNoTrackingAsync(where);
        }

        /// <summary>
        /// 获取第一条数据（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityNoTrackingAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityNoTrackingAsync<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        public virtual async Task<TDto> GetFirstEntityNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityNoTrackingAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromCacheNoTrackingAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return await BaseDal.GetFirstEntityFromCacheNoTrackingAsync(where, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>映射实体</returns>
        public virtual async Task<T> GetFirstEntityFromCacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return await BaseDal.GetFirstEntityFromCacheNoTrackingAsync(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromCacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return await BaseDal.GetFirstEntityFromCacheNoTrackingAsync<TDto>(where, timespan);
        }

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
        public virtual async Task<TDto> GetFirstEntityFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return await BaseDal.GetFirstEntityFromCacheNoTrackingAsync<TS, TDto>(where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromL2CacheNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityFromL2CacheNoTrackingAsync(where);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromL2CacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityFromL2CacheNoTrackingAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromL2CacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.GetFirstEntityFromL2CacheNoTrackingAsync<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>映射实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromL2CacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return await BaseDal.GetFirstEntityFromL2CacheNoTrackingAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 根据ID找实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public virtual T GetById(object id)
        {
            return BaseDal.GetById(id);
        }

        /// <summary>
        /// 根据ID找实体(异步)
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await BaseDal.GetByIdAsync(id);
        }

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
        public virtual IQueryable<T> LoadPageEntities<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby,
            bool isAsc = true)
        {
            return BaseDal.LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> LoadPageEntities<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc)
        {
            return BaseDal.LoadPageEntities<TS, TDto>(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

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
        public virtual IEnumerable<T> LoadPageEntitiesFromCache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return BaseDal.LoadPageEntitiesFromCache(pageIndex, pageSize, out totalCount, where, orderby, isAsc, timespan);
        }

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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromCache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc, int timespan = 30) where TDto : class
        {
            return BaseDal.LoadPageEntitiesFromCache<TS, TDto>(pageIndex, pageSize, out totalCount, where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadPageEntitiesFromL2Cache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadPageEntitiesFromL2Cache(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadPageEntitiesFromL2Cache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc)
        {
            return BaseDal.LoadPageEntitiesFromL2Cache<TS, TDto>(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> LoadPageEntitiesNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

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
        public virtual IQueryable<TDto> LoadPageEntitiesNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadPageEntitiesNoTracking<TS, TDto>(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadPageEntitiesFromCacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {
            return BaseDal.LoadPageEntitiesFromCacheNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadPageEntitiesFromCacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return BaseDal.LoadPageEntitiesFromCacheNoTracking<TS, TDto>(pageIndex, pageSize, out totalCount, where, orderby, isAsc, timespan);
        }

        /// <summary>
        /// 高效分页查询方法，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadPageEntitiesFromL2CacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadPageEntitiesFromL2CacheNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="totalCount">数据总数</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadPageEntitiesFromL2CacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.LoadPageEntitiesFromL2CacheNoTracking<TS, TDto>(pageIndex, pageSize, out totalCount, where, orderby, isAsc);
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteById(object id)
        {
            return BaseDal.DeleteById(id);
        }

        /// <summary>
        /// 根据ID删除实体并保存
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteByIdSaved(object id)
        {
            BaseDal.DeleteById(id);
            return SaveChanges() > 0;
        }

        /// <summary>
        /// 根据ID删除实体并保存（异步）
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public virtual async Task<int> DeleteByIdSavedAsync(object id)
        {
            BaseDal.DeleteById(id);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 删除实体并保存
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteEntity(T t)
        {
            return BaseDal.DeleteEntity(t);
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public virtual int DeleteEntity(Expression<Func<T, bool>> @where)
        {
            return BaseDal.DeleteEntity(where);
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public virtual int DeleteEntitySaved(Expression<Func<T, bool>> @where)
        {
            BaseDal.DeleteEntity(where);
            return SaveChanges();
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public virtual async Task<int> DeleteEntitySavedAsync(Expression<Func<T, bool>> @where)
        {
            BaseDal.DeleteEntity(where);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 根据条件删除实体（异步）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public virtual async Task<int> DeleteEntityAsync(Expression<Func<T, bool>> @where)
        {
            return await BaseDal.DeleteEntityAsync(where);
        }

        /// <summary>
        /// 删除实体并保存
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteEntitySaved(T t)
        {
            BaseDal.DeleteEntity(t);
            return SaveChanges() > 0;
        }

        /// <summary>
        /// 删除实体并保存（异步）
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public virtual async Task<int> DeleteEntitySavedAsync(T t)
        {
            BaseDal.DeleteEntity(t);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual bool UpdateEntity(T t)
        {
            return BaseDal.UpdateEntity(t);
        }

        /// <summary>
        /// 更新实体并保存
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual bool UpdateEntitySaved(T t)
        {
            BaseDal.UpdateEntity(t);
            return SaveChanges() > 0;
        }

        /// <summary>
        /// 更新实体并保存（异步）
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual async Task<int> UpdateEntitySavedAsync(T t)
        {
            BaseDal.UpdateEntity(t);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 根据条件更新实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual int UpdateEntity(Expression<Func<T, bool>> @where, T t)
        {
            return BaseDal.UpdateEntity(where, t);
        }

        /// <summary>
        /// 根据条件更新实体（异步）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual async Task<int> UpdateEntityAsync(Expression<Func<T, bool>> @where, T t)
        {
            return await BaseDal.UpdateEntityAsync(where, t);
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public virtual T AddEntity(T t)
        {
            return BaseDal.AddEntity(t);
        }

        /// <summary>
        /// 添加实体并保存
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public virtual T AddEntitySaved(T t)
        {
            T entity = BaseDal.AddEntity(t);
            bool b = SaveChanges() > 0;
            return b ? entity : null;
        }

        /// <summary>
        /// 添加实体并保存（异步）
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public virtual async Task<int> AddEntitySavedAsync(T t)
        {
            BaseDal.AddEntity(t);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="exp">更新条件</param>
        /// <param name="entities">实体集合</param>
        public virtual void AddOrUpdate(Expression<Func<T, object>> exp, params T[] entities)
        {
            BaseDal.AddOrUpdate(exp, entities);
        }

        /// <summary>
        /// 添加或更新并保存
        /// </summary>
        /// <param name="exp">更新条件</param>
        /// <param name="entities">实体集合</param>
        public virtual int AddOrUpdateSaved(Expression<Func<T, object>> exp, params T[] entities)
        {
            AddOrUpdate(exp, entities);
            return SaveChanges();
        }

        /// <summary>
        /// 添加或更新并保存（异步）
        /// </summary>
        /// <param name="exp">更新条件</param>
        /// <param name="entities">实体集合</param>
        public virtual async Task<int> AddOrUpdateSavedAsync(Expression<Func<T, object>> exp, params T[] entities)
        {
            AddOrUpdate(exp, entities);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 统一保存的方法
        /// </summary>
        /// <returns>受影响的行数</returns>
        public virtual int SaveChanges()
        {
            return BaseDal.SaveChanges();
        }

        /// <summary>
        /// 统一保存数据
        /// </summary>
        /// <returns>受影响的行数</returns>
        public virtual async Task<int> SaveChangesAsync()
        {
            return await BaseDal.SaveChangesAsync();
        }

        /// <summary>
        /// 判断实体是否在数据库中存在
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>是否存在</returns>
        public virtual bool Any(Expression<Func<T, bool>> @where)
        {
            return BaseDal.Any(where);
        }

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteEntities(IEnumerable<T> list)
        {
            return BaseDal.DeleteEntities(list);
        }

        /// <summary>
        /// 删除多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteEntitiesSaved(IEnumerable<T> list)
        {
            BaseDal.DeleteEntities(list);
            return SaveChanges() > 0;
        }

        /// <summary>
        /// 删除多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public virtual async Task<int> DeleteEntitiesSavedAsync(IEnumerable<T> list)
        {
            BaseDal.DeleteEntities(list);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        public virtual bool UpdateEntities(IEnumerable<T> list)
        {
            return BaseDal.UpdateEntities(list);
        }

        /// <summary>
        /// 更新多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        public virtual bool UpdateEntitiesSaved(IEnumerable<T> list)
        {
            BaseDal.UpdateEntities(list);
            return SaveChanges() > 0;
        }

        /// <summary>
        /// 更新多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        public virtual async Task<int> UpdateEntitiesSavedAsync(IEnumerable<T> list)
        {
            BaseDal.UpdateEntities(list);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// 添加多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>添加成功</returns>
        public virtual IEnumerable<T> AddEntities(IList<T> list)
        {
            IEnumerable<T> entities = BaseDal.AddEntities(list);
            SaveChanges();
            return entities;
        }

        /// <summary>
        /// 添加多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>添加成功</returns>
        public virtual async Task<IEnumerable<T>> AddEntitiesAsync(IList<T> list)
        {
            IEnumerable<T> entities = BaseDal.AddEntities(list);
            await SaveChangesAsync();
            return entities;
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">参数</param>
        /// <returns>泛型集合</returns>
        public virtual DbRawSqlQuery<TS> SqlQuery<TS>(string sql, params SqlParameter[] parameters)
        {
            return BaseDal.SqlQuery<TS>(sql, parameters);
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">参数</param>
        public virtual DbRawSqlQuery SqlQuery(Type t, string sql, params SqlParameter[] parameters)
        {
            return BaseDal.SqlQuery(t, sql, parameters);
        }

        /// <summary>
        /// 执行DML语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public virtual void ExecuteSql(string sql, params SqlParameter[] parameters)
        {
            BaseDal.ExecuteSql(sql, parameters);
        }

        /// <summary>
        /// 批量添加实体
        /// </summary>
        /// <param name="list">需要添加的实体</param>
        public virtual void BulkInsert(IEnumerable<T> list)
        {
            BaseDal.BulkInsert(list);
        }

        /// <summary>
        /// 统一批量保存数据
        /// </summary>
        public virtual void BulkSaveChanges()
        {
            BaseDal.BulkSaveChanges();
        }
    }
}