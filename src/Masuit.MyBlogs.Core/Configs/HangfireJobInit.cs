using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools;
using Masuit.Tools.NoSQL;
using System;

namespace Masuit.MyBlogs.Core.Configs
{
    /// <summary>
    /// hangfire配置
    /// </summary>
    public class HangfireJobInit
    {
        public static void Start()
        {
            RecurringJob.AddOrUpdate(() => CheckLinks(), Cron.HourInterval(5)); //每5h检查友链
            RecurringJob.AddOrUpdate(() => EverydayJob(), Cron.Daily, TimeZoneInfo.Local); //每天的任务
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                if (!redisHelper.KeyExists("ArticleViewToken"))
                {
                    redisHelper.SetString("ArticleViewToken", string.Empty.CreateShortToken()); //更新加密文章的密码
                }
            }
        }

        public static void CheckLinks()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.CheckLinks), "default");
        }

        public static void EverydayJob()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.EverydayJob), "default");
        }
    }
}