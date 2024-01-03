using Masuit.Tools.Logging;
using Polly;
using System.Net;
using System.Net.Sockets;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public sealed class CloudflareRepoter(HttpClient httpClient, IConfiguration configuration) : IFirewallRepoter
{
    public string ReporterName { get; set; } = "cloudflare";

    public void Report(IPAddress ip)
    {
        ReportAsync(ip).Wait();
    }

    public Task ReportAsync(IPAddress ip)
    {
        var scope = configuration["FirewallService:Cloudflare:Scope"];
        var zoneid = configuration["FirewallService:Cloudflare:ZoneId"];
        var fallbackPolicy = Policy.HandleInner<HttpRequestException>().FallbackAsync(_ =>
        {
            LogManager.Info($"cloudflare请求出错，{ip}上报失败！");
            return Task.CompletedTask;
        });
        var retryPolicy = Policy.HandleInner<HttpRequestException>().RetryAsync(3);
        return fallbackPolicy.WrapAsync(retryPolicy).ExecuteAsync(() => httpClient.PostAsJsonAsync($"https://api.cloudflare.com/client/v4/{scope}/{zoneid}/firewall/access_rules/rules", new
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
                value = ip.ToString()
            }
        }).ContinueWith(t =>
        {
            if (!t.Result.IsSuccessStatusCode)
            {
                throw new HttpRequestException("请求失败");
            }
        }));
    }
}
