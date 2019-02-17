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
    public static class HangfireJobInit
    {
        /// <summary>
        /// hangfire初始化
        /// </summary>
        public static void Start()
        {
            RecurringJob.AddOrUpdate(() => CheckLinks(), Cron.HourInterval(5)); //每5h检查友链
            RecurringJob.AddOrUpdate(() => EverydayJob(), Cron.Daily, TimeZoneInfo.Local); //每天的任务
            RecurringJob.AddOrUpdate(() => EveryweekJob(), Cron.Weekly(DayOfWeek.Monday, 3), TimeZoneInfo.Local); //每周的任务
            using (RedisHelper redisHelper = RedisHelper.GetInstance())
            {
                if (!redisHelper.KeyExists("ArticleViewToken"))
                {
                    redisHelper.SetString("ArticleViewToken", string.Empty.CreateShortToken()); //更新加密文章的密码
                }
            }
        }

        /// <summary>
        /// 检查友链
        /// </summary>
        public static void CheckLinks()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.CheckLinks), "default");
        }

        /// <summary>
        /// 每日任务
        /// </summary>
        public static void EverydayJob()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.EverydayJob), "default");
        }

        /// <summary>
        /// 每周任务
        /// </summary>
        public static void EveryweekJob()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.CreateLuceneIndex), "default");
        }
    }
}