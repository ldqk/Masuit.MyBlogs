using Masuit.MyBlogs.Core.Common;
using Masuit.Tools.Logging;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class CloudflareRepoter : IFirewallRepoter
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

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
            return _httpClient.PostAsJsonAsync($"https://api.cloudflare.com/client/v4/zones/{_configuration["FirewallService:Cloudflare:ZoneId"]}/firewall/access_rules/rules", new
            {
                mode = "block",
                notes = $"恶意请求IP({ip.GetIPLocation()})",
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
                    LogManager.Info("cloudflare请求出错");
                }
            });
        }
    }
}