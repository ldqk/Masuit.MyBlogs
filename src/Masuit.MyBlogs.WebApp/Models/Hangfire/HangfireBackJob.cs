using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using IBLL;
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Masuit.Tools.Net;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Models.ViewModel;

namespace Masuit.MyBlogs.WebApp.Models.Hangfire
{
    public class HangfireBackJob : IHangfireBackJob
    {
        public IInterviewBll InterviewBll { get; set; }
        public ISystemSettingBll SystemSettingBll { get; set; }
        public IUserInfoBll UserInfoBll { get; set; }
        public IPostBll PostBll { get; set; }
        public HangfireBackJob(IInterviewBll bll, ISystemSettingBll systemSettingBll, IUserInfoBll userInfoBll, IPostBll postBll)
        {
            InterviewBll = bll;
            SystemSettingBll = systemSettingBll;
            UserInfoBll = userInfoBll;
            PostBll = postBll;
        }
        public void FlushInetAddress(Interview interview)
        {
            PhysicsAddress address = interview.IP.GetPhysicsAddressInfo().Result;
            if (address?.Status == 0)
            {
                interview.Address = $"{address.AddressResult.FormattedAddress} {address.AddressResult.AddressComponent.Direction}{address.AddressResult.AddressComponent.Distance ?? "0"}米";
                interview.Country = address.AddressResult.AddressComponent.Country;
                interview.Province = address.AddressResult.AddressComponent.Province;
                IList<string> strs = new List<string>();
                address.AddressResult?.Pois?.ForEach(s => strs.Add($"{s.AddressDetail} {s.Direction}{s.Distance ?? "0"}米"));
                if (strs.Any())
                {
                    interview.ReferenceAddress = string.Join("|", strs);
                }
            }
            interview.ISP = interview.IP.GetISP();
            Interview i = InterviewBll.AddEntitySaved(interview);
            CommonHelper.InterviewCount = InterviewBll.GetAll().Count();//记录访问量
        }

        public void FlushUnhandledAddress()
        {
            var list = InterviewBll.LoadEntities(i => string.IsNullOrEmpty(i.Address)).AsEnumerable();
            list.ForEach(i =>
            {
                PhysicsAddress addr = i.IP.GetPhysicsAddressInfo().Result;
                if (addr?.Status == 0)
                {
                    i.Address = $"{addr.AddressResult.FormattedAddress} {addr.AddressResult.AddressComponent.Direction}{addr.AddressResult.AddressComponent.Distance}米";
                    i.Province = addr.AddressResult.AddressComponent.Province;
                    IList<string> strs = new List<string>();
                    addr.AddressResult.Pois.ForEach(s => strs.Add($"{s.AddressDetail} {s.Direction}{s.Distance}米"));
                    i.ReferenceAddress = string.Join("|", strs);
                }
                i.ISP = i.IP.GetISP();
                InterviewBll.UpdateEntitySaved(i);
            });
            InterviewBll.DeleteEntitySaved(i => i.IP.Contains(":") || i.IP.Equals("127.0.0.1"));
        }

        public void UpdateLucene()
        {
            LuceneHelper.CreateIndex(PostBll.LoadEntitiesNoTracking(p => p.Status == Status.Pended));
            LuceneHelper.IncreaseIndex();
        }

        public void ResetLucene()
        {
            LuceneHelper.DeleteIndex();
            UpdateLucene();
        }

