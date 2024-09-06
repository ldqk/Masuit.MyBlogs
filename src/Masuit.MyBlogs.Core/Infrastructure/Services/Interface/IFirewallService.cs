using Masuit.MyBlogs.Core.Extensions.Firewall;

namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface;

public interface IFirewallService
{
    void AddIntercept(IpInterceptLog log);

    Task AddInterceptAsync(IpInterceptLog log);

    int InterceptCount(string ip);

    public int TotalCount();

    List<IpInterceptLog> GetAll();

    IQueryable<IpInterceptLog> Queryable();

    bool Clear();

    public bool Reported(string ip);

    public Task<bool> ReportedAsync(string ip);
}
