using Common;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
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
        public PostService(IPostRepository repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }
        public SearchResult<PostOutputDto> SearchPage(int page, int size, string keyword)
        {
            var searchResult = _searchEngine.ScoredSearch<Post>(new SearchOptions(keyword, page, size, typeof(Post)));
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
            return new SearchResult<PostOutputDto>()
            {
                Results = posts,
                Elapsed = searchResult.Elapsed,
                Total = searchResult.TotalHits
            };
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
        /// 更新实体并保存
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public override bool UpdateEntitySaved(Post t)
        {
            base.UpdateEntity(t);
            return _searchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 更新多个实体并保存
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        public override bool UpdateEntitiesSaved(IEnumerable<Post> list)
        {
            base.UpdateEntities(list);
            return _searchEngine.SaveChanges() > 0;
        }

        /// <summary>
        /// 更新多个实体并保存（异步）
        /// </summary>
        /// <param name="list">实体集合</param>
        /// <returns>更新成功</returns>
        public override Task<int> UpdateEntitiesSavedAsync(IEnumerable<Post> list)
        {
            base.UpdateEntities(list);
            return _searchEngine.SaveChangesAsync();
        }

        /// <summary>
        /// 更新实体并保存（异步）
        /// </summary>
        /// <param name="t">更新后的实体</param>
        /// <returns>更新成功</returns>
        public override Task<int> UpdateEntitySavedAsync(Post t)
        {
            base.UpdateEntity(t);
            return _searchEngine.SaveChangesAsync();
        }
    }
}