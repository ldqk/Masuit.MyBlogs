using Hangfire;

namespace Masuit.MyBlogs.Core.Extensions.Hangfire;

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
        RecurringJob.AddOrUpdate<IHangfireBackJob>(nameof(IHangfireBackJob.CheckLinks), job => job.CheckLinks(), "0 */5 * * *"); //每5h检查友链
        RecurringJob.AddOrUpdate<IHangfireBackJob>("CheckAdvertisements", job => job.CheckAdvertisements(), Cron.Daily);
        RecurringJob.AddOrUpdate<IHangfireBackJob>("EverydayJob", job => job.EverydayJob(), Cron.Daily(5), new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.Local
        }); //每天的任务
        RecurringJob.AddOrUpdate<IHangfireBackJob>("CreateLuceneIndex", job => job.CreateLuceneIndex(), Cron.Weekly(DayOfWeek.Monday, 5), new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.Local
        }); //每周的任务
        RecurringJob.AddOrUpdate<IHangfireBackJob>("EverymonthJob", job => job.EverymonthJob(), Cron.Monthly(1, 0, 0)); //每月的任务
        RecurringJob.AddOrUpdate<IHangfireBackJob>("StatisticsSearchKeywords", job => job.StatisticsSearchKeywords(), Cron.Hourly); //每小时的任务
        BackgroundJob.Enqueue<IHangfireBackJob>(job => job.StatisticsSearchKeywords());
    }
}
