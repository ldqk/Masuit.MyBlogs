using CacheManager.Core;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
        public IPostService PostService { get; set; }

        public ICacheManager<string> CacheManager { get; set; }

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
            var nul = new List<PostDto>();
            ViewBag.Elapsed = 0;
            ViewBag.Total = 0;
            ViewBag.PageSize = size;
            ViewBag.Keyword = wd;
            string ip = ClientIP;
            string key = "Search:" + ip;
            if (CacheManager.Exists(key) && CacheManager.Get(key) != wd)
            {
                var hotSearches = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Week").Take(10).ToList();
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
                        IP = ClientIP
                    });
                    SearchDetailsService.SaveChanges();
                    HttpContext.Session.Set("search:" + wd, wd.ToByteArray());
                }

                var posts = PostService.SearchPage(page, size, wd);
                ViewBag.Elapsed = posts.Elapsed;
                ViewBag.Total = posts.Total;
                if (posts.Total > 1)
                {
                    CacheManager.AddOrUpdate(key, wd, s => wd);
                    CacheManager.Expire(key, TimeSpan.FromSeconds(10));
                }

                ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.PostList);
                ViewBag.hotSearches = new List<KeywordsRank>();
                return View(posts.Results);
            }

            ViewBag.hotSearches = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Week").Take(10).ToList();
            ViewData["page"] = new Pagination(page, size);
            return View(nul);
        }

        /// <summary>
        /// 关键词搜索记录
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [MyAuthorize, HttpPost, ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "search" }, VaryByHeader = "Cookie")]
        public ActionResult SearchList(int page = 1, int size = 10, string search = "")
        {
            var where = string.IsNullOrEmpty(search) ? (Expression<Func<SearchDetails, bool>>)(s => true) : s => s.KeyWords.Contains(search);
            var list = SearchDetailsService.GetPages<DateTime, SearchDetailsDto>(page, size, out int total, where, s => s.SearchTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 热词
        /// </summary>
        /// <returns></returns>
        [MyAuthorize, HttpPost, ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public ActionResult HotKey()
        {
            return ResultData(new
            {
                month = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Month"),
                week = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Week"),
                today = RedisHelper.Get<List<KeywordsRank>>("SearchRank:Today")
            });
        }

        /// <summary>
        /// 删除搜索记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, MyAuthorize]
        public ActionResult Delete(int id)
        {
            bool b = SearchDetailsService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }
    }
}