        public void LoginRecord(UserInfoOutputDto userInfo, string ip, LoginType type)
        {
            Interview view = InterviewBll.GetFirstEntityFromL2CacheNoTracking(i => i.IP.Equals(ip), i => i.ViewTime, false);
            string addr = view.Address;
            string prov = view.Province;
            LoginRecord record = new LoginRecord() { IP = ip, LoginTime = DateTime.Now, LoginType = type, PhysicAddress = addr, Province = prov };
            UserInfo u = UserInfoBll.GetByUsername(userInfo.Username);
            u.LoginRecord.Add(record);
            UserInfoBll.UpdateEntitySaved(u);
            string content = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "template\\login.html").Replace("{{name}}", u.Username).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{ip}}", record.IP).Replace("{{address}}", record.PhysicAddress);
            CommonHelper.SendMail(CommonHelper.GetSettings("Title") + "账号登录通知", content, CommonHelper.GetSettings("ReceiveEmail"));
        }

        public void PublishPost(Post p)
        {
            p.Status = Status.Pended;
            p.PostDate = DateTime.Now;
            p.ModifyDate = DateTime.Now;
            PostBll.AddOrUpdateSaved(e => e.Id, p);
            UpdateLucene();
        }

        public void AggregateInterviews()
        {
            AggregatedCounter.TotalInterviews = Analysis();
            AggregatedCounter.UniqueInterviews = Analysis(true);
        }

        public void InterviewTrace(Guid uid, string url)
        {
            for (int j = 0; j < 10; j++)//重试10次，找到这个访客
            {
                var view = InterviewBll.GetFirstEntity(i => i.Uid.Equals(uid));
                if (view != null)
                {
                    view.InterviewDetails.Add(new InterviewDetail()
                    {
                        Time = DateTime.Now,
                        Url = url
                    });
                    if (view.InterviewDetails.Count >= 2)
                    {
                        TimeSpan ts = DateTime.Now - view.InterviewDetails.FirstOrDefault().Time;
                        string len = string.Empty;
                        if (ts.Hours > 0)
                        {
                            len += $"{ts.Hours}小时";
                        }

                        if (ts.Minutes > 0)
                        {
                            len += $"{ts.Minutes}分钟";
                        }
                        len += $"{ts.Seconds}.{ts.Milliseconds}秒";
                        view.OnlineSpan = len;
                        view.OnlineSpanSeconds = ts.TotalSeconds;
                    }
                    InterviewBll.UpdateEntitySaved(view);
                    break;
                }
                Thread.Sleep(1000);
            }
        }
        public void FlushException(Exception ex)
        {
            LogManager.Error(ex);
        }

        private AnalysisModel Analysis(bool uniq = false)
        {
            var all = InterviewBll.SqlQuery<InterviewAnalysisDto>("select ip,BrowserType,Province,ViewTime,OnlineSpanSeconds from Interview ORDER BY ViewTime desc").ToList();
            var unique = InterviewBll.SqlQuery<InterviewAnalysisDto>("select ip,BrowserType,Province,ViewTime,OnlineSpanSeconds from Interview where Id in(select max(Id) from Interview group by ip ) ORDER BY ViewTime desc").ToList();
            var entities = uniq ? unique : all;
            var dap = InterviewBll.SqlQuery<BounceRate>(@"DECLARE @a float DECLARE @b int
                                                                        SET @a=((SELECT count(1) from (SELECT interviewid FROM InterviewDetail GROUP BY interviewid HAVING count(1)=1) as t)*1.0)
                                                                        SET @b=(SELECT count(1) from (SELECT interviewid FROM InterviewDetail GROUP BY interviewid) as t)
                                                                        SET @b=case when @b=0 then 1 else @b end
                                                                        SELECT @a Dap,@b [All],@a/@b Result").FirstOrDefault();//计算跳出率
            var dapAgg = InterviewBll.SqlQuery<BounceRateAggregate>("SELECT CONVERT(datetime,t.tt) [Time],sum(CASE WHEN t.c=1 THEN 1 ELSE 0 END) as Dap,count(t.c) as [All],sum(CASE WHEN t.c=1 THEN 1 ELSE 0 END)*1.0/count(t.c) as Rate from (SELECT interviewid,convert(char(10),[Time],23) tt,count(InterviewId) c FROM [dbo].[InterviewDetail] GROUP BY convert(char(10),[Time],23),InterviewId) as t GROUP BY t.tt ORDER BY Time");//每日跳出率统计

            //今日统计
            var todaypv = all.Count(i => i.ViewTime >= DateTime.Today);
            var todayuv = unique.Count(i => i.ViewTime >= DateTime.Today);

            //本月统计
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var monthpv = all.Count(i => i.ViewTime >= monthStart);
            var monthuv = unique.Count(i => i.ViewTime >= monthStart);

            //YTD统计
            var yearStart = new DateTime(DateTime.Now.Year, 1, 1);
            var yearpv = all.Count(i => i.ViewTime >= yearStart);
            var yearuv = unique.Count(i => i.ViewTime >= yearStart);

            //完全统计
            var totalpv = all.Count;
            var totaluv = unique.Count;
            var client = new List<object>();
            var region = new List<object>();

            //省份和世界地区统计
            entities.GroupBy(i => i.Province).OrderBy(g => Guid.NewGuid()).ForEach(g =>
            {
                if (string.IsNullOrEmpty(g.Key))
                {
                    return;
                }
                string name;
                if (g.Key.Contains("新疆"))
                {
                    name = "新疆";
                }
                else if (g.Key.Contains("广西"))
                {
                    name = "广西";
                }
                else if (g.Key.Contains("西藏"))
                {
                    name = "西藏";
                }
                else if (g.Key.Contains("香港"))
                {
                    name = "香港";
                }
                else if (g.Key.Contains("澳门"))
                {
                    name = "澳门";
                }
                else if (g.Key.Contains("内蒙古"))
                {
                    name = "内蒙古";
                }
                else if (g.Key.Contains("宁夏"))
                {
                    name = "宁夏";
                }
                else if (g.Key.Contains("海南"))
                {
                    name = "海南";
                }
                else if (g.Key.Contains("台湾"))
                {
                    name = "台湾";
                }
                else if (g.Key.Contains("省"))
                {
                    name = g.Key.Replace("省", "");
                }
                else if (g.Key.Contains("市"))
                {
                    name = g.Key.Replace("市", "");
                }
                else if (g.Key.Contains("XX"))
                {
                    return;
                }
                else
                {
                    name = g.Key;
                }
                client.Add(new { name, value = g.Count() });
                region.Add(new dynamic[] { name, g.Count() });
            });
            var browser = entities.GroupBy(i => i.BrowserType).OrderBy(g => g.Key).Select(g => new object[] { g.Key, g.Count() }).ToList();
            var reduce = all.OrderBy(i => i.ViewTime).GroupBy(i => i.ViewTime.ToString("yyyy-MM-dd")).Select(g => new { g.Key, pv = g.Count(), uv = g.DistinctBy(i => i.IP).Count() }).ToList();//汇总统计

            //找出PV最高的一天
            var p = reduce.FirstOrDefault(e => e.pv == reduce.Max(a => a.pv));
            var highpv = new { date = p.Key, p.pv, p.uv };

            //找出UV最高的一天
            var u = reduce.FirstOrDefault(e => e.uv == reduce.Max(a => a.uv));
            var highuv = new { date = u.Key, u.pv, u.uv };

            //汇总统计
            var pv = reduce.Select(g => new List<object> { g.Key.ToDateTime().GetTotalMilliseconds(), g.pv }).ToList();//每日PV
            var uv = reduce.Select(g => new List<object> { g.Key.ToDateTime().GetTotalMilliseconds(), g.uv }).ToList();//每日UV
            var iv = all.OrderBy(i => i.ViewTime).DistinctBy(e => e.IP).GroupBy(i => i.ViewTime.ToString("yyyy-MM-dd")).Select(g => new List<object> { g.Key.ToDateTime().GetTotalMilliseconds(), g.Count() }).ToList();//每日新增独立访客

            //访问时长统计
            InterviewAnalysisDto maxSpanViewer = all.OrderByDescending(i => i.OnlineSpanSeconds).FirstOrDefault();//历史最久访客
            InterviewAnalysisDto maxSpanViewerToday = all.Where(i => DateTime.Today == i.ViewTime.Date).OrderByDescending(i => i.OnlineSpanSeconds).FirstOrDefault();//今日最久访客
            double average = 0;
            double average2 = 0;
            if (all.Any(i => DateTime.Today == i.ViewTime.Date && i.OnlineSpanSeconds > 0))
            {
                average = all.Where(i => i.OnlineSpanSeconds > 0).Average(i => i.OnlineSpanSeconds);
                average2 = all.Where(i => DateTime.Today == i.ViewTime.Date && i.OnlineSpanSeconds > 0).Average(i => i.OnlineSpanSeconds);
            }
            var averSpan = TimeSpan2String(TimeSpan.FromSeconds(average));//平均访问时长
            var averSpanToday = TimeSpan2String(TimeSpan.FromSeconds(average2));//今日访问时长

            BounceRateAggregate todayDap = dapAgg.LastOrDefault();
            return new AnalysisModel()
            {
                Browser = browser,
                Client = client,
                Highpv = highpv,
                Highuv = highuv,
                Iv = iv,
                Monthpv = monthpv,
                Monthuv = monthuv,
                Pv = pv,
                Region = region,
                Todaypv = todaypv,
                Todayuv = todayuv,
                Totalpv = totalpv,
                Totaluv = totaluv,
                Uv = uv,
                Yearpv = yearpv,
                Yearuv = yearuv,
                BounceRate = $"{dap?.Dap}/{dap?.All}({dap?.Result:P})",
                BounceRateAggregate = dapAgg.Select(a => new object[] { a.Time.GetTotalMilliseconds(), a.Rate * 100 }).ToList(),
                BounceRateToday = $"{todayDap?.Dap}/{todayDap?.All}({todayDap?.Rate:P})",
                OnlineSpanAggregate = new
                {
                    maxSpanViewerToday = new
                    {
                        maxSpanViewerToday?.IP,
                        maxSpanViewerToday?.BrowserType,
                        maxSpanViewerToday?.Province,
                        maxSpanViewerToday?.ViewTime,
                        OnlineSpanSeconds = TimeSpan2String(TimeSpan.FromSeconds(maxSpanViewerToday?.OnlineSpanSeconds ?? 0))
                    },
                    averSpanToday,
                    maxSpanViewer = new
                    {
                        maxSpanViewer?.IP,
                        maxSpanViewer?.BrowserType,
                        maxSpanViewer?.Province,
                        maxSpanViewer?.ViewTime,
                        OnlineSpanSeconds = TimeSpan2String(TimeSpan.FromSeconds(maxSpanViewer?.OnlineSpanSeconds ?? 0))
                    },
                    averSpan,
                }
            };
        }

        private static string TimeSpan2String(TimeSpan span)
        {
            string averSpan = String.Empty;
            if (span.Hours > 0)
            {
                averSpan += span.Hours + "小时";
            }

            if (span.Minutes > 0)
            {
                averSpan += span.Minutes + "分钟";
            }

            averSpan += span.Seconds + "秒";
            return averSpan;
        }
    }
}