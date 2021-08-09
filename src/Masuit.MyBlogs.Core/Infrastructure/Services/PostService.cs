using CacheManager.Core;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Html;
using PanGu;
using PanGu.HighLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class PostService : BaseService<Post>, IPostService
    {
        private readonly ICacheManager<SearchResult<PostDto>> _cacheManager;
        private readonly ICacheManager<List<Post>> _searchCacheManager;
        private readonly ICacheManager<Dictionary<string, int>> _tagCacheManager;

        public PostService(IPostRepository repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher, ICacheManager<SearchResult<PostDto>> cacheManager, ICacheManager<List<Post>> searchCacheManager, ICacheManager<Dictionary<string, int>> tagCacheManager) : base(repository, searchEngine, searcher)
        {
            _cacheManager = cacheManager;
            _searchCacheManager = searchCacheManager;
            _tagCacheManager = tagCacheManager;
        }

        public List<Post> ScoreSearch(int page, int size, string keyword)
        {
            var cacheKey = $"scoreSearch:{keyword}:{page}:{size}";
            return _searchCacheManager.GetOrAdd(cacheKey, s =>
            {
                _searchCacheManager.Expire(cacheKey, TimeSpan.FromHours(1));
                return SearchEngine.ScoredSearch<Post>(BuildSearchOptions(page, size, keyword)).Results.Select(r => r.Entity).Distinct().ToList();
            });
        }

        public SearchResult<PostDto> SearchPage(int page, int size, string keyword)
        {
            var cacheKey = $"search:{keyword}:{page}:{size}";
            if (_cacheManager.Exists(cacheKey))
            {
                return _cacheManager.Get(cacheKey);
            }

            var searchResult = SearchEngine.ScoredSearch<Post>(BuildSearchOptions(page, size, keyword));
            var entities = searchResult.Results.Where(s => s.Entity.Status == Status.Published).DistinctBy(s => s.Entity.Id).ToList();
            var ids = entities.Select(s => s.Entity.Id).ToArray();
            var dic = GetQuery<PostDto>(p => ids.Contains(p.Id)).ToDictionary(p => p.Id);
            var posts = entities.Where(s => dic.ContainsKey(s.Entity.Id)).Select(s => dic[s.Entity.Id]).ToList();
            var simpleHtmlFormatter = new SimpleHTMLFormatter("<span style='color:red;background-color:yellow;font-size: 1.1em;font-weight:700;'>", "</span>");
            var highlighter = new Highlighter(simpleHtmlFormatter, new Segment()) { FragmentSize = 200 };
            var keywords = Searcher.CutKeywords(keyword);
            HighlightSegment(posts, keywords, highlighter);
            var result = new SearchResult<PostDto>()
            {
                Results = posts,
                Elapsed = searchResult.Elapsed,
                Total = searchResult.TotalHits
            };
            _cacheManager.Add(cacheKey, result);
            _cacheManager.Expire(cacheKey, TimeSpan.FromHours(1));
            return result;
        }

        /// <summary>
        /// 高亮截取处理
        /// </summary>
        /// <param name="posts"></param>
        /// <param name="keywords"></param>
        /// <param name="highlighter"></param>
        private static void HighlightSegment(List<PostDto> posts, List<string> keywords, Highlighter highlighter)
        {
            foreach (var p in posts)
            {
                p.Content = p.Content.RemoveHtmlTag();
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
                    p.Content = p.Content[..200];
                }
            }
        }

        private static SearchOptions BuildSearchOptions(int page, int size, string keyword)
        {
            keyword = Regex.Replace(keyword, @":\s+", ":");
            var fields = new List<string>();
            var newkeywords = new List<string>();
            foreach (var item in keyword.Split(' ', '　').Where(s => s.Contains(new[] { ":", "：" })))
            {
                var part = item.Split(':', '：');
                var field = typeof(Post).GetProperty(part[0], BindingFlags.IgnoreCase)?.Name;
                if (!string.IsNullOrEmpty(field))
                {
                    fields.Add(field);
                }

                newkeywords.Add(part[1]);
            }

            var searchOptions = fields.Any() ? new SearchOptions(newkeywords.Join(" "), page, size, fields.Join(",")) : new SearchOptions(keyword, page, size, typeof(Post));
            if (keyword.Contains(new[] { " ", ",", ";" }))
            {
                searchOptions.Score = 0.3f;
            }

            return searchOptions;
        }

        /// <summary>
        /// 文章所有tag
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetTags()
        {
            var key = "postTags";
            var dic = _tagCacheManager.Get(key);
            if (dic != null)
            {
                return dic;
            }

            dic = GetQuery(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).Distinct().ToList().SelectMany(s => s.Split(',', '，')).GroupBy(s => s).OrderByDescending(g => g.Count()).ToDictionary(g => g.Key, g => g.Count());
            _tagCacheManager.Add(key, dic);
            _tagCacheManager.Expire(key, DateTimeOffset.Now.AddDays(1));
            return dic;
        }

        /// <summary>
        /// 添加实体并保存
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Post AddEntitySaved(Post t)
        {
            t = base.AddEntity(t);
            SearchEngine.SaveChanges(t.Status == Status.Published);
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
            return SearchEngine.SaveChangesAsync(t.Status == Status.Published);
        }

        /// <summary>
        /// 根据ID删除实体并保存
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public override bool DeleteById(int id)
        {
            DeleteEntity(GetById(id));
            return SearchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 删除多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public override bool DeleteEntitiesSaved(IEnumerable<Post> list)
        {
            base.DeleteEntities(list);
            return SearchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 根据ID删除实体并保存（异步）
        /// </summary>
        /// <param name="id">实体id</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteByIdAsync(int id)
        {
            base.DeleteById(id);
            return SearchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 删除多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteEntitiesSavedAsync(IEnumerable<Post> list)
        {
            base.DeleteEntities(list);
            return SearchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public override int DeleteEntitySaved(Expression<Func<Post, bool>> @where)
        {
            base.DeleteEntity(@where);
            return SearchEngine.SaveChanges();
        }

        /// <summary>
        /// 删除实体并保存
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public override bool DeleteEntitySaved(Post t)
        {
            base.DeleteEntity(t);
            return SearchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteEntitySavedAsync(Expression<Func<Post, bool>> @where)
        {
            base.DeleteEntity(@where);
            return SearchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 删除实体并保存（异步）
        /// </summary>
        /// <param name="t">需要删除的实体</param>
        /// <returns>删除成功</returns>
        public override Task<int> DeleteEntitySavedAsync(Post t)
        {
            base.DeleteEntity(t);
            return SearchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 统一保存的方法
        /// </summary>
        /// <returns>受影响的行数</returns>
        public int SaveChanges(bool flushIndex)
        {
            return flushIndex ? SearchEngine.SaveChanges() : base.SaveChanges();
        }

        /// <summary>
        /// 统一保存数据
        /// </summary>
        /// <returns>受影响的行数</returns>
        public async Task<int> SaveChangesAsync(bool flushIndex)
        {
            return flushIndex ? await SearchEngine.SaveChangesAsync() : await base.SaveChangesAsync();
        }
    }
}