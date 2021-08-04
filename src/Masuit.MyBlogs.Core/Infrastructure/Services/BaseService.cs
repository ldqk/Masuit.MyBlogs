using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.Tools.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    /// <summary>
    /// 业务层基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseService<T> : IBaseService<T> where T : LuceneIndexableBaseEntity
    {
        public virtual IBaseRepository<T> BaseDal { get; set; }
        protected readonly ISearchEngine<DataContext> SearchEngine;
        protected readonly ILuceneIndexSearcher Searcher;

        public BaseService(IBaseRepository<T> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher)
        {
            BaseDal = repository;
            SearchEngine = searchEngine;
            Searcher = searcher;
        }

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
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCache()
        {
            return BaseDal.GetAllFromCache();
        }

        /// <summary>
        /// 从二级缓存获取所有实体
        /// </summary>
        /// <returns>还未执行的SQL语句</returns>
        public Task<List<T>> GetAllFromCacheAsync()
        {
            return BaseDal.GetAllFromCacheAsync();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAll<TDto>() where TDto : class
        {
            return BaseDal.GetAll<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetAllFromCache<TDto>() where TDto : class
        {
            return BaseDal.GetAllFromCache<TDto>();
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <returns>还未执行的SQL语句</returns>
        public Task<List<TDto>> GetAllFromCacheAsync<TDto>() where TDto : class
        {
            return BaseDal.GetAllFromCacheAsync<TDto>();
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
            return BaseDal.GetAll(orderby, isAsc);
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
            return BaseDal.GetAllNoTracking(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetAllFromCache<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAllFromCache(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<List<T>> GetAllFromCacheAsync<TS>(Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAllFromCacheAsync(@orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetAll<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
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
        public virtual IEnumerable<TDto> GetAllFromCache<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetAllFromCache<TS, TDto>(orderby, isAsc);
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<List<TDto>> GetAllFromCacheAsync<TS, TDto>(Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetAllFromCacheAsync<TS, TDto>(@orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetQuery(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetQuery(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        IOrderedQueryable<T> IBaseService<T>.GetQuery<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc)
        {
            return BaseDal.GetQuery(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<TDto> GetQuery<TDto>(Expression<Func<T, bool>> @where) where TDto : class
        {
            return BaseDal.GetQuery<TDto>(where);
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
        public virtual IQueryable<TDto> GetQuery<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetQuery<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetQueryFromCache(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetQueryFromCache(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<List<T>> GetQueryFromCacheAsync(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetQueryFromCacheAsync(@where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<T> GetQueryFromCache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetQueryFromCache(where, orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<List<T>> GetQueryFromCacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetQueryFromCacheAsync(@where, @orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IEnumerable<TDto> GetQueryFromCache<TDto>(Expression<Func<T, bool>> @where) where TDto : class
        {
            return BaseDal.GetQueryFromCache<TDto>(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个被AutoMapper映射后的集合，优先从二级缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<List<TDto>> GetQueryFromCacheAsync<TDto>(Expression<Func<T, bool>> @where) where TDto : class
        {
            return BaseDal.GetQueryFromCacheAsync<TDto>(@where);
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
        public virtual IEnumerable<TDto> GetQueryFromCache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetQueryFromCache<TS, TDto>(where, orderby, isAsc);
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
        public Task<List<TDto>> GetQueryFromCacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetQueryFromCacheAsync<TS, TDto>(@where, @orderby, isAsc);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IQueryable<T> GetQueryNoTracking(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetQueryNoTracking(where);
        }

        /// <summary>
        /// 基本查询方法，获取一个集合（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序方式</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual IOrderedQueryable<T> GetQueryNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetQueryNoTracking(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T Get(Expression<Func<T, bool>> @where)
        {
            return BaseDal.Get(where);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetFromCache(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFromCache(where);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public Task<T> GetFromCacheAsync(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetFromCacheAsync(where);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual T Get<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.Get(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public T GetFromCache<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFromCache(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public Task<T> GetFromCacheAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetFromCacheAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual TDto Get<TDto>(Expression<Func<T, bool>> @where) where TDto : class
        {
            return BaseDal.Get<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public TDto GetFromCache<TDto>(Expression<Func<T, bool>> @where) where TDto : class
        {
            return BaseDal.GetFromCache<TDto>(where);
        }

        /// <summary>
        /// 获取第一条被AutoMapper映射后的数据
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public Task<TDto> GetFromCacheAsync<TDto>(Expression<Func<T, bool>> @where) where TDto : class
        {
            return BaseDal.GetFromCacheAsync<TDto>(where);
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
        public virtual TDto Get<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.Get<TS, TDto>(where, orderby, isAsc);
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
        public Task<TDto> GetAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetAsync<TS, TDto>(where, orderby, isAsc);
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
        public TDto GetFromCache<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetFromCache<TS, TDto>(where, orderby, isAsc);
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
        public Task<TDto> GetFromCacheAsync<TS, TDto>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true) where TDto : class
        {
            return BaseDal.GetFromCacheAsync<TS, TDto>(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual Task<T> GetAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual Task<T> GetAsync(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetAsync(where);
        }

        /// <summary>
        /// 获取第一条数据（不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual T GetNoTracking(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetNoTracking(where);
        }

        /// <summary>
        /// 获取第一条数据（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual T GetNoTracking<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetNoTracking(where, orderby, isAsc);
        }

        /// <summary>
        /// 获取第一条数据（异步，不跟踪实体）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public virtual Task<T> GetNoTrackingAsync(Expression<Func<T, bool>> @where)
        {
            return BaseDal.GetNoTrackingAsync(where);
        }

        /// <summary>
        /// 获取第一条数据（异步，不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序</typeparam>
        /// <param name="where">查询条件</param>
        /// <param name="orderby">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <returns>实体</returns>
        public virtual Task<T> GetNoTrackingAsync<TS>(Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetNoTrackingAsync(where, orderby, isAsc);
        }

        /// <summary>
        /// 根据ID找实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public virtual T GetById(int id)
        {
            return BaseDal.GetById(id);
        }

        /// <summary>
        /// 根据ID找实体(异步)
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>实体</returns>
        public virtual Task<T> GetByIdAsync(int id)
        {
            return BaseDal.GetByIdAsync(id);
        }

        /// <summary>
        /// 高效分页查询方法
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual PagedList<T> GetPages<TS>(int pageIndex, int pageSize, Expression<Func<T, bool>> where, Expression<Func<T, TS>> orderby, bool isAsc = true)
        {
            return BaseDal.GetPages(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual PagedList<TDto> GetPages<TS, TDto>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc) where TDto : class
        {
            return BaseDal.GetPages<TS, TDto>(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<PagedList<T>> GetPagesAsync<TS>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc)
        {
            return BaseDal.GetPagesAsync(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<PagedList<TDto>> GetPagesAsync<TS, TDto>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc) where TDto : class
        {
            return BaseDal.GetPagesAsync<TS, TDto>(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual PagedList<T> GetPagesFromCache<TS>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetPagesFromCache(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<PagedList<T>> GetPagesFromCacheAsync<TS>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc)
        {
            return BaseDal.GetPagesFromCacheAsync(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <typeparam name="TDto">映射实体</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual PagedList<TDto> GetPagesFromCache<TS, TDto>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc) where TDto : class
        {
            return BaseDal.GetPagesFromCache<TS, TDto>(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法，优先从二级缓存读取，取出被AutoMapper映射后的数据集合
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public Task<PagedList<TDto>> GetPagesFromCacheAsync<TS, TDto>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc) where TDto : class
        {
            return BaseDal.GetPagesFromCacheAsync<TS, TDto>(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 高效分页查询方法（不跟踪实体）
        /// </summary>
        /// <typeparam name="TS">排序字段</typeparam>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="where">where Lambda条件表达式</param>
        /// <param name="orderby">orderby Lambda条件表达式</param>
        /// <param name="isAsc">升序降序</param>
        /// <returns>还未执行的SQL语句</returns>
        public virtual PagedList<T> GetPagesNoTracking<TS>(int pageIndex, int pageSize, Expression<Func<T, bool>> @where, Expression<Func<T, TS>> @orderby, bool isAsc = true)
        {
            return BaseDal.GetPagesNoTracking(pageIndex, pageSize, where, orderby, isAsc);
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public virtual bool DeleteById(int id)
        {
            return BaseDal.DeleteById(id);
        }

        /// <summary>
        /// 根据ID删除实体并保存（异步）
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public virtual Task<int> DeleteByIdAsync(int id)
        {
            return BaseDal.DeleteByIdAsync(id);
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
        public virtual Task<int> DeleteEntitySavedAsync(Expression<Func<T, bool>> @where)
        {
            BaseDal.DeleteEntity(where);
            return SaveChangesAsync();
        }

        /// <summary>
        /// 根据条件删除实体（异步）
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public virtual Task<int> DeleteEntityAsync(Expression<Func<T, bool>> @where)
        {
            return BaseDal.DeleteEntityAsync(where);
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
        public virtual Task<int> DeleteEntitySavedAsync(T t)
        {
            BaseDal.DeleteEntity(t);
            return SaveChangesAsync();
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
        /// 添加或更新实体
        /// </summary>
        /// <param name="key">更新键规则</param>
        /// <param name="t">需要保存的实体</param>
        /// <returns>保存成功</returns>
        public T AddOrUpdate<TKey>(Expression<Func<T, TKey>> key, T t)
        {
            return BaseDal.AddOrUpdate(key, t);
        }

        /// <summary>
        /// 添加或更新实体
        /// </summary>
        /// <param name="key">更新键规则</param>
        /// <param name="entities">需要保存的实体</param>
        /// <returns>保存成功</returns>
        public void AddOrUpdate<TKey>(Expression<Func<T, TKey>> key, params T[] entities)
        {
            BaseDal.AddOrUpdate(key, entities);
        }

        /// <summary>
        /// 添加或更新实体
        /// </summary>
        /// <param name="key">更新键规则</param>
        /// <param name="entities">需要保存的实体</param>
        /// <returns>保存成功</returns>
        public void AddOrUpdate<TKey>(Expression<Func<T, TKey>> key, IEnumerable<T> entities)
        {
            BaseDal.AddOrUpdate(key, entities);
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
        /// 添加或更新实体
        /// </summary>
        /// <param name="key">更新键规则</param>
        /// <param name="t">需要保存的实体</param>
        /// <returns>保存成功</returns>
        public T AddOrUpdateSaved<TKey>(Expression<Func<T, TKey>> key, T t)
        {
            BaseDal.AddOrUpdate(key, t);
            var b = BaseDal.SaveChanges() > 0;
            return b ? t : null;
        }

        /// <summary>
        /// 添加或更新实体
        /// </summary>
        /// <param name="key">更新键规则</param>
        /// <param name="entities">需要保存的实体</param>
        /// <returns>保存成功</returns>
        public bool AddOrUpdateSaved<TKey>(Expression<Func<T, TKey>> key, params T[] entities)
        {
            AddOrUpdate(key, entities);
            return SaveChanges() > 0;
        }

        /// <summary>
        /// 添加或更新实体
        /// </summary>
        /// <param name="key">更新键规则</param>
        /// <param name="t">需要保存的实体</param>
        /// <returns>保存成功</returns>
        public Task<int> AddOrUpdateSavedAsync<TKey>(Expression<Func<T, TKey>> key, T t)
        {
            AddOrUpdate(key, t);
            return SaveChangesAsync();
        }

        /// <summary>
        /// 添加或更新实体
        /// </summary>
        /// <param name="key">更新键规则</param>
        /// <param name="entities">需要保存的实体</param>
        /// <returns>保存成功</returns>
        public Task<int> AddOrUpdateSavedAsync<TKey>(Expression<Func<T, TKey>> key, IEnumerable<T> entities)
        {
            AddOrUpdate(key, entities);
            return SaveChangesAsync();
        }

        /// <summary>
        /// 添加实体并保存（异步）
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public virtual Task<int> AddEntitySavedAsync(T t)
        {
            BaseDal.AddEntity(t);
            return SaveChangesAsync();
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
        public virtual Task<int> SaveChangesAsync()
        {
            return BaseDal.SaveChangesAsync();
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
        /// 统计符合条件的个数
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns></returns>
        public int Count(Expression<Func<T, bool>> @where)
        {
            return BaseDal.Count(where);
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
        public virtual Task<int> DeleteEntitiesSavedAsync(IEnumerable<T> list)
        {
            BaseDal.DeleteEntities(list);
            return SaveChangesAsync();
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

        public virtual T this[int id]
        {
            get => GetById(id);
            set => AddEntity(value);
        }

        public virtual string this[int id, Expression<Func<T, string>> selector] => GetQuery(t => t.Id == id).Select(selector).FirstOrDefault();
        public virtual int this[int id, Expression<Func<T, int>> selector] => GetQuery(t => t.Id == id).Select(selector).FirstOrDefault();
        public virtual DateTime this[int id, Expression<Func<T, DateTime>> selector] => GetQuery(t => t.Id == id).Select(selector).FirstOrDefault();
        public virtual long this[int id, Expression<Func<T, long>> selector] => GetQuery(t => t.Id == id).Select(selector).FirstOrDefault();
        public virtual decimal this[int id, Expression<Func<T, decimal>> selector] => GetQuery(t => t.Id == id).Select(selector).FirstOrDefault();

        public static T operator +(BaseService<T> left, T right) => left.AddEntity(right);
        public static bool operator -(BaseService<T> left, T right) => left.DeleteEntity(right);
        public static bool operator -(BaseService<T> left, int id) => left.DeleteById(id);
    }
}