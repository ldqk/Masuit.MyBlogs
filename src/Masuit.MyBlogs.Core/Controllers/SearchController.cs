using Common;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Application;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.NoSQL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 站内搜索
    /// </summary>
    public class SearchController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        public ISearchDetailsService SearchDetailsService { get; set; }
        private readonly IPostService _postService;
        private readonly ISearchEngine<DataContext> _searchEngine;

        /// <summary>
        /// 站内搜索
        /// </summary>
        /// <param name="searchDetailsService"></param>
        /// <param name="postService"></param>
        /// <param name="searchEngine"></param>
        public SearchController(ISearchDetailsService searchDetailsService, IPostService postService, ISearchEngine<DataContext> searchEngine)
        {
            SearchDetailsService = searchDetailsService;
            _postService = postService;
            _searchEngine = searchEngine;
        }

        /// <summary>
        /// 搜索页
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("s/{wd?}/{page:int?}/{size:int?}"), ResponseCache(VaryByQueryKeys = new[] { "wd", "page", "size" }, VaryByHeader = HeaderNames.Cookie, Duration = 600)]
        public ActionResult Search(string wd = "", int page = 1, int size = 10)
        {
            var nul = new List<PostOutputDto>();
            ViewBag.Elapsed = 0;
            ViewBag.Total = 0;
            ViewBag.Keyword = wd ?? "";
            if (Regex.Match(wd, CommonHelper.BanRegex).Length > 0 || Regex.Match(wd, CommonHelper.ModRegex).Length > 0)
            {
                //ViewBag.Wd = "";
                return RedirectToAction("Search");
            }
            var start = DateTime.Today.AddDays(-7);
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                string key = HttpContext.Connection.Id;
                if (redisHelper.KeyExists(key) && !redisHelper.GetString(key).Equals(wd))
                {
                    var hotSearches = SearchDetailsService.LoadEntitiesFromL2CacheNoTracking(s => s.SearchTime > start, s => s.SearchTime, false).GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(7).Select(g => new KeywordsRankOutputDto()
                    {
                        KeyWords = g.First().KeyWords,
                        SearchCount = g.Count()
                    }).ToList();
                    ViewBag.hotSearches = hotSearches;
                    ViewBag.ErrorMsg = "10秒内只能搜索1次！";
                    return View(nul);
                }
                wd = wd.Trim().Replace("+", " ");
                if (!string.IsNullOrWhiteSpace(wd) && !wd.Contains("锟斤拷"))
                {
                    if (!HttpContext.Session.TryGetValue("search:" + wd, out _))
                    {
                        SearchDetailsService.AddEntity(new SearchDetails
                        {
                            KeyWords = wd,
                            SearchTime = DateTime.Now,
                            IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                        });
                        HttpContext.Session.Set("search:" + wd, wd);
                    }
                    var posts = _postService.SearchPage(page, size, wd);
                    ViewBag.Elapsed = posts.Elapsed;
                    ViewBag.Total = posts.Total;
                    SearchDetailsService.SaveChanges();
                    if (posts.Total > 1)
                    {
                        redisHelper.SetString(key, wd, TimeSpan.FromSeconds(10));
                    }
                    ViewBag.hotSearches = new List<KeywordsRankOutputDto>();
                    return View(posts.Results);
                }
                ViewBag.hotSearches = SearchDetailsService.LoadEntitiesFromL2CacheNoTracking(s => s.SearchTime > start, s => s.SearchTime, false).GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(7).Select(g => new KeywordsRankOutputDto()
                {
                    KeyWords = g.FirstOrDefault().KeyWords,
                    SearchCount = g.Count()
                }).ToList();
                return View(nul);
            }
        }

        /// <summary>
        /// 关键词推荐
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authority, HttpPost, ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "search" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult SearchList(int page = 1, int size = 10, string search = "")
        {
            if (page <= 0)
            {
                page = 1;
            }
            var where = string.IsNullOrEmpty(search) ? (Expression<Func<SearchDetails, bool>>)(s => true) : s => s.KeyWords.Contains(search);
            var list = SearchDetailsService.LoadPageEntities<DateTime, SearchDetailsOutputDto>(page, size, out int total, where, s => s.SearchTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 热词
        /// </summary>
        /// <returns></returns>
        [Authority, HttpPost, ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult HotKey()
        {
            var start = DateTime.Today.AddMonths(-1);
            var temp = SearchDetailsService.LoadEntitiesNoTracking(s => s.SearchTime > start, s => s.SearchTime, false).ToList();
            var month = temp.GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(30).Select(g => new
            {
                Keywords = g.FirstOrDefault().KeyWords,
                Count = g.Count()
            }).ToList();
            var week = temp.Where(s => s.SearchTime > DateTime.Today.AddDays(-7)).GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(30).Select(g => new
            {
                Keywords = g.FirstOrDefault().KeyWords,
                Count = g.Count()
            }).ToList();
            var today = temp.Where(s => s.SearchTime > DateTime.Today).GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(30).Select(g => new
            {
                Keywords = g.FirstOrDefault().KeyWords,
                Count = g.Count()
            }).ToList();
            return ResultData(new
            {
                month,
                week,
                today
            });
        }

        /// <summary>
        /// 删除搜索记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, Authority]
        public ActionResult Delete(int id)
        {
            bool b = SearchDetailsService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }
    }
}