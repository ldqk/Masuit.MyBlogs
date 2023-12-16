using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Hardware;
using Masuit.Tools.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;

namespace Masuit.MyBlogs.Core.Common;

public interface IPerfCounter
{
    public static ConcurrentLimitedQueue<PerformanceCounter> List { get; } = new(50000);

    public static readonly DateTime StartTime = DateTime.Now;

    public static void Init()
    {
        Task.Run(() =>
        {
            int errorCount = 0;
            while (true)
            {
                try
                {
                    List.Enqueue(GetCurrentPerformanceCounter());
                }
                catch (Exception e)
                {
                    if (errorCount > 20)
                    {
                        LogManager.Error(e);
                        break;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = ConsoleColor.White;
                    errorCount++;
                }
                Thread.Sleep(5000);
            }
        });
    }

    public static PerformanceCounter GetCurrentPerformanceCounter()
    {
        var time = DateTime.Now.GetTotalMilliseconds();
        var load = SystemInfo.CpuLoad;
        var mem = (1 - SystemInfo.MemoryAvailable.ConvertTo<float>() / SystemInfo.PhysicalMemory.ConvertTo<float>()) * 100;

        var read = SystemInfo.GetDiskData(DiskData.Read) / 1024f;
        var write = SystemInfo.GetDiskData(DiskData.Write) / 1024;

        var up = SystemInfo.GetNetData(NetData.Received) / 1024;
        var down = SystemInfo.GetNetData(NetData.Sent) / 1024;
        return new PerformanceCounter()
        {
            Time = time,
            CpuLoad = load,
            MemoryUsage = mem,
            DiskRead = read,
            DiskWrite = write,
            Download = down,
            Upload = up,
            ServerIP = SystemInfo.GetLocalUsedIP(AddressFamily.InterNetwork).ToString()
        };
    }

    IQueryable<PerformanceCounter> CreateDataSource();

    void Process();
}

public sealed class DefaultPerfCounter : IPerfCounter
{
    static DefaultPerfCounter()
    {
    }

    public IQueryable<PerformanceCounter> CreateDataSource()
    {
        return IPerfCounter.List.AsQueryable();
    }

    public void Process()
    {
    }
}

public sealed class PerfCounterInDatabase(LoggerDbContext dbContext) : IPerfCounter
{
    public static ConcurrentLimitedQueue<PerformanceCounter> List { get; } = new(50000);

    public IQueryable<PerformanceCounter> CreateDataSource()
    {
        return dbContext.Set<PerformanceCounter>();
    }

    public void Process()
    {
        if (Debugger.IsAttached)
        {
            return;
        }

        while (IPerfCounter.List.TryDequeue(out var result))
        {
            dbContext.Add(result);
        }

        var start = DateTime.Now.AddMonths(-1).GetTotalMilliseconds();
        dbContext.Set<PerformanceCounter>().Where(e => e.Time < start).ExecuteDelete();
        dbContext.SaveChanges();
    }
}

public sealed class PerfCounterBackService(IServiceScopeFactory serviceScopeFactory) : ScheduledService(TimeSpan.FromSeconds(5))
{
    protected override Task ExecuteAsync()
    {
#if RELEASE
        using var scope = serviceScopeFactory.CreateAsyncScope();
        var counter = scope.ServiceProvider.GetRequiredService<IPerfCounter>();
        counter.Process();
#endif
        return Task.CompletedTask;
    }
}

public static class PerfCounterServiceExtension
{
    public static IServiceCollection AddPerfCounterManager(this IServiceCollection services, IConfiguration configuration)
    {
        IPerfCounter.Init();
        switch (configuration["PerfCounterStorage"])
        {
            case "database":
                services.AddScoped<IPerfCounter, PerfCounterInDatabase>();
                services.TryAddScoped<PerfCounterInDatabase>();
                break;

            default:
                services.AddSingleton<IPerfCounter, DefaultPerfCounter>();
                break;
        }

        services.Configure<HostOptions>(options => options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);
        services.AddHostedService<PerfCounterBackService>();
        return services;
    }
}

/// <summary>
/// 性能计数器
/// </summary>
[Table(nameof(PerformanceCounter))]
public sealed class PerformanceCounter
{
    [StringLength(128)]
    public string ServerIP { get; set; }

    /// <summary>
    /// 当前时间戳
    /// </summary>
    public long Time { get; set; }

    /// <summary>
    /// CPU当前负载
    /// </summary>
    public float CpuLoad { get; set; }

    /// <summary>
    /// 内存使用率
    /// </summary>
    public float MemoryUsage { get; set; }

    /// <summary>
    /// 磁盘读
    /// </summary>
    public float DiskRead { get; set; }

    /// <summary>
    /// 磁盘写
    /// </summary>
    public float DiskWrite { get; set; }

    /// <summary>
    /// 网络上行
    /// </summary>
    public float Upload { get; set; }

    /// <summary>
    /// 网络下行
    /// </summary>
    public float Download { get; set; }
}
