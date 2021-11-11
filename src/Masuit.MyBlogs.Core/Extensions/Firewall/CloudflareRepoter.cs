using Masuit.MyBlogs.Core.Common;
using Masuit.Tools.Logging;
using Polly;
using System.Net;
using System.Net.Sockets;

namespace Masuit.MyBlogs.Core.Extensions.Firewall
{
    public class CloudflareRepoter : IFirewallRepoter
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public string ReporterName { get; set; } = "cloudflare";


        public CloudflareRepoter(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public void Report(IPAddress ip)
        {
            ReportAsync(ip).Wait();
        }

        public Task ReportAsync(IPAddress ip)
        {
            var scope = _configuration["FirewallService:Cloudflare:Scope"];
            var zoneid = _configuration["FirewallService:Cloudflare:ZoneId"];
            var fallbackPolicy = Policy.HandleInner<HttpRequestException>().FallbackAsync(_ =>
            {
                LogManager.Info($"cloudflare请求出错，{ip}上报失败！");
                return Task.CompletedTask;
            });
            var retryPolicy = Policy.HandleInner<HttpRequestException>().RetryAsync(3);
            return fallbackPolicy.WrapAsync(retryPolicy).ExecuteAsync(() => _httpClient.PostAsJsonAsync($"https://api.cloudflare.com/client/v4/{scope}/{zoneid}/firewall/access_rules/rules", new
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
}