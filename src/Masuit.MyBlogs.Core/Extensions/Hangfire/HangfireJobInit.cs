using Hangfire;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire
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
            RecurringJob.AddOrUpdate<IHangfireBackJob>(job => job.CheckLinks(), "0 */5 * * *"); //每5h检查友链
            RecurringJob.AddOrUpdate<IHangfireBackJob>(job => job.EverydayJob(), Cron.Daily(5), TimeZoneInfo.Local); //每天的任务
            RecurringJob.AddOrUpdate<IHangfireBackJob>(job => job.CreateLuceneIndex(), Cron.Weekly(DayOfWeek.Monday, 5), TimeZoneInfo.Local); //每周的任务
            RecurringJob.AddOrUpdate<IHangfireBackJob>(job => job.EverymonthJob(), Cron.Monthly(1, 0, 0), TimeZoneInfo.Local); //每月的任务
            RecurringJob.AddOrUpdate<IHangfireBackJob>(job => job.StatisticsSearchKeywords(), Cron.Hourly); //每小时的任务
            BackgroundJob.Enqueue<IHangfireBackJob>(job => job.StatisticsSearchKeywords());
        }
    }
}
