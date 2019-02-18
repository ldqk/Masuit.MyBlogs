using Common;
using Masuit.LuceneEFCore.SearchEngine;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Application;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Core.NoSQL;
using Masuit.Tools.NoSQL;
using Masuit.Tools.Systems;
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
        private readonly RedisHelper _redisHelper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISearchEngine<DataContext> _searchEngine;

        /// <summary>
        /// hangfire后台任务
        /// </summary>
        /// <param name="userInfoService"></param>
        /// <param name="postService"></param>
        /// <param name="settingService"></param>
        /// <param name="searchDetailsService"></param>
        /// <param name="linksService"></param>
        /// <param name="redis"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="searchEngine"></param>
        public HangfireBackJob(IUserInfoService userInfoService, IPostService postService, ISystemSettingService settingService, ISearchDetailsService searchDetailsService, ILinksService linksService, RedisHelperFactory redis, IHttpClientFactory httpClientFactory, IHostingEnvironment hostingEnvironment, ISearchEngine<DataContext> searchEngine)
        {
            _userInfoService = userInfoService;
            _postService = postService;
            _settingService = settingService;
            _searchDetailsService = searchDetailsService;
            _linksService = linksService;
            _redisHelper = redis.CreateDefault();
            _httpClientFactory = httpClientFactory;
            _hostingEnvironment = hostingEnvironment;
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
            if (result?.Status == 0)
            {
                string addr = result.AddressResult.FormattedAddress;
                string prov = result.AddressResult.AddressComponent.Province;
                LoginRecord record = new LoginRecord()
                {
                    IP = ip,
                    LoginTime = DateTime.Now,
                    LoginType = type,
                    PhysicAddress = addr,
                    Province = prov
                };
                UserInfo u = _userInfoService.GetByUsername(userInfo.Username);
                u.LoginRecord.Add(record);
                _userInfoService.UpdateEntitySaved(u);
                string content = File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "template", "login.html")).Replace("{{name}}", u.Username).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{ip}}", record.IP).Replace("{{address}}", record.PhysicAddress);
                CommonHelper.SendMail(_settingService.GetFirstEntity(s => s.Name.Equals("Title")).Value + "账号登录通知", content, _settingService.GetFirstEntity(s => s.Name.Equals("ReceiveEmail")).Value);
            }
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
            Post post = _postService.GetById(p.Id);
            if (post is null)
            {
                _postService.AddEntitySaved(post);
            }
            else
            {
                post.Status = Status.Pended;
                post.PostDate = DateTime.Now;
                post.ModifyDate = DateTime.Now;
                _postService.UpdateEntitySaved(post);
            }
        }

        /// <summary>
        /// 文章访问记录
        /// </summary>
        /// <param name="pid"></param>
        public void RecordPostVisit(int pid)
        {
            Post post = _postService.GetById(pid);
            var record = post.PostAccessRecord.FirstOrDefault(r => r.AccessTime == DateTime.Today);
            if (record != null)
            {
                record.ClickCount += 1;
            }
            else
            {
                post.PostAccessRecord.Add(new PostAccessRecord
                {
                    ClickCount = 1,
                    AccessTime = DateTime.Today
                });
            }

            _postService.UpdateEntity(post);
            _postService.SaveChanges();
        }

        /// <summary>
        /// 防火墙拦截日志
        /// </summary>
        /// <param name="s"></param>
        public static void InterceptLog(IpIntercepter s)
        {
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                redisHelper.StringIncrement("interceptCount");
                redisHelper.ListLeftPush("intercept", s);
            }
        }

        /// <summary>
        /// 每天的任务
        /// </summary>
        public void EverydayJob()
        {
            CommonHelper.IPErrorTimes.RemoveWhere(kv => kv.Value < 100); //将访客访问出错次数少于100的移开
            _redisHelper.SetString("ArticleViewToken", SnowFlake.GetInstance().GetUniqueShortId(6)); //更新加密文章的密码
            _redisHelper.StringIncrement("Interview:RunningDays");
            DateTime time = DateTime.Now.AddMonths(-1);
            _searchDetailsService.DeleteEntitySaved(s => s.SearchTime < time);
            foreach (var p in _postService.GetAll().AsParallel())
            {
                try
                {
                    p.AverageViewCount = p.PostAccessRecord.Average(r => r.ClickCount);
                    p.TotalViewCount = p.PostAccessRecord.Sum(r => r.ClickCount);
                    _postService.UpdateEntity(p);
                    _postService.SaveChanges();
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// 检查友链
        /// </summary>
        public void CheckLinks()
        {
            var links = _linksService.LoadEntities(l => !l.Except).AsParallel();
            Parallel.ForEach(links, link =>
            {
                Uri uri = new Uri(link.Url);
                HttpClient client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                client.DefaultRequestHeaders.Referrer = new Uri("https://masuit.com");
                client.Timeout = TimeSpan.FromHours(10);
                client.GetAsync(uri).ContinueWith(async t =>
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
                    _linksService.UpdateEntity(link);
                }).Wait();
            });
            _linksService.SaveChanges();
        }

        /// <summary>
        /// 重建Lucene索引库
        /// </summary>
        public void CreateLuceneIndex()
        {
            _searchEngine.CreateIndex(new List<string>()
            {
                nameof(DataContext.Post),nameof(DataContext.Issues),
            });
            var list1 = _searchEngine.Context.Issues.Where(i => i.Status != Status.Handled && i.Level != BugLevel.Fatal).ToList();
            var list2 = _searchEngine.Context.Post.Where(i => i.Status != Status.Pended).ToList();
            var list = new List<LuceneIndexableBaseEntity>();
            list.AddRange(list1);
            list.AddRange(list2);
            _searchEngine.LuceneIndexer.Delete(list);
        }
    }
}