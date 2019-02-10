using Common;
using Hangfire;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools;
using Masuit.Tools.Logging;
using Masuit.Tools.NoSQL;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 网站防火墙
    /// </summary>
    public class FirewallMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedisHelper _redisHelper;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        /// <param name="redisHelper"></param>
        public FirewallMiddleware(RequestDelegate next, RedisHelper redisHelper)
        {
            _next = next;
            _redisHelper = redisHelper;
        }

        public async Task Invoke(HttpContext context)
        {
            string httpMethod = context.Request.Method;
            if (httpMethod.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase) || httpMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            try
            {
                if (context.Connection.RemoteIpAddress.MapToIPv4().ToString().IsDenyIpAddress())
                {
                    await context.Response.WriteAsync($"检测到您的IP（{context.Connection.RemoteIpAddress.MapToIPv4()}）异常，已被本站禁止访问，如有疑问，请联系站长！");
                    BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                    {
                        IP = context.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                        RequestUrl = context.Request.Host.ToString(),
                        Time = DateTime.Now
                    }));
                    return;
                }
                bool isSpider = context.Request.Headers[HeaderNames.UserAgent].ToString().Contains(new[]
                {
                "DNSPod",
                "Baidu",
                "spider",
                "Python",
                "bot"
            });
                if (isSpider) return;
                var times = _redisHelper.StringIncrement("Frequency:" + context.Connection.Id);
                _redisHelper.Expire("Frequency:" + context.Connection.Id, TimeSpan.FromMinutes(1));
                if (times > 300)
                {
                    await context.Response.WriteAsync($"检测到您的IP（{context.Connection.RemoteIpAddress}）访问过于频繁，已被本站暂时禁止访问，如有疑问，请联系站长！");
                    return;
                }
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                LogManager.Error($"异常源：{e.Source}，异常类型：{e.GetType().Name}，\n请求路径：{context.Request.Scheme}://{context.Request.Host}{context.Request.Path.Value}，客户端用户代理：{context.Request.Headers["User-Agent"]}，客户端IP：{context.Connection.RemoteIpAddress}\t", e);
            }
        }
    }
}