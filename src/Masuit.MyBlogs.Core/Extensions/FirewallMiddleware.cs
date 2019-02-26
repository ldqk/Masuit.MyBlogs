using Common;
using Hangfire;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 网站防火墙
    /// </summary>
    public class FirewallMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        public FirewallMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 执行调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            string httpMethod = context.Request.Method;
            if (httpMethod.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase) || httpMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (context.Request.IsRobot())
            {
                //await _next.Invoke(context);
                return;
            }

            if (context.Request.Path.ToString().Contains(new[] { "error", "serviceunavailable" }))
            {
                await _next.Invoke(context);
                return;
            }

            if (context.Connection.RemoteIpAddress.MapToIPv4().ToString().IsDenyIpAddress())
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync($"检测到您的IP（{context.Connection.RemoteIpAddress.MapToIPv4()}）异常，已被本站禁止访问，如有疑问，请联系站长！");
                BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                {
                    IP = context.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                    RequestUrl = HttpUtility.UrlDecode(context.Request.Scheme + "://" + context.Request.Host + context.Request.Path),
                    Time = DateTime.Now
                }));
                return;
            }

            var times = RedisHelper.IncrBy("Frequency:" + context.Connection.Id);
            RedisHelper.Expire("Frequency:" + context.Connection.Id, TimeSpan.FromMinutes(1));
            if (times > 300)
            {
                await context.Response.WriteAsync($"检测到您的IP（{context.Connection.RemoteIpAddress}）访问过于频繁，已被本站暂时禁止访问，如有疑问，请联系站长！");
                return;
            }
            await _next.Invoke(context);
        }
    }
}