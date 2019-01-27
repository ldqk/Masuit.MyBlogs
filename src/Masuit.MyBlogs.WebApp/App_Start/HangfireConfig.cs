using Common;
using Hangfire;
using Masuit.Tools;
using Masuit.Tools.NoSQL;
using Masuit.Tools.Win32;
using Models.Application;
using Models.Enum;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.WebApp
{
    /// <summary>
    /// hangfire配置
    /// </summary>
    public class HangfireConfig
    {
        public static void Register()
        {
            GlobalConfiguration.Configuration.UseRedisStorage();
            GlobalConfiguration.Configuration.UseAutofacActivator(AutofacConfig.Container);
            Server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                ServerName = $"{Environment.MachineName}", //服务器名称
                SchedulePollingInterval = TimeSpan.FromSeconds(1),
                ServerCheckInterval = TimeSpan.FromSeconds(1),
                WorkerCount = Environment.ProcessorCount * 2,
                //Queues = new[] { "masuit" } //队列名
            });

            //AggregateInterviews(); //访客统计
            RecurringJob.AddOrUpdate(() => Windows.ClearMemorySilent(), Cron.Hourly); //每小时清理系统内存
            RecurringJob.AddOrUpdate(() => CheckLinks(), Cron.HourInterval(5)); //每5h检查友链
            RecurringJob.AddOrUpdate(() => EverydayJob(), Cron.Daily, TimeZoneInfo.Local); //每天的任务
            //RecurringJob.AddOrUpdate(() => AggregateInterviews(), Cron.Hourly(30)); //每半小时统计访客
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                if (!redisHelper.KeyExists("ArticleViewToken"))
                {
                    redisHelper.SetString("ArticleViewToken", string.Empty.CreateShortToken()); //更新加密文章的密码
                }
            }
        }

        public static BackgroundJobServer Server { get; set; }

        /// <summary>
        /// 每天的任务
        /// </summary>
        public static void EverydayJob()
        {
            CommonHelper.IPErrorTimes.RemoveWhere(kv => kv.Value < 100); //将访客访问出错次数少于100的移开
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                redisHelper.SetString("ArticleViewToken", string.Empty.CreateShortToken()); //更新加密文章的密码
                redisHelper.StringIncrement("Interview:RunningDays");
            }
            using (DataContext db = new DataContext())
            {
                DateTime time = DateTime.Now.AddMonths(-2);
                db.SearchDetails.Where(s => s.SearchTime < time).DeleteFromQuery();
            }
        }

        /// <summary>
        /// 检查友链
        /// </summary>
        public static void CheckLinks()
        {
            using (DataContext db = new DataContext())
            {
                var links = db.Links.Where(l => !l.Except).AsParallel();
                Parallel.ForEach(links, link =>
                {
                    Uri uri = new Uri(link.Url);
                    using (HttpClient client = new HttpClient())
                    {
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
                                link.Status = !(await res.Content.ReadAsStringAsync()).Contains(CommonHelper.GetSettings("Domain")) ? Status.Unavailable : Status.Available;
                            }
                            else
                            {
                                link.Status = Status.Unavailable;
                            }
                            db.Entry(link).State = EntityState.Modified;
                        }).Wait();
                    }
                });
                db.SaveChanges();
            }
        }
    }
}