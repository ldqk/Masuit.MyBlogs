using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using EFSecondLevelCache;
using EntityFramework.Caching;
using EntityFramework.Extensions;
using IDAL;
using Masuit.Tools;
using Masuit.Tools.Net;
using Masuit.Tools.Systems;
using Models.Application;

namespace DAL
{
    /// <summary>
    /// DAL基类
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public class BaseDal<T> : Disposable, IBaseDal<T> where T : class, new()
    {
        public virtual DataContext DataContext { get; set; } = WebExtension.GetDbContext<DataContext>();

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetAll()
        {
            return DataContext.Set<T>();
        }

        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetAllNoTracking()
        {
            return DataContext.Set<T>().AsNoTracking();
        }

        /// <summary>
        /// 从一级缓存获取所有实体
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCache(int timespan = 20)
        {
            return DataContext.Set<T>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCacheNoTracking(int timespan = 20)
        {
            return DataContext.Set<T>().AsNoTracking().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 获取所有实体（不跟踪）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheNoTrackingAsync(int timespan = 20)
        {
            return await DataContext.Set<T>().AsNoTracking().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 从一级缓存获取所有实体（异步）
        /// </summary>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheAsync(int timespan = 20)
        {
            return await DataContext.Set<T>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual EFCachedQueryable<T> GetAllFromL2Cache()
        {
            return DataContext.Set<T>().Cacheable();
        }

        /// <summary>
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual EFCachedQueryable<T> GetAllFromL2CacheNoTracking()
        {
            return DataContext.Set<T>().AsNoTracking().Cacheable();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAll<TDto>()
        {
            return DataContext.Set<T>().ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAllNoTracking<TDto>()
        {
            return DataContext.Set<T>().AsNoTracking().ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCache<TDto>(int timespan = 20) where TDto : class
        {
            return DataContext.Set<T>().ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheAsync<TDto>(int timespan = 20) where TDto : class
        {
            return await DataContext.Set<T>().ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCacheNoTracking<TDto>(int timespan = 20) where TDto : class
        {
            return DataContext.Set<T>().AsNoTracking().ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheNoTrackingAsync<TDto>(int timespan = 20) where TDto : class
        {
            return await DataContext.Set<T>().AsNoTracking().ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }


        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2Cache<TDto>()
        {
            return DataContext.Set<T>().ProjectToQueryable<TDto>().Cacheable();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2CacheNoTracking<TDto>()
        {
            return DataContext.Set<T>().AsNoTracking().ProjectToQueryable<TDto>().Cacheable();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> GetAll<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return isAsc ? DataContext.Set<T>().OrderBy(orderby) : DataContext.Set<T>().OrderByDescending(orderby);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> GetAll<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            IOrderedQueryable<T> query;
            if (isAsc)
            {
                query = DataContext.Set<T>().OrderBy(@orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenBy(e);
                    }
                }
            }
            else
            {
                query = DataContext.Set<T>().OrderByDescending(@orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenByDescending(e);
                    }
                }
            }
            return query;
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> GetAllNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return isAsc ? DataContext.Set<T>().AsNoTracking().OrderBy(orderby) : DataContext.Set<T>().AsNoTracking().OrderByDescending(orderby);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> GetAllNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            IOrderedQueryable<T> query;
            if (isAsc)
            {
                query = DataContext.Set<T>().AsNoTracking().OrderBy(@orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenBy(e);
                    }
                }
            }
            else
            {
                query = DataContext.Set<T>().AsNoTracking().OrderByDescending(@orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenByDescending(e);
                    }
                }
            }
            return query;
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
            return GetAll(orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCache<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby)
        {
            return GetAll(orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return await GetAll(orderby, isAsc).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheAsync<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby)
        {
            return await GetAll(orderby, isAsc, thenby).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return GetAllNoTracking(orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCacheNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby)
        {
            return GetAllNoTracking(orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return await GetAllNoTracking(orderby, isAsc).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> GetAllFromCacheNoTrackingAsync<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby)
        {
            return await GetAllNoTracking(orderby, isAsc, thenby).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return GetAll(orderby, isAsc).Cacheable();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromL2Cache<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return GetAll(orderby, isAsc, thenby).Cacheable();
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
            return GetAllNoTracking(orderby, isAsc).Cacheable();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromL2CacheNoTracking<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return GetAllNoTracking(orderby, isAsc, thenby).Cacheable();
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
            return GetAll(orderby, isAsc).ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAll<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return GetAll(orderby, isAsc, thenby).ProjectToQueryable<TDto>();
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
            return GetAllNoTracking(orderby, isAsc).ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAllNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return GetAllNoTracking(orderby, isAsc, thenby).ProjectToQueryable<TDto>();
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
            return GetAll<TS, TDto>(orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCache<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return GetAll<TS, TDto>(orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return await GetAll<TS, TDto>(orderby, isAsc).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <param name="thenby">次排序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheAsync<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return await GetAll<TS, TDto>(orderby, isAsc, thenby).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return GetAllNoTracking<TS, TDto>(orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<TDto> GetAllFromCacheNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return GetAllNoTracking<TS, TDto>(orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return await GetAllNoTracking(orderby, isAsc).ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
        public virtual async Task<IEnumerable<TDto>> GetAllFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 20, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return await GetAllNoTracking(orderby, isAsc, thenby).ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return GetAll(orderby, isAsc).ProjectToQueryable<TDto>().Cacheable();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2Cache<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return GetAll(orderby, isAsc, thenby).ProjectToQueryable<TDto>().Cacheable();
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
            return GetAllNoTracking(orderby, isAsc).ProjectToQueryable<TDto>().Cacheable();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return GetAllNoTracking(orderby, isAsc, thenby).ProjectToQueryable<TDto>().Cacheable();
        }

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> LoadEntities(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> LoadEntities<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return isAsc ? DataContext.Set<T>().Where(@where).OrderBy(orderby) : DataContext.Set<T>().Where(@where).OrderByDescending(orderby);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> LoadEntities<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            IOrderedQueryable<T> query;
            if (isAsc)
            {
                query = DataContext.Set<T>().Where(@where).OrderBy(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenBy(e);
                    }
                }
            }
            else
            {
                query = DataContext.Set<T>().Where(@where).OrderByDescending(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenByDescending(e);
                    }
                }
            }
            return query;
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> LoadEntities<TDto>(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).ProjectToQueryable<TDto>();
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
            return LoadEntities(where, orderby, isAsc).ProjectToQueryable<TDto>();
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
        public virtual IQueryable<TDto> LoadEntities<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntities(where, orderby, isAsc, thenby).ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromCache(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return DataContext.Set<T>().Where(@where).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return LoadEntities(where, orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<T> LoadEntitiesFromCache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntities(where, orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromCache<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return DataContext.Set<T>().Where(@where).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return LoadEntities(where, orderby, isAsc).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<TDto> LoadEntitiesFromCache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return LoadEntities(where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2Cache(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).Cacheable();
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
            return LoadEntities(where, orderby, isAsc).Cacheable();
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2Cache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntities(where, orderby, isAsc, thenby).Cacheable();
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromL2Cache<TDto>(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).Cacheable().ProjectToQueryable<TDto>();
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
            return LoadEntities(where, orderby, isAsc).Cacheable().ProjectToQueryable<TDto>();
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
        public virtual IEnumerable<TDto> LoadEntitiesFromL2Cache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntities(where, orderby, isAsc, thenby).Cacheable().ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<T>> LoadEntitiesAsync(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where)).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby) : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby)).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IOrderedQueryable<T>> LoadEntitiesAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query;
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<TDto>> LoadEntitiesAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).ProjectToQueryable<TDto>()).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby).ProjectToQueryable<TDto>() : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby).ProjectToQueryable<TDto>()).ConfigureAwait(true);
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
        public virtual async Task<IQueryable<TDto>> LoadEntitiesAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query.ProjectToQueryable<TDto>();
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return await WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return await LoadEntities(where, orderby, isAsc).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby)
        {
            return await LoadEntities(where, orderby, isAsc, thenby).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return await DataContext.Set<T>().Where(@where).ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return await LoadEntities(where, orderby, isAsc).ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return await LoadEntities(where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheAsync(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).Cacheable()).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby).Cacheable() : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby).Cacheable()).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query.Cacheable();
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).ProjectToQueryable<TDto>().Cacheable()).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby).ProjectToQueryable<TDto>().Cacheable() : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby).ProjectToQueryable<TDto>().Cacheable()).ConfigureAwait(true);
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
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query.ProjectToQueryable<TDto>().Cacheable();
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> LoadEntitiesNoTracking(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).AsNoTracking();
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
            return isAsc ? DataContext.Set<T>().Where(@where).AsNoTracking().OrderBy(orderby) : DataContext.Set<T>().Where(@where).AsNoTracking().OrderByDescending(orderby);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> LoadEntitiesNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            IOrderedQueryable<T> query;
            if (isAsc)
            {
                query = DataContext.Set<T>().Where(@where).AsNoTracking().OrderBy(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenBy(e);
                    }
                }
            }
            else
            {
                query = DataContext.Set<T>().Where(@where).AsNoTracking().OrderByDescending(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenByDescending(e);
                    }
                }
            }
            return query;
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> LoadEntitiesNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).AsNoTracking().ProjectToQueryable<TDto>();
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
            return LoadEntitiesNoTracking(where, orderby, isAsc).ProjectToQueryable<TDto>();
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
        public virtual IQueryable<TDto> LoadEntitiesNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntitiesNoTracking(where, orderby, isAsc, thenby).ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<T> LoadEntitiesFromCacheNoTracking(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return DataContext.Set<T>().Where(@where).AsNoTracking().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return LoadEntitiesNoTracking(where, orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<T> LoadEntitiesFromCacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntitiesNoTracking(where, orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromCacheNoTracking<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return DataContext.Set<T>().Where(@where).AsNoTracking().ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
            return LoadEntitiesNoTracking(where, orderby, isAsc).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<TDto> LoadEntitiesFromCacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return LoadEntitiesNoTracking(where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2CacheNoTracking(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).AsNoTracking().Cacheable();
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
            return LoadEntitiesNoTracking(where, orderby, isAsc).Cacheable();
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> LoadEntitiesFromL2CacheNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntitiesNoTracking(where, orderby, isAsc, thenby).Cacheable();
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual IEnumerable<TDto> LoadEntitiesFromL2CacheNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).AsNoTracking().Cacheable().ProjectToQueryable<TDto>();
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
            return LoadEntitiesNoTracking(where, orderby, isAsc).Cacheable().ProjectToQueryable<TDto>();
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
        public virtual IEnumerable<TDto> LoadEntitiesFromL2CacheNoTracking<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadEntitiesNoTracking(where, orderby, isAsc, thenby).Cacheable().ProjectToQueryable<TDto>();
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IQueryable<T>> LoadEntitiesNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking()).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby).AsNoTracking() : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby).AsNoTracking()).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<IQueryable<T>> LoadEntitiesNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query;
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IQueryable<TDto>> LoadEntitiesNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().ProjectToQueryable<TDto>()).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby).AsNoTracking().ProjectToQueryable<TDto>() : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby).AsNoTracking().ProjectToQueryable<TDto>()).ConfigureAwait(true);
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
        public virtual async Task<IQueryable<TDto>> LoadEntitiesNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query.ProjectToQueryable<TDto>();
            }).ConfigureAwait(true);
        }

        /// <summary>
        ///  基本查询方法，获取一个集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheNoTrackingAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return await DataContext.Set<T>().Where(@where).AsNoTracking().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return await LoadEntitiesNoTracking(where, orderby, isAsc).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
        public virtual async Task<IEnumerable<T>> LoadEntitiesFromCacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby)
        {
            return await LoadEntitiesNoTracking(where, orderby, isAsc, thenby).FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        ///  基本查询方法，获取一个被AutoMapper映射后的集合，优先从缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体集合</returns>
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return await DataContext.Set<T>().Where(@where).AsNoTracking().ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
            return await LoadEntitiesNoTracking(where, orderby, isAsc).ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
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
        public virtual async Task<IEnumerable<TDto>> LoadEntitiesFromCacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return await LoadEntitiesNoTracking(where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().FromCacheAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan))).ConfigureAwait(true);
        }

        /// <summary>
        ///  基本查询方法，获取一个集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().Cacheable()).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby).AsNoTracking().Cacheable() : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby).AsNoTracking().Cacheable()).ConfigureAwait(true);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual async Task<EFCachedQueryable<T>> LoadEntitiesFromL2CacheNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query.Cacheable();
            }).ConfigureAwait(true);
        }

        /// <summary>
        ///  基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取(异步，不跟踪实体)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体集合</returns>
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await Task.Run(() => WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().ProjectToQueryable<TDto>().Cacheable()).ConfigureAwait(true);
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
            return await Task.Run(() => isAsc ? WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderBy(orderby).AsNoTracking().ProjectToQueryable<TDto>().Cacheable() : WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).OrderByDescending(orderby).AsNoTracking().ProjectToQueryable<TDto>().Cacheable()).ConfigureAwait(true);
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
        public virtual async Task<EFCachedQueryable<TDto>> LoadEntitiesFromL2CacheNoTrackingAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return await Task.Run(() =>
            {
                IOrderedQueryable<T> query;
                if (isAsc)
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderBy(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenBy(e);
                        }
                    }
                }
                else
                {
                    query = WebExtension.GetDbContext<DataContext>().Set<T>().Where(@where).AsNoTracking().OrderByDescending(@orderby);
                    if (thenby.Length > 0)
                    {
                        foreach (var e in thenby)
                        {
                            query = query.ThenByDescending(e);
                        }
                    }
                }
                return query.ProjectToQueryable<TDto>().Cacheable();
            }).ConfigureAwait(true);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntity(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().FirstOrDefault(where);
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
            return isAsc ? DataContext.Set<T>().OrderBy(orderby).FirstOrDefault(where) : DataContext.Set<T>().OrderByDescending(orderby).FirstOrDefault(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntity<TDto>(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(where).ProjectToFirstOrDefault<TDto>();
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
            return isAsc ? DataContext.Set<T>().Where(where).OrderBy(orderby).ProjectToFirstOrDefault<TDto>() : DataContext.Set<T>().Where(where).OrderByDescending(orderby).ProjectToFirstOrDefault<TDto>();
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromCache(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return LoadEntitiesFromCache(where, timespan).FirstOrDefault();
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
            return LoadEntitiesFromCache(where, orderby, isAsc, timespan).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromCache<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return LoadEntitiesFromCache<TDto>(where, timespan).FirstOrDefault();
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
            return LoadEntitiesFromCache<TS, TDto>(where, orderby, isAsc, timespan).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromL2Cache(Expression<Func<T, bool>> @where)
        {
            return LoadEntitiesFromL2Cache(where).FirstOrDefault();
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
            return LoadEntitiesFromL2Cache(where, orderby, isAsc).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromL2Cache<TDto>(Expression<Func<T, bool>> @where)
        {
            return LoadEntitiesFromL2Cache<TDto>(where).FirstOrDefault();
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
            return LoadEntitiesFromL2Cache<TS, TDto>(where, orderby, isAsc).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityAsync(Expression<Func<T, bool>> @where)
        {
            return await DataContext.Set<T>().FirstOrDefaultAsync(where).ConfigureAwait(true);
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
            return isAsc ? await DataContext.Set<T>().OrderBy(orderby).FirstOrDefaultAsync(where).ConfigureAwait(true) : await DataContext.Set<T>().OrderByDescending(orderby).FirstOrDefaultAsync(where).ConfigureAwait(true);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await DataContext.Set<T>().Where(where).ProjectToFirstOrDefaultAsync<TDto>().ConfigureAwait(true);
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
            return isAsc ? await DataContext.Set<T>().Where(where).OrderBy(orderby).ProjectToFirstOrDefaultAsync<TDto>().ConfigureAwait(true) : await DataContext.Set<T>().Where(where).OrderByDescending(orderby).ProjectToFirstOrDefaultAsync<TDto>().ConfigureAwait(true);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromCacheAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            var list = await LoadEntitiesFromCacheAsync(@where, timespan).ConfigureAwait(true);
            return list.FirstOrDefault();
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
            return (await LoadEntitiesFromCacheAsync(where, orderby, isAsc, timespan)).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromCacheAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return (await LoadEntitiesFromCacheAsync<TDto>(where, timespan)).FirstOrDefault();
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
            return (await LoadEntitiesFromCacheAsync<TS, TDto>(where, orderby, isAsc, timespan)).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromL2CacheAsync(Expression<Func<T, bool>> @where)
        {
            return (await LoadEntitiesFromL2CacheAsync(where)).FirstOrDefault();
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
            return (await LoadEntitiesFromL2CacheAsync(where, orderby, isAsc)).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取(异步)
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromL2CacheAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return (await LoadEntitiesFromL2CacheAsync<TDto>(where)).FirstOrDefault();
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
            return (await LoadEntitiesFromL2CacheAsync<TS, TDto>(where, orderby, isAsc)).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条数据（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityNoTracking(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().AsNoTracking().FirstOrDefault(where);
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
            return isAsc ? DataContext.Set<T>().OrderBy(orderby).AsNoTracking().FirstOrDefault(where) : DataContext.Set<T>().OrderByDescending(orderby).AsNoTracking().FirstOrDefault(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(where).AsNoTracking().ProjectToFirstOrDefault<TDto>();
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
            return isAsc ? DataContext.Set<T>().Where(where).OrderBy(orderby).AsNoTracking().ProjectToFirstOrDefault<TDto>() : DataContext.Set<T>().Where(where).OrderByDescending(orderby).AsNoTracking().ProjectToFirstOrDefault<TDto>();
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromCacheNoTracking(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return LoadEntitiesFromCacheNoTracking(where, timespan).FirstOrDefault();
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
            return LoadEntitiesFromCacheNoTracking(where, orderby, isAsc, timespan).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromCacheNoTracking<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return LoadEntitiesFromCacheNoTracking<TDto>(where, timespan).FirstOrDefault();
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
            return LoadEntitiesFromCacheNoTracking<TS, TDto>(where, orderby, isAsc, timespan).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFirstEntityFromL2CacheNoTracking(Expression<Func<T, bool>> @where)
        {
            return LoadEntitiesFromL2CacheNoTracking(where).FirstOrDefault();
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
            return LoadEntitiesFromL2CacheNoTracking(where, orderby, isAsc).FirstOrDefault();
        }


        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从二级缓存读取（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto GetFirstEntityFromL2CacheNoTracking<TDto>(Expression<Func<T, bool>> @where)
        {
            return LoadEntitiesFromL2CacheNoTracking<TDto>(where).FirstOrDefault();
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
            return LoadEntitiesFromL2CacheNoTracking<TS, TDto>(where, orderby, isAsc).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条数据（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return await DataContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(where).ConfigureAwait(true);
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
            return isAsc ? await DataContext.Set<T>().OrderBy(orderby).AsNoTracking().FirstOrDefaultAsync(where).ConfigureAwait(true) : await DataContext.Set<T>().OrderByDescending(orderby).AsNoTracking().FirstOrDefaultAsync(where).ConfigureAwait(true);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return await DataContext.Set<T>().Where(where).AsNoTracking().ProjectToFirstOrDefaultAsync<TDto>().ConfigureAwait(true);
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
            return isAsc ? await DataContext.Set<T>().Where(where).OrderBy(orderby).AsNoTracking().ProjectToFirstOrDefaultAsync<TDto>().ConfigureAwait(true) : await DataContext.Set<T>().Where(where).OrderByDescending(orderby).AsNoTracking().ProjectToFirstOrDefaultAsync<TDto>().ConfigureAwait(true);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromCacheNoTrackingAsync(Expression<Func<T, bool>> @where, int timespan = 30)
        {
            return (await LoadEntitiesFromCacheNoTrackingAsync(where, timespan)).FirstOrDefault();
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
            return (await LoadEntitiesFromCacheNoTrackingAsync(where, orderby, isAsc, timespan)).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="timespan">缓存过期时间</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromCacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where, int timespan = 30) where TDto : class
        {
            return (await LoadEntitiesFromCacheNoTrackingAsync<TDto>(where, timespan)).FirstOrDefault();
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
            return (await LoadEntitiesFromCacheNoTrackingAsync<TS, TDto>(where, orderby, isAsc, timespan)).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetFirstEntityFromL2CacheNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return (await LoadEntitiesFromL2CacheNoTrackingAsync(where)).FirstOrDefault();
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
            return (await LoadEntitiesFromL2CacheNoTrackingAsync(where, orderby, isAsc)).FirstOrDefault();
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据，优先从缓存读取（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual async Task<TDto> GetFirstEntityFromL2CacheNoTrackingAsync<TDto>(Expression<Func<T, bool>> @where)
        {
            return (await LoadEntitiesFromL2CacheNoTrackingAsync<TDto>(where)).FirstOrDefault();
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
            return (await LoadEntitiesFromL2CacheNoTrackingAsync<TS, TDto>(where, orderby, isAsc)).FirstOrDefault();
        }

        /// <summary>
        /// 根据ID找实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public virtual T GetById(object id)
        {
            return DataContext.Set<T>().Find(id);
        }

        /// <summary>
        /// 根据ID找实体(异步)
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public virtual async Task<T> GetByIdAsync(object id)
        {
            return await DataContext.Set<T>().FindAsync(id).ConfigureAwait(true);
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
        public virtual IQueryable<T> LoadPageEntities<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc)
        {
            var temp = DataContext.Set<T>().Where(where);
            totalCount = temp.Count();
            if (pageIndex * pageSize > totalCount)
            {
                pageIndex = (int)Math.Ceiling(totalCount / (pageSize * 1.0));
            }

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            return isAsc ? temp.OrderBy(orderby).Skip(pageSize * (pageIndex - 1)).Take(pageSize) : temp.OrderByDescending(orderby).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
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
        public virtual IQueryable<T> LoadPageEntities<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc, params Expression<Func<T, TS>>[] thenby)
        {
            var temp = DataContext.Set<T>().Where(where);
            totalCount = temp.Count();
            if (pageIndex * pageSize > totalCount)
            {
                pageIndex = (int)Math.Ceiling(totalCount / (pageSize * 1.0));
            }

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            IOrderedQueryable<T> query;
            if (isAsc)
            {
                query = temp.OrderBy(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenBy(e);
                    }
                }
            }
            else
            {
                query = temp.OrderByDescending(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenByDescending(e);
                    }
                }
            }
            return query.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

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
        public virtual IQueryable<TDto> LoadPageEntities<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc).ProjectToQueryable<TDto>();
        }

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
        public virtual IQueryable<TDto> LoadPageEntities<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).ProjectToQueryable<TDto>();
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
        public virtual IEnumerable<T> LoadPageEntitiesFromCache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc, int timespan = 30)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<T> LoadPageEntitiesFromCache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc, int timespan = 30, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromCache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc, int timespan = 30) where TDto : class
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromCache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc, int timespan = 30, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

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
        public virtual IEnumerable<T> LoadPageEntitiesFromL2Cache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc).Cacheable();
        }

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
        public virtual IEnumerable<T> LoadPageEntitiesFromL2Cache<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).Cacheable();
        }

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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromL2Cache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc).ProjectToQueryable<TDto>().Cacheable();
        }

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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromL2Cache<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadPageEntities(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().Cacheable();
        }

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
        public virtual IQueryable<T> LoadPageEntitiesNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            var temp = DataContext.Set<T>().Where(where).AsNoTracking();
            totalCount = temp.Count();
            if (pageIndex * pageSize > totalCount)
            {
                pageIndex = (int)Math.Ceiling(totalCount / (pageSize * 1.0));
            }

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            return isAsc ? temp.OrderBy(orderby).Skip(pageSize * (pageIndex - 1)).Take(pageSize) : temp.OrderByDescending(orderby).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

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
        public virtual IQueryable<T> LoadPageEntitiesNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            var temp = DataContext.Set<T>().Where(where).AsNoTracking();
            totalCount = temp.Count();
            if (pageIndex * pageSize > totalCount)
            {
                pageIndex = (int)Math.Ceiling(totalCount / (pageSize * 1.0));
            }

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }
            IOrderedQueryable<T> query;
            if (isAsc)
            {
                query = temp.OrderBy(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenBy(e);
                    }
                }
            }
            else
            {
                query = temp.OrderByDescending(orderby);
                if (thenby.Length > 0)
                {
                    foreach (var e in thenby)
                    {
                        query = query.ThenByDescending(e);
                    }
                }
            }
            return query.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
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
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc).ProjectToQueryable<TDto>();
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
        public virtual IQueryable<TDto> LoadPageEntitiesNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).ProjectToQueryable<TDto>();
        }

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
        public virtual IEnumerable<T> LoadPageEntitiesFromCacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30)
        {

            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

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
        public virtual IEnumerable<T> LoadPageEntitiesFromCacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby)
        {

            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromCacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30) where TDto : class
        {
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromCacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, int timespan = 30, params Expression<Func<T, TS>>[] thenby) where TDto : class
        {
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(timespan)));
        }

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
        public virtual IEnumerable<T> LoadPageEntitiesFromL2CacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc).Cacheable();
        }

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
        public virtual IEnumerable<T> LoadPageEntitiesFromL2CacheNoTracking<TS>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).Cacheable();
        }

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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromL2CacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc).ProjectToQueryable<TDto>().Cacheable();
        }

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
        public virtual IEnumerable<TDto> LoadPageEntitiesFromL2CacheNoTracking<TS, TDto>(int pageIndex, int pageSize, out int totalCount, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true, params Expression<Func<T, TS>>[] thenby)
        {
            return LoadPageEntitiesNoTracking(pageIndex, pageSize, out totalCount, where, orderby, isAsc, thenby).ProjectToQueryable<TDto>().Cacheable();
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteById(object id)
        {
            T t = GetById(id);
            if (t != null)
            {
                DataContext.Entry(t).State = EntityState.Deleted;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteEntity(T t)
        {
            DataContext.Entry(t).State = EntityState.Unchanged;
            DataContext.Entry(t).State = EntityState.Deleted;
            return true;
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public virtual int DeleteEntity(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Where(@where).Delete();
        }

        /// <summary>
        /// 根据条件删除实体（异步）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public virtual async Task<int> DeleteEntityAsync(Expression<Func<T, bool>> @where)
        {
            return await DataContext.Set<T>().Where(@where).DeleteAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual bool UpdateEntity(T t)
        {
            DataContext.Entry(t).State = EntityState.Modified;
            return true;
        }

        /// <summary>
        /// 根据条件更新实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual int UpdateEntity(Expression<Func<T, bool>> @where, T t)
        {
            return DataContext.Set<T>().Where(@where).Update(ts => t);
        }

        /// <summary>
        /// 根据条件更新实体（异步）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public virtual async Task<int> UpdateEntityAsync(Expression<Func<T, bool>> @where, T t)
        {
            return await DataContext.Set<T>().Where(@where).UpdateAsync(ts => t).ConfigureAwait(true);
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public virtual T AddEntity(T t)
        {
            DataContext.Entry(t).State = EntityState.Added;
            return t;
        }

        /// <summary>
        /// 批量添加实体
        /// </summary>
        /// <param name="list">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public virtual void BulkInsert(IEnumerable<T> list)
        {
            DataContext.BulkInsert(list);
        }

        /// <summary>
        /// 添加或更新
        /// </summary>
        /// <param name="exp">更新条件</param>
        /// <param name="entities">实体集合</param>
        /// <returns></returns>
        public virtual void AddOrUpdate(Expression<Func<T, object>> exp, params T[] entities)
        {
            DataContext.Set<T>().AddOrUpdate(exp, entities);
        }

        /// <summary>
        /// 统一保存数据
        /// </summary>
        /// <returns>受影响的行数</returns>
        public virtual int SaveChanges()
        {
            return DataContext.SaveChanges();
        }

        /// <summary>
        /// 统一批量保存数据
        /// </summary>
        /// <returns>受影响的行数</returns>
        public virtual void BulkSaveChanges()
        {
            DataContext.BulkSaveChanges();
        }

        /// <summary>
        /// 统一保存数据（异步）
        /// </summary>
        /// <returns>受影响的行数</returns>
        public virtual async Task<int> SaveChangesAsync()
        {
            return await DataContext.SaveChangesAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// 判断实体是否在数据库中存在
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>是否存在</returns>
        public virtual bool Any(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Any(where);
        }

        /// <summary>
        /// 符合条件的个数
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>是否存在</returns>
        public virtual int Count(Expression<Func<T, bool>> @where)
        {
            return DataContext.Set<T>().Count(where);
        }

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteEntities(IEnumerable<T> list)
        {
            list.ForEach(t => { DeleteEntity(t); });
            return true;
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        public virtual bool UpdateEntities(IEnumerable<T> list)
        {
            list.ForEach(t => { UpdateEntity(t); });
            return true;
        }

        /// <summary>
        /// 添加多个实体
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>添加成功</returns>
        public virtual IEnumerable<T> AddEntities(IList<T> list)
        {
            //foreach (T t in list)
            //{
            //    yield return AddEntity(t);
            //}
            DataContext.BulkInsert(list);
            return list;
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
            return DataContext.Database.SqlQuery<TS>(sql, parameters);
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters">参数</param>
        public virtual DbRawSqlQuery SqlQuery(Type t, string sql, params SqlParameter[] parameters)
        {
            return DataContext.Database.SqlQuery(t, sql, parameters);
        }

        /// <summary>
        /// 执行DML语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        public virtual void ExecuteSql(string sql, params SqlParameter[] parameters)
        {
            DataContext.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, sql, parameters);
        }

        public override void Dispose(bool disposing)
        {
            DataContext?.Dispose();
            DataContext = null;
        }
    }
}