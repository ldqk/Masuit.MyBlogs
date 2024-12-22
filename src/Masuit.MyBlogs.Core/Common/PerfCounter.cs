using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Hardware;
using Masuit.Tools.Logging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;
using System.Net.Sockets;

namespace Masuit.MyBlogs.Core.Common;

public interface IPerfCounter
{
    public static ConcurrentLimitedQueue<PerformanceCounter> List { get; } = new(150000);
    public static Process CurrentProcess = System.Diagnostics.Process.GetCurrentProcess();
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
        var result = Task.WhenAll(Task.Run(() => SystemInfo.CpuLoad),
            Task.Run(CurrentProcess.GetProcessCpuUsage),
            Task.Run(() => CurrentProcess.GetProcessMemory()),
            Task.Run(() => (SystemInfo.PhysicalMemory - SystemInfo.MemoryAvailable) * 1f / 1024 / 1024),
            Task.Run(() => SystemInfo.GetDiskData(DiskData.Read) / 1024f),
            Task.Run(() => SystemInfo.GetDiskData(DiskData.Write) / 1024f),
            Task.Run(() => SystemInfo.GetNetData(NetData.Received) / 1024),
            Task.Run(() => SystemInfo.GetNetData(NetData.Sent) / 1024)).Result;
        var (load, processCpuUsage, processMemory, mem, read, write, up, down) = (result[0], result[1], result[2], result[3], result[4], result[5], result[6], result[7]);
        return new PerformanceCounter()
        {
            Time = time,
            CpuLoad = load,
            ProcessCpuLoad = processCpuUsage,
            MemoryUsage = mem,
            ProcessMemoryUsage = processMemory,
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

        var start = DateTime.Now.AddMonths(-2).GetTotalMilliseconds();
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

    public float ProcessCpuLoad { get; set; }

    /// <summary>
    /// 内存使用量(MB)
    /// </summary>
    public float MemoryUsage { get; set; }

    /// <summary>
    /// 进程内存使用量(MB)
    /// </summary>
    public float ProcessMemoryUsage { get; set; }

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