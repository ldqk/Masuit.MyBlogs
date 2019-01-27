using Common;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.NoSQL;
using Models.DTO;
using Models.Entity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using KeywordsRankOutputDto = Models.DTO.KeywordsRankOutputDto;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class SearchController : BaseController
    {
        public ISearchDetailsBll SearchDetailsBll { get; set; }
        private readonly IPostBll _postBll;

        public SearchController(ISearchDetailsBll searchDetailsBll, IPostBll postBll)
        {
            SearchDetailsBll = searchDetailsBll;
            _postBll = postBll;
        }

        [Route("s/{wd?}/{page:int?}/{size:int?}"), OutputCache(VaryByParam = "wd", Duration = 60)]
        public async Task<ActionResult> Search(string wd = "", int page = 1, int size = 10)
        {
            var nul = new List<PostOutputDto>();
            int count = 0;
            ViewBag.Elapsed = 0;
            ViewBag.Total = count;
            ViewBag.Keyword = wd;
            if (Regex.Match(wd, CommonHelper.BanRegex).Length > 0 || Regex.Match(wd, CommonHelper.ModRegex).Length > 0)
            {
                //ViewBag.Wd = "";
                return RedirectToAction("Search");
            }
            var start = DateTime.Today.AddDays(-7);
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                string key = Request.UserHostAddress + Request.UserAgent;
                if (redisHelper.KeyExists(key) && !redisHelper.GetString(key).Equals(wd))
                {
                    var hotSearches = (await SearchDetailsBll.LoadEntitiesFromCacheNoTrackingAsync(s => s.SearchTime > start, s => s.SearchTime, false)).GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(7).Select(g => new KeywordsRankOutputDto()
                    {
                        KeyWords = g.FirstOrDefault()?.KeyWords,
                        SearchCount = g.Count()
                    }).ToList();
                    ViewBag.hotSearches = hotSearches;
                    ViewBag.ErrorMsg = "10秒内只能搜索1次！";
                    return View(nul);
                }
                wd = wd.Trim().Replace("+", " ");
                if (!string.IsNullOrWhiteSpace(wd) && !wd.Contains("锟斤拷"))
                {
                    if (page == 1)
                    {
                        SearchDetailsBll.AddEntity(new SearchDetails()
                        {
                            KeyWords = wd,
                            SearchTime = DateTime.Now,
                            IP = Request.UserHostAddress
                        });
                    }
                    string[] keywords = LuceneHelper.CutKeywords(wd).ToArray();
                    Stopwatch sw = Stopwatch.StartNew();
                    var posts = _postBll.SearchPage(page, size, out count, keywords, p => p.ModifyDate);
                    ViewBag.Elapsed = sw.Elapsed.TotalMilliseconds;
                    ViewBag.Total = count;
                    SearchDetailsBll.SaveChanges();
                    if (count > 1)
                    {
                        redisHelper.SetString(key, wd, TimeSpan.FromSeconds(10));
                    }
                    ViewBag.hotSearches = new List<KeywordsRankOutputDto>();
                    return View(posts);
                }
                ViewBag.hotSearches = (await SearchDetailsBll.LoadEntitiesFromCacheNoTrackingAsync(s => s.SearchTime > start, s => s.SearchTime, false)).GroupBy(s => s.KeyWords.ToLower()).OrderByDescending(g => g.Count()).Take(7).Select(g => new KeywordsRankOutputDto()
                {
                    KeyWords = g.FirstOrDefault()?.KeyWords,
                    SearchCount = g.Count()
                }).ToList();
                return View(nul);
            }
        }

        [Authority, HttpPost]
        public ActionResult SearchList(int page = 1, int size = 10, string search = "")
        {
            if (page <= 0)
            {
                page = 1;
            }
            var @where = string.IsNullOrEmpty(search) ? (Expression<Func<SearchDetails, bool>>)(s => true) : s => s.KeyWords.Contains(search);
            var list = SearchDetailsBll.LoadPageEntitiesNoTracking<DateTime, SearchDetailsOutputDto>(page, size, out int total, where, s => s.SearchTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        [Authority, HttpPost]
        public ActionResult HotKey()
        {
            var start = DateTime.Today.AddMonths(-1);
            var temp = SearchDetailsBll.LoadEntitiesNoTracking(s => s.SearchTime > start, s => s.SearchTime, false).ToList();
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

        [HttpPost, Authority]
        public ActionResult Delete(int id)
        {
            bool b = SearchDetailsBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }
    }
}