using System.Collections.Concurrent;
using System.Diagnostics;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public interface IRequestLogger
{
    void Log(string ip, string url, string userAgent);

    void Process();
}

public class RequestNoneLogger : IRequestLogger
{
    public void Log(string ip, string url, string userAgent)
    {
    }

    public void Process()
    {
    }
}

public class RequestFileLogger : IRequestLogger
{
    public void Log(string ip, string url, string userAgent)
    {
        TrackData.RequestLogs.AddOrUpdate(ip, new RequestLog()
        {
            Count = 1,
            RequestUrls = { url },
            UserAgents = { userAgent }
        }, (_, i) =>
        {
            i.UserAgents.Add(userAgent);
            i.RequestUrls.Add(url);
            i.Count++;
            return i;
        });
    }

    public void Process()
    {
    }
}

public class RequestDatabaseLogger : IRequestLogger
{
    private readonly LoggerDbContext _dataContext;
    private static readonly ConcurrentQueue<RequestLogDetail> Queue = new();

    public RequestDatabaseLogger(LoggerDbContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void Log(string ip, string url, string userAgent)
    {
        Queue.Enqueue(new RequestLogDetail
        {
            Time = DateTime.Now,
            UserAgent = userAgent,
            RequestUrl = url,
            IP = ip
        });
    }

    public void Process()
    {
        if (Debugger.IsAttached)
        {
            return;
        }

        while (Queue.TryDequeue(out var result))
        {
            var (location, network, info) = result.IP.GetIPLocation();
            result.Location = location;
            result.Network = network;
            _dataContext.Add(result);
        }

        if (_dataContext.SaveChanges() > 0)
        {
            var start = DateTime.Now.AddMonths(-6);
            var tableName = _dataContext.Model.FindEntityType(typeof(RequestLogDetail)).GetTableName();
            _dataContext.Database.ExecuteSqlRaw($"DELETE FROM \"{tableName}\" WHERE \"{nameof(RequestLogDetail.Time)}\" <'{start:yyyy-MM-dd HH:mm:ss}'");
        }
    }
}

public class RequestLoggerBackService : ScheduledService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RequestLoggerBackService(IServiceScopeFactory scopeFactory) : base(TimeSpan.FromMinutes(5))
    {
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync()
    {
        using var scope = _scopeFactory.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<IRequestLogger>();
        logger.Process();
        return Task.CompletedTask;
    }
}

public static class RequestLoggerServiceExtension
{
    public static IServiceCollection AddRequestLogger(this IServiceCollection services, IConfiguration configuration)
    {
        switch (configuration["RequestLogStorage"])
        {
            case "database":
                services.AddScoped<IRequestLogger, RequestDatabaseLogger>();
                services.TryAddScoped<RequestDatabaseLogger>();
                break;

            case "file":
                services.AddSingleton<IRequestLogger, RequestFileLogger>();
                break;

            default:
                services.AddSingleton<IRequestLogger, RequestNoneLogger>();
                break;
        }

        services.AddHostedService<RequestLoggerBackService>();
        return services;
    }
}
