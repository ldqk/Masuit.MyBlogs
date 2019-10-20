using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
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
            RecurringJob.AddOrUpdate(() => CheckLinks(), "0 */5 * * *"); //每5h检查友链
            RecurringJob.AddOrUpdate(() => EverydayJob(), Cron.Daily(5), TimeZoneInfo.Local); //每天的任务
            RecurringJob.AddOrUpdate(() => EveryweekJob(), Cron.Weekly(DayOfWeek.Monday, 5), TimeZoneInfo.Local); //每周的任务
            RecurringJob.AddOrUpdate(() => EveryHourJob(), Cron.Hourly); //每小时的任务
            BackgroundJob.Enqueue(() => HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.StatisticsSearchKeywords), "default"));
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
        /// 每小时任务
        /// </summary>
        public static void EveryHourJob()
        {
            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.StatisticsSearchKeywords), "default");
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