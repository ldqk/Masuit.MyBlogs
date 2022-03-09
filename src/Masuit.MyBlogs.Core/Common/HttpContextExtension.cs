using DnsClient;
using Masuit.Tools;
using Microsoft.Net.Http.Headers;
using Polly;

namespace Masuit.MyBlogs.Core.Common
{
    public static class HttpContextExtension
    {
        /// <summary>
        /// 地理位置信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IPLocation Location(this HttpRequest request)
        {
            return (IPLocation)request.HttpContext.Items.GetOrAdd("ip.location", () => request.HttpContext.Connection.RemoteIpAddress.GetIPLocation());
        }

        /// <summary>
        /// 是否是搜索引擎访问
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsRobot(this HttpRequest req)
        {
            var robotUA = UserAgent.Parse(req.Headers[HeaderNames.UserAgent].ToString()).IsRobot || req.Location().Contains("Spider", "蜘蛛");
            if (robotUA)
            {
                var nslookup = new LookupClient();
                var fallbackPolicy = Policy<bool>.Handle<Exception>().FallbackAsync(false);
                var retryPolicy = Policy<bool>.Handle<Exception>().RetryAsync(3);
                return Policy.WrapAsync(fallbackPolicy, retryPolicy).ExecuteAsync(async () =>
                {
                    using var cts = new CancellationTokenSource(1000);
                    var query = await nslookup.QueryReverseAsync(req.HttpContext.Connection.RemoteIpAddress, cts.Token);
                    return query.Answers.Any(r => r.ToString().Trim('.').EndsWith(new[]
                    {
                        "baidu.com",
                        "google.com",
                        "googlebot.com",
                        "googleusercontent.com",
                        "bing.com",
                        "search.msn.com",
                        "sogou.com",
                        "soso.com",
                        "yandex.com",
                        "apple.com",
                        "sm.cn"
                    }));
                }).Result;
            }

            return robotUA;
        }
    }
}
