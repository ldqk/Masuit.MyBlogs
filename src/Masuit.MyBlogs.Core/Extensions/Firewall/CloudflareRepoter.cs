using Masuit.Tools.Logging;
using Polly;
using System.Net;
using System.Net.Sockets;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public sealed class CloudflareRepoter(HttpClient httpClient, IConfiguration configuration, DataContext dataContext) : IFirewallRepoter
{
    public string ReporterName { get; set; } = "cloudflare";

    public void Report(IPAddress ip)
    {
        ReportAsync(ip).Wait();
    }

    public Task<bool> ReportAsync(IPAddress ip)
    {
        var s = ip.ToString();
        if (dataContext.IpReportLogs.Any(e => e.IP == s))
        {
            return Task.FromResult(false);
        }

        var scope = configuration["FirewallService:Cloudflare:Scope"];
        var zoneid = configuration["FirewallService:Cloudflare:ZoneId"];
        var fallbackPolicy = Policy.HandleInner<HttpRequestException>().FallbackAsync(_ =>
        {
            LogManager.Info($"cloudflare请求出错，{ip}上报失败！");
            return Task.FromResult(false);
        });
        var retryPolicy = Policy.HandleInner<HttpRequestException>().RetryAsync(3);
        return fallbackPolicy.WrapAsync(retryPolicy).ExecuteAsync(async () =>
        {
            await httpClient.PostAsJsonAsync($"https://api.cloudflare.com/client/v4/{scope}/{zoneid}/firewall/access_rules/rules", new
            {
                mode = "block",
                notes = $"恶意请求IP{ip.GetIPLocation()}",
                configuration = new
                {
                    target = ip.AddressFamily switch
                    {
                        AddressFamily.InterNetworkV6 => "ip6",
                        _ => "ip"
                    },
                    value = s
                }
            }).ContinueWith(t =>
            {
                if (!t.Result.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("请求失败");
                }
            });
            dataContext.IpReportLogs.Add(new IpReportLog
            {
                IP = s,
                Time = DateTime.Now
            });
            await dataContext.SaveChangesAsync();
            return true;
        });
    }
}
