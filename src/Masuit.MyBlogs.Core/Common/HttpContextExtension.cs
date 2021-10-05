using DnsClient;
using Masuit.Tools;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Threading;

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
        /// 是否是机器人访问
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsRobot(this HttpRequest req)
        {
            var robotUA = UserAgent.Parse(req.Headers[HeaderNames.UserAgent].ToString()).IsRobot;
            if (robotUA)
            {
                var nslookup = new LookupClient();
                using var cts = new CancellationTokenSource(1000);
                return nslookup.QueryReverseAsync(req.HttpContext.Connection.RemoteIpAddress, cts.Token).ContinueWith(t => t.IsCompletedSuccessfully && t.Result.Answers.Any(r => r.ToString().Contains(new[]
                {
                    "baidu.com",
                    "google.com",
                    "googlebot.com",
                    "googleusercontent.com",
                    "bing.com",
                    "sogou.com",
                    "soso.com",
                    "yandex.com",
                    "sm.cn"
                 }))).Result;
            }

            return robotUA;
        }
    }
}