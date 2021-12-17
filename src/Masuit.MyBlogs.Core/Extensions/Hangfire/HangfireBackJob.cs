using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Strings;

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
        private readonly ILinkLoopbackService _loopbackService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ISearchEngine<DataContext> _searchEngine;
        private readonly IAdvertisementService _advertisementService;
        private readonly INoticeService _noticeService;
        private readonly IPostVisitRecordService _recordService;
        private readonly IPostVisitRecordStatsService _recordStatsService;

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
        public HangfireBackJob(IUserInfoService userInfoService, IPostService postService, ISystemSettingService settingService, ISearchDetailsService searchDetailsService, ILinksService linksService, IHttpClientFactory httpClientFactory, IWebHostEnvironment HostEnvironment, ISearchEngine<DataContext> searchEngine, IAdvertisementService advertisementService, INoticeService noticeService, ILinkLoopbackService loopbackService, IPostVisitRecordService recordService, IPostVisitRecordStatsService recordStatsService)
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
            _noticeService = noticeService;
            _loopbackService = loopbackService;
            _recordService = recordService;
            _recordStatsService = recordStatsService;
        }

        /// <summary>
        /// 登录记录
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="ip"></param>
        /// <param name="type"></param>
        public void LoginRecord(UserInfoDto userInfo, string ip, LoginType type)
        {
            var record = new LoginRecord()
            {
                IP = ip,
                LoginTime = DateTime.Now,
                LoginType = type,
                PhysicAddress = ip.GetIPLocation()
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
        /// <param name="ip"></param>
        /// <param name="refer"></param>
        /// <param name="url"></param>
        public void RecordPostVisit(int pid, string ip, string refer, string url)
        {
            var time = DateTime.Now.AddMonths(-3);
            _recordService.GetQuery(b => b.Time < time).DeleteFromQuery();
            _recordStatsService.GetQuery(b => b.Date < time).DeleteFromQuery();
            var post = _postService.GetById(pid);
            if (post == null)
            {
                return;
            }

            post.TotalViewCount += 1;
            post.AverageViewCount = _recordService.GetQuery(e => e.PostId == pid).GroupBy(r => r.Time.Date).Select(g => g.Count()).DefaultIfEmpty().Average();
            _recordService.AddEntity(new PostVisitRecord()
            {
                IP = ip,
                Referer = refer,
                Location = ip.GetIPLocation(),
                Time = DateTime.Now,
                RequestUrl = url,
                PostId = pid
            });
            var stats = _recordStatsService.Get(e => e.PostId == pid && e.Date >= DateTime.Today);
            if (stats != null)
            {
                stats.Count++;
            }
            else
            {
                _recordStatsService.AddEntity(new PostVisitRecordStats()
                {
                    Count = 1,
                    Date = DateTime.Today,
                    PostId = pid
                });
            }

            _postService.SaveChanges();
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
            _advertisementService.GetQuery(a => DateTime.Now >= a.ExpireTime).UpdateFromQuery(a => new Advertisement()
            {
                Status = Status.Unavailable
            });
            _noticeService.GetQuery(n => n.NoticeStatus == NoticeStatus.UnStart && n.StartTime < DateTime.Now).UpdateFromQuery(n => new Notice()
            {
                NoticeStatus = NoticeStatus.Normal,
                PostDate = DateTime.Now,
                ModifyDate = DateTime.Now
            });
            _noticeService.GetQuery(n => n.NoticeStatus == NoticeStatus.Normal && n.EndTime < DateTime.Now).UpdateFromQuery(n => new Notice()
            {
                NoticeStatus = NoticeStatus.Expired,
                ModifyDate = DateTime.Now
            });
        }

        /// <summary>
        /// 每月的任务
        /// </summary>
        public void EverymonthJob()
        {
            _advertisementService.GetAll().UpdateFromQuery(a => new Advertisement()
            {
                DisplayCount = 0
            });
        }

        /// <summary>
        /// 检查友链
        /// </summary>
        public void CheckLinks()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36 Edg/93.0.961.47");
            client.DefaultRequestHeaders.Add("X-Forwarded-For", "1.1.1.1");
            client.DefaultRequestHeaders.Add("X-Forwarded-Host", "1.1.1.1");
            client.DefaultRequestHeaders.Add("X-Real-IP", "1.1.1.1");
            client.DefaultRequestHeaders.Referrer = new Uri("https://google.com");
            client.Timeout = TimeSpan.FromSeconds(10);
            _linksService.GetQuery(l => !l.Except).AsParallel().ForAll(link =>
            {
                var prev = link.Status;
                client.GetStringAsync(link.Url, new CancellationTokenSource(client.Timeout).Token).ContinueWith(t =>
                {
                    if (t.IsCanceled || t.IsFaulted)
                    {
                        link.Status = Status.Unavailable;
                    }
                    else
                    {
                        link.Status = !t.Result.Contains(CommonHelper.SystemSettings["Domain"].Split("|")) ? Status.Unavailable : Status.Available;
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
        /// <param name="ip"></param>
        public void UpdateLinkWeight(string referer, string ip)
        {
            var list = _linksService.GetQuery(l => referer.Contains(l.UrlBase)).ToList();
            foreach (var link in list)
            {
                link.Loopbacks.Add(new LinkLoopback()
                {
                    IP = ip,
                    Referer = referer,
                    Time = DateTime.Now
                });
            }
            var time = DateTime.Now.AddMonths(-1);
            _loopbackService.GetQuery(b => b.Time < time).DeleteFromQuery();
            _linksService.SaveChanges();
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
            var list = _postService.GetQuery(i => i.Status != Status.Published).ToList();
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
