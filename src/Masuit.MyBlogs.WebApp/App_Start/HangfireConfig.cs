using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Common;
using Hangfire;
using Hangfire.Console;
using Masuit.MyBlogs.WebApp.Models.Hangfire;
using Masuit.Tools;
using Masuit.Tools.NoSQL;
using Masuit.Tools.Win32;
using Models.Application;
using Models.Enum;

namespace Masuit.MyBlogs.WebApp
{
    /// <summary>
    /// hangfire配置
    /// </summary>
    public class HangfireConfig
    {
        public static void Register()
        {
            #region Hangfire配置

            //GlobalConfiguration.Configuration.UseMemoryStorage();
            GlobalConfiguration.Configuration.UseSqlServerStorage(ConfigurationManager.ConnectionStrings["DataContext"].ConnectionString).UseConsole();
            #region 实现类注册

            GlobalConfiguration.Configuration.UseAutofacActivator(AutofacConfig.Container);

            #endregion

            #region 服务启动

            Server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                ServerName = $"{Environment.MachineName}", //服务器名称
                SchedulePollingInterval = TimeSpan.FromSeconds(1),
                ServerCheckInterval = TimeSpan.FromSeconds(1),
                WorkerCount = Environment.ProcessorCount * 2,
                //Queues = new[] { "masuit" } //队列名
            });
            #endregion

            #endregion
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.UpdateLucene));//更新文章索引
            AggregateInterviews();//访客统计
            RecurringJob.AddOrUpdate(() => Windows.ClearMemorySilent(), Cron.Hourly);//每小时清理系统内存
            RecurringJob.AddOrUpdate(() => CheckLinks(), Cron.HourInterval(5));//每5h检查友链
            RecurringJob.AddOrUpdate(() => EverydayJob(), Cron.Daily, TimeZoneInfo.Local);//每天的任务
            RecurringJob.AddOrUpdate(() => FlushAddress(), Cron.Weekly(DayOfWeek.Sunday, 2), TimeZoneInfo.Local);//刷新没统计到的访客的信息
            RecurringJob.AddOrUpdate(() => AggregateInterviews(), Cron.Hourly(30));//每半小时统计访客
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                if (!redisHelper.KeyExists("ArticleViewToken"))
                {
                    redisHelper.SetString("ArticleViewToken", string.Empty.CreateShortToken());//更新加密文章的密码
                }
            }
        }

        public static BackgroundJobServer Server { get; set; }

        /// <summary>
        /// 刷新没统计到的访客的信息
        /// </summary>
        public static void FlushAddress()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.FlushUnhandledAddress));
        }

        /// <summary>
        /// 每天的任务
        /// </summary>
        public static void EverydayJob()
        {
            CommonHelper.IPErrorTimes.RemoveWhere(kv => kv.Value < 100);//将访客访问出错次数少于100的移开
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                redisHelper.SetString("ArticleViewToken", string.Empty.CreateShortToken());//更新加密文章的密码
            }
            using (DataContext db = new DataContext())
            {//清理hangfire数据库
                db.Database.ExecuteSqlCommand(@"DELETE FROM [HangFire].[Job] WHERE StateName='Succeeded' or StateName='Deleted';
                    TRUNCATE TABLE [HangFire].[Counter];
                    DELETE FROM [HangFire].[Hash] WHERE Field='Jobid';
                    DELETE FROM [HangFire].[AggregatedCounter] WHERE ExpireAt is not null;
                    UPDATE [HangFire].[AggregatedCounter] SET [Value] = (select count(1) from [HangFire].[Job] WHERE StateName<>'Succeeded' and StateName<>'Deleted')-1 WHERE [Key] = 'stats:succeeded'; 
                    UPDATE [HangFire].[AggregatedCounter] SET [Value] = 0 WHERE [Key] = 'stats:deleted'");
            }
        }

        public static void AggregateInterviews()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.AggregateInterviews));
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
                     using (HttpClient client = new HttpClient() { BaseAddress = uri })
                     {
                         client.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                         client.DefaultRequestHeaders.Referrer = new Uri("https://masuit.com");
                         client.Timeout = TimeSpan.FromHours(10);
                         client.GetAsync(uri.PathAndQuery).ContinueWith(async t =>
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