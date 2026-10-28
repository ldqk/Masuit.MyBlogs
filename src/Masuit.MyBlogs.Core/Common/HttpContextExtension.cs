using AngleSharp.Text;
using DnsClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using Polly;
using System.Net;

namespace Masuit.MyBlogs.Core.Common;

public static class HttpContextExtension
{
    private static readonly IMemoryCache RobotCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly LookupClient LookupClient = new();

    public static string LocalReferrerPath(this HttpRequest request, IUrlHelper url, bool includeQuery = false)
    {
        var referer = request.Headers[HeaderNames.Referer].ToString();
        if (!Uri.TryCreate(referer, UriKind.Absolute, out var uri))
        {
            return "/";
        }

        var path = includeQuery ? uri.PathAndQuery : uri.AbsolutePath;
        return url.IsLocalUrl(path) ? path : "/";
    }

    /// <summary>
    /// 地理位置信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static IPLocation Location(this HttpRequest request)
    {
        var location = (IPLocation)request.HttpContext.Items.GetOrAdd("ip.location", request.HttpContext.Connection.RemoteIpAddress.GetIPLocation);
        if (request.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["UseCloudflare"]?.ToBoolean() ?? false)
        {
            request.Headers.TryGetValue("cf-ipcity", out var city);
            request.Headers.TryGetValue("cf-region", out var region);
            request.Headers.TryGetValue("cf-ipcountry", out var country);
            location.Address2 += $"({country},{region},{city})";
        }

        return location;
    }

    public static int[] GetHideCategories(this HttpRequest request)
    {
        return request.Cookies[SessionKey.HideCategories]?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToInt32()).ToArray() ?? request.Query[SessionKey.SafeMode].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToInt32()).ToArray();
    }

    private static readonly string[] Spiders =
    [
        "baidu.com",
        "google.com",
        "googlebot.com",
        "bing.com",
        "bingbot.com",
        "search.msn.com",
        "sogou.com",
        "soso.com",
        "yandex.com",
        "yandex.net",
        "applebot.apple.com",
        "sm.cn",
        "telegram.org",
        "twttr.com"
    ];

    /// <summary>
    /// 是否是搜索引擎访问
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    public static bool IsRobot(this HttpRequest req)
    {
        return (bool)req.HttpContext.Items.GetOrAdd("ip.robot", () =>
        {
            if (UserAgent.Parse(req.Headers[HeaderNames.UserAgent].ToString()).IsRobot || req.Location().Contains("Spider", "蜘蛛", "Google", "Microsoft", "Baidu", "Cloudflare", "Telegram"))
            {
                return RobotCache.GetOrCreate($"ip.robot:{req.HttpContext.Connection.RemoteIpAddress}", entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60);
                    var fallbackPolicy = Policy<bool>.Handle<Exception>().FallbackAsync(false);
                    var retryPolicy = Policy<bool>.Handle<Exception>().RetryAsync(3);
                    return Policy.WrapAsync(fallbackPolicy, retryPolicy).ExecuteAsync(async () =>
                    {
                        using var cts = new CancellationTokenSource(1000);
                        var query = await LookupClient.QueryReverseAsync(req.HttpContext.Connection.RemoteIpAddress, cts.Token);
                        var remoteIp = req.HttpContext.Connection.RemoteIpAddress;
                        foreach (var host in query.Answers.PtrRecords().Select(record => record.PtrDomainName.Value.TrimEnd('.')).Where(host => Spiders.Any(domain => host.Equals(domain, StringComparison.OrdinalIgnoreCase) || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))))
                        {
                            var addresses = await Dns.GetHostAddressesAsync(host, cts.Token);
                            if (addresses.Any(address => address.Equals(remoteIp) || (address.IsIPv4MappedToIPv6 && address.MapToIPv4().Equals(remoteIp)) || (remoteIp.IsIPv4MappedToIPv6 && remoteIp.MapToIPv4().Equals(address))))
                            {
                                return true;
                            }
                        }

                        return false;
                    }).GetAwaiter().GetResult();
                });
            }

            return false;
        });
    }
}