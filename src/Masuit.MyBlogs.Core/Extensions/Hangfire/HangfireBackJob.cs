using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Strings;
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
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ISearchEngine<DataContext> _searchEngine;
        private readonly IAdvertisementService _advertisementService;

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
        public HangfireBackJob(IUserInfoService userInfoService, IPostService postService, ISystemSettingService settingService, ISearchDetailsService searchDetailsService, ILinksService linksService, IHttpClientFactory httpClientFactory, IWebHostEnvironment HostEnvironment, ISearchEngine<DataContext> searchEngine, IAdvertisementService advertisementService)
        {
            _userInfoService = userInfoService;
            _postService = postService;
            _settingService = settingService;
            _searchDetailsService = searchDetailsService;
            _linksService = linksService;
            _httpClientFactory = httpClientFactory;
            _hostEnvironment = HostEnvironment;
            _searchEngine = searchEngine;
            _advertisementService = advertisementService;
        }

        /// <summary>
        /// 登录记录
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="ip"></param>
        /// <param name="type"></param>
        public void LoginRecord(UserInfoDto userInfo, string ip, LoginType type)
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
            var content = new Template(File.ReadAllText(Path.Combine(_hostEnvironment.WebRootPath, "template", "login.html")))
                .Set("name", u.Username)
                .Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Set("ip", record.IP)
                .Set("address", record.PhysicAddress).Render();
            CommonHelper.SendMail(_settingService.Get(s => s.Name.Equals("Title")).Value + "账号登录通知", content, _settingService.Get(s => s.Name.Equals("ReceiveEmail")).Value, "127.0.0.1");
        }

        /// <summary>
        /// 文章定时发布
        /// </summary>
        /// <param name="p"></param>
        public void PublishPost(Post p)
        {
            p.Status = Status.Published;
            p.PostDate = DateTime.Now;
            p.ModifyDate = DateTime.Now;
            var post = _postService.GetById(p.Id);
            if (post is null)
            {
                _postService.AddEntitySaved(p);
            }
            else
            {
                post.Status = Status.Published;
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
            s.Address = result?.Status == 0 ? result.AddressResult.FormattedAddress : s.IP.GetIPLocation();
            RedisHelper.LPush("intercept", s);
        }

        /// <summary>
        /// 每天的任务
        /// </summary>
        public void EverydayJob()
        {
            CommonHelper.IPErrorTimes.RemoveWhere(kv => kv.Value < 100); //将访客访问出错次数少于100的移开
            DateTime time = DateTime.Now.AddMonths(-1);
            _searchDetailsService.DeleteEntitySaved(s => s.SearchTime < time);
            TrackData.DumpLog();
        }

        /// <summary>
        /// 每月的任务
        /// </summary>
        public void EverymonthJob()
        {
            _advertisementService.GetAll().UpdateFromQuery(a => new Advertisement()
            {
                DisplayCount = 0,
                ViewCount = 0
            });
        }

        /// <summary>
        /// 检查友链
        /// </summary>
        public void CheckLinks()
        {
            var links = _linksService.GetQuery(l => !l.Except).AsParallel();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("MasuitBot-link/1.0"));
            client.DefaultRequestHeaders.Referrer = new Uri("https://masuit.com");
            client.Timeout = TimeSpan.FromSeconds(10);
            Parallel.ForEach(links, link =>
            {
                var prev = link.Status;
                client.GetStringAsync(link.Url).ContinueWith(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        link.Status = Status.Unavailable;
                    }
                    else
                    {
                        link.Status = !t.Result.Contains(CommonHelper.SystemSettings["Domain"]) ? Status.Unavailable : Status.Available;
                    }

                    if (link.Status != prev)
                    {
                        link.UpdateTime = DateTime.Now;
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
            _searchEngine.LuceneIndexer.DeleteAll();
            _searchEngine.CreateIndex(new List<string>()
            {
                nameof(DataContext.Post),
            });
            var list = _searchEngine.Context.Post.Where(i => i.Status != Status.Published).ToList();
            _searchEngine.LuceneIndexer.Delete(list);
        }

        /// <summary>
        /// 搜索统计
        /// </summary>
        public void StatisticsSearchKeywords()
        {
            RedisHelper.Set("SearchRank:Month", _searchDetailsService.GetRanks(DateTime.Today.AddMonths(-1)));
            RedisHelper.Set("SearchRank:Week", _searchDetailsService.GetRanks(DateTime.Today.AddDays(-7)));
            RedisHelper.Set("SearchRank:Today", _searchDetailsService.GetRanks(DateTime.Today));
        }
    }
}