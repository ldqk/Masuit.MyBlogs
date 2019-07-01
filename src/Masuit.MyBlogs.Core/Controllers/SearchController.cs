using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
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
        }

        /// <summary>
        /// 搜索页
        /// </summary>
        /// <param name="wd"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("s/{wd?}/{page:int?}/{size:int?}")]
        public ActionResult Search(string wd = "", int page = 1, int size = 15)
        {
            var nul = new List<PostOutputDto>();
            ViewBag.Elapsed = 0;
            ViewBag.Total = 0;
            ViewBag.PageSize = size;
            ViewBag.Keyword = wd;
            if (Regex.Match(wd ?? "", CommonHelper.BanRegex + "|" + CommonHelper.ModRegex).Length > 0)
            {
                return RedirectToAction("Search");
            }

            string key = "Search:" + HttpContext.Session.Id;
            if (RedisHelper.Exists(key) && !RedisHelper.Get(key).Equals(wd))
            {
                var hotSearches = RedisHelper.Get<List<KeywordsRankOutputDto>>("SearchRank:Week").Take(10).ToList();
                ViewBag.hotSearches = hotSearches;
                ViewBag.ErrorMsg = "10秒内只能搜索1次！";
                return View(nul);
            }

            wd = wd?.Trim().Replace("+", " ");
            if (!string.IsNullOrWhiteSpace(wd) && !wd.Contains("锟斤拷"))
            {
                if (!HttpContext.Session.TryGetValue("search:" + wd, out _) && !HttpContext.Request.IsRobot())
                {
                    SearchDetailsService.AddEntity(new SearchDetails
                    {
                        KeyWords = wd,
                        SearchTime = DateTime.Now,
                        IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    });
                    SearchDetailsService.SaveChanges();
                    HttpContext.Session.Set("search:" + wd, wd.ToByteArray());
                }

                var posts = _postService.SearchPage(page, size, wd);
                ViewBag.Elapsed = posts.Elapsed;
                ViewBag.Total = posts.Total;
                if (posts.Total > 1)
                {
                    RedisHelper.Set(key, wd);
                    RedisHelper.Expire(key, TimeSpan.FromSeconds(10));
                }

                ViewBag.hotSearches = new List<KeywordsRankOutputDto>();
                return View(posts.Results);
            }

            ViewBag.hotSearches = RedisHelper.Get<List<KeywordsRankOutputDto>>("SearchRank:Week").Take(10).ToList();
            return View(nul);
        }

        /// <summary>
        /// 关键词搜索记录
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
            return ResultData(new
            {
                month = RedisHelper.Get<List<KeywordsRankOutputDto>>("SearchRank:Month"),
                week = RedisHelper.Get<List<KeywordsRankOutputDto>>("SearchRank:Week"),
                today = RedisHelper.Get<List<KeywordsRankOutputDto>>("SearchRank:Today")
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