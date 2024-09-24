using System.Collections.Concurrent;
using EFCoreSecondLevelCacheInterceptor;
using FreeRedis;
using Masuit.MyBlogs.Core.Extensions.Firewall;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public class FirewallService(DataContext dataContext, IRedisClient redis) : IFirewallService
{
    private static readonly ConcurrentQueue<IpInterceptLog> Buffer = new();
    private DateTime _lastSave = DateTime.Now;

    public void AddIntercept(IpInterceptLog log)
    {
        Buffer.Enqueue(log);
        redis.IncrBy("interceptCount", 1);
        if (Buffer.Count >= 10 || DateTime.Now - _lastSave > TimeSpan.FromMinutes(10))
        {
            while (Buffer.TryDequeue(out var item))
            {
                dataContext.Add(item);
            }

            dataContext.SaveChanges();
            _lastSave = DateTime.Now;
        }
    }

    public async Task AddInterceptAsync(IpInterceptLog log)
    {
        Buffer.Enqueue(log);
        await redis.IncrByAsync("interceptCount", 1);
        if (Buffer.Count >= 10 || DateTime.Now - _lastSave > TimeSpan.FromMinutes(10))
        {
            while (Buffer.TryDequeue(out var item))
            {
                dataContext.Add(item);
            }

            await dataContext.SaveChangesAsync();
            _lastSave = DateTime.Now;
        }
    }

    public int InterceptCount(string ip)
    {
        return dataContext.IpInterceptLogs.Count(e => e.IP == ip) + Buffer.Count(e => e.IP == ip);
    }

    public int TotalCount()
    {
        return redis.Get<int>("interceptCount");
    }

    public List<IpInterceptLog> GetAll()
    {
        return Buffer.Union(dataContext.IpInterceptLogs.OrderByDescending(e => e.Time).Cacheable()).ToList();
    }

    public IQueryable<IpInterceptLog> Queryable()
    {
        return dataContext.IpInterceptLogs;
    }

    public bool Clear()
    {
        return dataContext.IpInterceptLogs.ExecuteDelete() > 0;
    }

    public bool Reported(string ip)
    {
        return dataContext.IpReportLogs.Any(e => e.IP == ip);
    }

    public Task<bool> ReportedAsync(string ip)
    {
        return dataContext.IpReportLogs.AnyAsync(e => e.IP == ip);
    }
}