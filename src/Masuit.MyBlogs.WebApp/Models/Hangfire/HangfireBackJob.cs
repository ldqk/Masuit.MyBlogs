using Common;
using IBLL;
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Masuit.Tools.Net;
using Masuit.Tools.NoSQL;
using Masuit.Tools.Win32;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Masuit.MyBlogs.WebApp.Models.Hangfire
{
    public class HangfireBackJob : IHangfireBackJob
    {
        //public IInterviewBll InterviewBll { get; set; }
        public ISystemSettingBll SystemSettingBll { get; set; }
        public IUserInfoBll UserInfoBll { get; set; }
        public IPostBll PostBll { get; set; }
        public RedisHelper RedisHelper { get; set; }

        public HangfireBackJob(ISystemSettingBll systemSettingBll, IUserInfoBll userInfoBll, IPostBll postBll, RedisHelper redisHelper)
        {
            //InterviewBll = bll;
            SystemSettingBll = systemSettingBll;
            UserInfoBll = userInfoBll;
            PostBll = postBll;
            RedisHelper = redisHelper;
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
                if ("true" == CommonHelper.GetSettings("EnableDenyArea"))
                {
                    CommonHelper.GetSettings("DenyArea")?.Split(',', '，').ForEach(area =>
                    {
                        if (interview.Address.Contains(area) || (interview.ReferenceAddress != null && interview.ReferenceAddress.Contains(area)))
                        {
                            CommonHelper.DenyAreaIP.AddOrUpdate(area, a => new HashSet<string>
                            {
                                interview.IP
                            }, (s, list) =>
                            {
                                lock (list)
                                {
                                    list.Add(interview.IP);
                                    return list;
                                }
                            });
                            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "denyareaip.txt"), CommonHelper.DenyAreaIP.ToJsonString());
                        }
                    });
                }
            }
            interview.ISP = interview.IP.GetISP();
            RedisHelper.ListRightPush($"Interview:{DateTime.Today:yyyy:MM:dd}", interview);
            //CommonHelper.InterviewCount = InterviewBll.GetAll().Count(); //记录访问量
            RedisHelper.StringIncrement("Interview:ViewCount");
        }

        //public void FlushUnhandledAddress()
        //{
        //    var list = InterviewBll.LoadEntities(i => string.IsNullOrEmpty(i.Address)).AsEnumerable();
        //    list.ForEach(i =>
        //    {
        //        PhysicsAddress addr = i.IP.GetPhysicsAddressInfo().Result;
        //        if (addr?.Status == 0)
        //        {
        //            i.Address = $"{addr.AddressResult.FormattedAddress} {addr.AddressResult.AddressComponent.Direction}{addr.AddressResult.AddressComponent.Distance}米";
        //            i.Province = addr.AddressResult.AddressComponent.Province;
        //            IList<string> strs = new List<string>();
        //            addr.AddressResult.Pois.ForEach(s => strs.Add($"{s.AddressDetail} {s.Direction}{s.Distance}米"));
        //            i.ReferenceAddress = string.Join("|", strs);
        //        }
        //        i.ISP = i.IP.GetISP();
        //        InterviewBll.UpdateEntitySaved(i);
        //    });
        //    InterviewBll.DeleteEntitySaved(i => i.IP.Contains(":") || i.IP.Equals("127.0.0.1"));
        //}

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
            var view = RedisHelper.ListRange<Interview>($"Interview:{DateTime.Today:yyyy:MM:dd}").OrderByDescending(i => i.ViewTime).FirstOrDefault(i => i.IP.Equals(ip));
            string addr = view?.Address;
            string prov = view?.Province;
            LoginRecord record = new LoginRecord()
            {
                IP = ip,
                LoginTime = DateTime.Now,
                LoginType = type,
                PhysicAddress = addr,
                Province = prov
            };
            UserInfo u = UserInfoBll.GetByUsername(userInfo.Username);
            u.LoginRecord.Add(record);
            UserInfoBll.UpdateEntitySaved(u);
            string content = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "template\\login.html").Replace("{{name}}", u.Username).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{ip}}", record.IP).Replace("{{address}}", record.PhysicAddress);
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
            var (all, unique) = Analysis();
            AggregatedCounter.TotalInterviews = all;
            AggregatedCounter.UniqueInterviews = unique;
            Windows.ClearMemorySilent();
        }

        public void InterviewTrace(Guid uid, string url)
        {
            for (int j = 0; j < 10; j++) //重试10次，找到这个访客
            {
                var view = RedisHelper.ListRange<Interview>($"Interview:{DateTime.Today:yyyy:MM:dd}").Union(RedisHelper.ListRange<Interview>($"Interview:{DateTime.Today.AddDays(-1):yyyy:MM:dd}")).FirstOrDefault(i => i.Uid.Equals(uid));
                if (view != null)
                {
                    RedisHelper.RemoveList($"Interview:{DateTime.Today:yyyy:MM:dd}", view);
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
                    RedisHelper.ListRightPush($"Interview:{DateTime.Today:yyyy:MM:dd}", view);
                    //InterviewBll.UpdateEntitySaved(view);
                    break;
                }
                Thread.Sleep(1000);
            }
        }

        public void FlushException(Exception ex)
        {
            LogManager.Error(ex);
        }

        private (AnalysisModel, AnalysisModel) Analysis()
        {
            List<Interview> list;
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                list = redisHelper.ListRange<Interview>($"Interview:{DateTime.Today:yyyy:MM:dd}");
                for (int i = -70; i < 0; i++)
                {
                    list.AddRange(redisHelper.ListRange<Interview>($"Interview:{DateTime.Today.AddDays(i):yyyy:MM:dd}"));
                }
                list = list.OrderBy(i => i.ViewTime).ToList();
            }
            if (!list.Any())
            {
                return (null, null);
            }
            var dap = list.Select(i =>
            {
                var (a, d) = (list.GroupBy(g => g.Uid).Count(), list.Count(g => g.InterviewDetails.Count == 1));
                return new BounceRate()
                {
                    All = a,
                    Dap = d,
                    Result = d * 1.0 / a
                };
            }).FirstOrDefault();
            var dapAgg = list.GroupBy(i => i.ViewTime.Date).Select(gs =>
            {
                var (a, d) = (gs.GroupBy(g => g.Uid).Count(), gs.Count(g => g.InterviewDetails.Count == 1));
                return new BounceRateAggregate()
                {
                    Time = gs.Key,
                    Dap = d,
                    All = a,
                    Rate = d * 1.0m / a
                };
            });
            int todaypv = list.Count(i => i.ViewTime >= DateTime.Today);
            int todayuv = list.DistinctBy(i => i.IP).Count(i => i.ViewTime >= DateTime.Today);

            //本月统计
            var monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            int monthpv = list.Count(i => i.ViewTime >= monthStart);
            int monthuv = list.DistinctBy(i => i.IP).Count(i => i.ViewTime >= monthStart);

            //YTD统计
            var yearStart = new DateTime(DateTime.Now.Year, 1, 1);
            int yearpv = list.Count(i => i.ViewTime >= yearStart);
            int yearuv = list.DistinctBy(i => i.IP).Count(i => i.ViewTime >= yearStart);

            //完全统计
            int totalpv = list.Count();
            int totaluv = list.DistinctBy(i => i.IP).Count();

            var allClient = new List<object>();
            var uniClient = new List<object>();
            var allRegion = new List<object>();
            var uniRegion = new List<object>();

            //省份和世界地区统计
            list.Where(i => !string.IsNullOrEmpty(i.Province)).GroupBy(i => i.Province).Select(gs => new KeyCount()
            {
                Key = gs.Key,
                Count = gs.Count(),
                UniqueCount = gs.DistinctBy(i => i.IP).Count()
            }).OrderBy(g => Guid.NewGuid()).ForEach(g =>
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
                allClient.Add(new
                {
                    name,
                    value = g.Count
                });
                uniClient.Add(new
                {
                    name,
                    value = g.UniqueCount
                });
                allRegion.Add(new dynamic[]
                {
                    name,
                    g.Count
                });
                uniRegion.Add(new dynamic[]
                {
                    name,
                    g.UniqueCount
                });
            });
            var allBrowser = list.GroupBy(i => i.BrowserType).OrderBy(g => g.Key).Select(g => new object[]
            {
                g.Key,
                g.Count()
            }).ToList();
            var uniBrowser = list.GroupBy(i => i.BrowserType).OrderBy(g => g.Key).Select(g => new object[]
            {
                g.Key,
                g.DistinctBy(i => i.IP).Count()
            }).ToList();
            Dictionary<DateTime, int> dic = list.DistinctBy(i => i.IP).GroupBy(g => g.ViewTime.Date).ToDictionary(g => g.Key, g => g.Count());
            var reduce = list.GroupBy(i => i.ViewTime.Date).Select(g =>
            {
                var count = g.DistinctBy(i => i.IP).Count();
                return new
                {
                    Date = g.Key,
                    TimeStamp = g.Key.GetTotalMilliseconds(),
                    pv = g.Count(),
                    uv = count,
                    iv = count - dic[g.Key]
                };
            }).ToList(); //汇总统计

            //找出PV最高的一天
            var p = reduce.FirstOrDefault(e => e.pv == reduce.Max(a => a.pv));
            var highpv = new
            {
                date = p.Date,
                p.pv,
                p.uv
            };

            //找出UV最高的一天
            var u = reduce.FirstOrDefault(e => e.uv == reduce.Max(a => a.uv));
            var highuv = new
            {
                date = u.Date,
                u.pv,
                u.uv
            };

            //汇总统计
            var pv = reduce.Select(g => new List<object>
            {
                g.TimeStamp,
                g.pv
            }).ToList(); //每日PV
            var uv = reduce.Select(g => new List<object>
            {
                g.TimeStamp,
                g.uv
            }).ToList(); //每日UV
            var iv = reduce.Select(g => new List<object>
            {
                g.TimeStamp,
                g.iv
            }).ToList();  //每日新增独立访客

            //访问时长统计
            InterviewAnalysisDto maxSpanViewer = list.OrderByDescending(i => i.OnlineSpanSeconds).Select(i => new InterviewAnalysisDto
            {
                ViewTime = i.ViewTime,
                IP = i.IP,
                BrowserType = i.BrowserType,
                Province = i.Province,
                OnlineSpanSeconds = i.OnlineSpanSeconds
            }).FirstOrDefault(); //历史最久访客
            InterviewAnalysisDto maxSpanViewerToday = list.Where(i => i.ViewTime >= DateTime.Today).OrderByDescending(i => i.OnlineSpanSeconds).Select(i => new InterviewAnalysisDto
            {
                ViewTime = i.ViewTime,
                IP = i.IP,
                BrowserType = i.BrowserType,
                Province = i.Province,
                OnlineSpanSeconds = i.OnlineSpanSeconds
            }).FirstOrDefault(); //今日最久访客

            double average = 0;
            double average2 = 0;
            if (list.Any(i => i.OnlineSpanSeconds > 0 && i.ViewTime >= DateTime.Today))
            {
                average = list.Where(i => i.OnlineSpanSeconds > 0).Average(i => i.OnlineSpanSeconds);
                average2 = list.Where(i => i.OnlineSpanSeconds > 0 && i.ViewTime >= DateTime.Today).Average(i => i.OnlineSpanSeconds);
            }
            var averSpan = TimeSpan2String(TimeSpan.FromSeconds(average)); //平均访问时长
            var averSpanToday = TimeSpan2String(TimeSpan.FromSeconds(average2)); //今日访问时长

            BounceRateAggregate todayDap = dapAgg.LastOrDefault();
            var all = new AnalysisModel()
            {
                Browser = allBrowser,
                Client = allClient,
                Region = allRegion,
                Highpv = highpv,
                Highuv = highuv,
                Iv = iv,
                Monthpv = monthpv,
                Monthuv = monthuv,
                Pv = pv,
                Todaypv = todaypv,
                Todayuv = todayuv,
                Totalpv = totalpv,
                Totaluv = totaluv,
                Uv = uv,
                Yearpv = yearpv,
                Yearuv = yearuv,
                BounceRate = $"{dap?.Dap}/{dap?.All}({dap?.Result:P})",
                BounceRateAggregate = dapAgg.Select(a => new object[]
                {
                    a.Time.GetTotalMilliseconds(),
                    a.Rate * 100
                }).ToList(),
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
            var unique = all.Copy();
            unique.Browser = uniBrowser;
            unique.Client = uniClient;
            unique.Region = uniRegion;
            return (all, unique);
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

        public static void InterceptLog(IpIntercepter s)
        {
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                redisHelper.StringIncrement("interceptCount");
                redisHelper.ListLeftPush("intercept", s);
            }
        }
    }
}