using IP2Region;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire
{
    /// <summary>
    /// hangfire后台任务
    /// </summary>
    public class HangfireBackJob : IHangfireBackJob
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IPostService _postService;
        private readonly ISystemSettingService _settingService;
        private readonly ISearchDetailsService _searchDetailsService;
        private readonly ILinksService _linksService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _HostEnvironment;
        private readonly ISearchEngine<DataContext> _searchEngine;

        /// <summary>
        /// hangfire后台任务
        /// </summary>
        /// <param name="userInfoService"></param>
        /// <param name="postService"></param>
        /// <param name="settingService"></param>
        /// <param name="searchDetailsService"></param>
        /// <param name="linksService"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="HostEnvironment"></param>
        /// <param name="searchEngine"></param>
        public HangfireBackJob(IUserInfoService userInfoService, IPostService postService, ISystemSettingService settingService, ISearchDetailsService searchDetailsService, ILinksService linksService, IHttpClientFactory httpClientFactory, IWebHostEnvironment HostEnvironment, ISearchEngine<DataContext> searchEngine)
        {
            _userInfoService = userInfoService;
            _postService = postService;
            _settingService = settingService;
            _searchDetailsService = searchDetailsService;
            _linksService = linksService;
            _httpClientFactory = httpClientFactory;
            _HostEnvironment = HostEnvironment;
            _searchEngine = searchEngine;
        }

        /// <summary>
        /// 登录记录
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="ip"></param>
        /// <param name="type"></param>
        public void LoginRecord(UserInfoOutputDto userInfo, string ip, LoginType type)
        {
            var result = ip.GetPhysicsAddressInfo().Result;
            if (result?.Status != 0)
            {
                return;
            }

            string addr = result.AddressResult.FormattedAddress;
            string prov = result.AddressResult.AddressComponent.Province;
            var record = new LoginRecord()
            {
                IP = ip,
                LoginTime = DateTime.Now,
                LoginType = type,
                PhysicAddress = addr,
                Province = prov
            };
            var u = _userInfoService.GetByUsername(userInfo.Username);
            u.LoginRecord.Add(record);
            _userInfoService.SaveChanges();
            var content = File.ReadAllText(Path.Combine(_HostEnvironment.WebRootPath, "template", "login.html"))
                .Replace("{{name}}", u.Username)
                .Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{{ip}}", record.IP)
                .Replace("{{address}}", record.PhysicAddress);
            CommonHelper.SendMail(_settingService.Get(s => s.Name.Equals("Title")).Value + "账号登录通知", content, _settingService.Get(s => s.Name.Equals("ReceiveEmail")).Value);
        }

        /// <summary>
        /// 文章定时发布
        /// </summary>
        /// <param name="p"></param>
        public void PublishPost(Post p)
        {
            p.Status = Status.Pended;
            p.PostDate = DateTime.Now;
            p.ModifyDate = DateTime.Now;
            var post = _postService.GetById(p.Id);
            if (post is null)
            {
                _postService.AddEntitySaved(p);
            }
            else
            {
                post.Status = Status.Pended;
                post.PostDate = DateTime.Now;
                post.ModifyDate = DateTime.Now;
                _postService.SaveChanges();
            }
        }

        /// <summary>
        /// 文章访问记录
        /// </summary>
        /// <param name="pid"></param>
        public void RecordPostVisit(int pid)
        {
            var post = _postService.GetById(pid);
            if (post == null)
            {
                return;
            }

            post.TotalViewCount += 1;
            post.AverageViewCount = post.TotalViewCount / (DateTime.Now - post.PostDate).TotalDays;
            _postService.SaveChanges();
        }

        /// <summary>
        /// 防火墙拦截日志
        /// </summary>
        /// <param name="s"></param>
        public static void InterceptLog(IpIntercepter s)
        {
            RedisHelper.IncrBy("interceptCount");
            var result = s.IP.GetPhysicsAddressInfo().Result;
            if (result.Status == 0)
            {
                s.Address = result.AddressResult.FormattedAddress;
            }
            else
            {
                using (DbSearcher searcher = new DbSearcher(Path.Combine(AppContext.BaseDirectory + "App_Data", "ip2region.db")))
                {
                    s.Address = searcher.MemorySearch(s.IP).Region;
                }
            }
            RedisHelper.LPush("intercept", s);
        }

        /// <summary>
        /// 每天的任务
        /// </summary>
        public void EverydayJob()
        {
            CommonHelper.IPErrorTimes.RemoveWhere(kv => kv.Value < 100); //将访客访问出错次数少于100的移开
            RedisHelper.IncrBy("Interview:RunningDays");
            DateTime time = DateTime.Now.AddMonths(-1);
            _searchDetailsService.DeleteEntitySaved(s => s.SearchTime < time);
        }

        /// <summary>
        /// 检查友链
        /// </summary>
        public void CheckLinks()
        {
            var links = _linksService.GetQuery(l => !l.Except).AsParallel();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
            client.DefaultRequestHeaders.Referrer = new Uri("https://masuit.com");
            client.Timeout = TimeSpan.FromSeconds(10);
            Parallel.ForEach(links, link =>
            {
                client.GetAsync(link.Url).ContinueWith(async t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        link.Status = Status.Unavailable;
                        return;
                    }

                    var res = await t;
                    if (res.IsSuccessStatusCode)
                    {
                        link.Status = !(await res.Content.ReadAsStringAsync()).Contains(CommonHelper.SystemSettings["Domain"]) ? Status.Unavailable : Status.Available;
                    }
                    else
                    {
                        link.Status = Status.Unavailable;
                    }
                }).Wait();
            });
            _linksService.SaveChanges();
        }

        /// <summary>
        /// 更新友链权重
        /// </summary>
        /// <param name="referer"></param>
        public void UpdateLinkWeight(string referer)
        {
            var uri = new Uri(referer);
            var query = _linksService.GetQuery(l => l.Url.Contains(uri.Host));
            if (query.Any())
            {
                var list = query.ToList();
                foreach (var link in list)
                {
                    link.Weight += 1;
                }

                _linksService.SaveChanges();
            }
        }

        /// <summary>
        /// 重建Lucene索引库
        /// </summary>
        public void CreateLuceneIndex()
        {
            _searchEngine.CreateIndex(new List<string>()
            {
                nameof(DataContext.Post),
            });
            var list = _searchEngine.Context.Post.Where(i => i.Status != Status.Pended).ToList();
            _searchEngine.LuceneIndexer.Delete(list);
        }

        /// <summary>
        /// 搜索统计
        /// </summary>
        public void StatisticsSearchKeywords()
        {
            var start = DateTime.Today.AddMonths(-1);
            var temp = _searchDetailsService.GetQueryNoTracking(s => s.SearchTime > start, s => s.SearchTime, false).ToList();
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
            RedisHelper.Set("SearchRank:Month", month);
            RedisHelper.Set("SearchRank:Week", week);
            RedisHelper.Set("SearchRank:Today", today);
        }
    }
}