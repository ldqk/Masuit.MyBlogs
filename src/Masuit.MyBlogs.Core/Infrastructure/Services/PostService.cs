using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class PostService : BaseService<Post>, IPostService
    {
        private readonly IMemoryCache _memoryCache;

        public PostService(IPostRepository repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher, IMemoryCache memoryCache) : base(repository, searchEngine, searcher)
        {
            _memoryCache = memoryCache;
        }

        public SearchResult<PostOutputDto> SearchPage(int page, int size, string keyword)
        {
            var cacheKey = $"search:{keyword}:{page}:{size}";
            if (_memoryCache.TryGetValue<SearchResult<PostOutputDto>>(cacheKey, out var value))
            {
                return value;
            }

            var searchResult = _searchEngine.ScoredSearch<Post>(BuildSearchOptions(page, size, keyword));
            var posts = searchResult.Results.Select(p => p.Entity.Mapper<PostOutputDto>()).Where(p => p.Status == Status.Pended).ToList();
            var simpleHtmlFormatter = new SimpleHTMLFormatter("<span style='color:red;background-color:yellow;font-size: 1.1em;font-weight:700;'>", "</span>");
            var highlighter = new Highlighter(simpleHtmlFormatter, new Segment()) { FragmentSize = 200 };
            var keywords = _searcher.CutKeywords(keyword);
            foreach (var p in posts)
            {
                foreach (var s in keywords)
                {
                    string frag;
                    if (p.Title.Contains(s) && !string.IsNullOrEmpty(frag = highlighter.GetBestFragment(s, p.Title)))
                    {
                        p.Title = frag;
                        break;
                    }
                }

                bool handled = false;
                foreach (var s in keywords)
                {
                    string frag;
                    if (p.Content.Contains(s) && !string.IsNullOrEmpty(frag = highlighter.GetBestFragment(s, p.Content)))
                    {
                        p.Content = frag;
                        handled = true;
                        break;
                    }
                }

                if (p.Content.Length > 200 && !handled)
                {
                    p.Content = p.Content.Substring(0, 200);
                }
            }

            var result = new SearchResult<PostOutputDto>()
            {
                Results = posts,
                Elapsed = searchResult.Elapsed,
                Total = searchResult.TotalHits
            };

            return _memoryCache.Set(cacheKey, result, TimeSpan.FromHours(1));
        }

        private static SearchOptions BuildSearchOptions(int page, int size, string keyword)
        {
            var fields = new List<string>();
            var newkeywords = new List<string>();
            if (keyword.Contains("intitle:"))
            {
                fields.Add("Title");
                newkeywords.Add(keyword.Split(' ', '　').FirstOrDefault(s => s.Contains("intitle")).Split(':')[1]);
            }

            if (keyword.Contains("content:"))
            {
                fields.Add("Content");
                newkeywords.Add(keyword.Split(' ', '　').FirstOrDefault(s => s.Contains("content")).Split(':')[1]);
            }

            var searchOptions = fields.Any() ? new SearchOptions(newkeywords.Join(" "), page, size, fields.Join(",")) : new SearchOptions(keyword, page, size, typeof(Post));
            return searchOptions;
        }

        /// <summary>
        /// 添加实体并保存
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Post AddEntitySaved(Post t)
        {
            t = base.AddEntity(t);
            _searchEngine.SaveChanges(t.Status == Status.Pended);
            return t;
        }

        /// <summary>
        /// 添加实体并保存（异步）
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Task<int> AddEntitySavedAsync(Post t)
        {
            base.AddEntity(t);
            return _searchEngine.SaveChangesAsync(t.Status == Status.Pended);
        }

        /// <summary>
        /// 根据ID删除实体并保存
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public override bool DeleteByIdSaved(object id)
        {
            base.DeleteById(id);
            return _searchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 删除多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public override bool DeleteEntitiesSaved(IEnumerable<Post> list)
        {
            base.DeleteEntities(list);
            return _searchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 根据ID删除实体并保存（异步）
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteByIdSavedAsync(object id)
        {
            base.DeleteById(id);
            return _searchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 删除多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteEntitiesSavedAsync(IEnumerable<Post> list)
        {
            base.DeleteEntities(list);
            return _searchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public override int DeleteEntitySaved(Expression<Func<Post, bool>> @where)
        {
            base.DeleteEntity(@where);
            return _searchEngine.SaveChanges();
        }

        /// <summary>
        /// 删除实体并保存
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public override bool DeleteEntitySaved(Post t)
        {
            base.DeleteEntity(t);
            return _searchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteEntitySavedAsync(Expression<Func<Post, bool>> @where)
        {
            base.DeleteEntity(@where);
            return _searchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 删除实体并保存（异步）
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteEntitySavedAsync(Post t)
        {
            base.DeleteEntity(t);
            return _searchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 统一保存的方法
        /// </summary>
        /// <returns>受影响的行数</returns>
        public int SaveChanges(bool flushIndex)
        {
            return flushIndex ? _searchEngine.SaveChanges() : base.SaveChanges();
        }

        /// <summary>
        /// 统一保存数据
        /// </summary>
        /// <returns>受影响的行数</returns>
        public async Task<int> SaveChangesAsync(bool flushIndex)
        {
            return flushIndex ? await _searchEngine.SaveChangesAsync() : await base.SaveChangesAsync();
        }
    }
}