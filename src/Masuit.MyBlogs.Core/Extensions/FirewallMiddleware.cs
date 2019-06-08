using Hangfire;
using Masuit.MyBlogs.Core.Common;
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
                await _next.Invoke(context);
                return;
            }

            if (context.Request.Path.ToString().Contains(new[] { "error", "serviceunavailable", "accessdeny", "tempdeny" }))
            {
                await _next.Invoke(context);
                return;
            }

            string ip = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (ip.IsDenyIpAddress())
            {
                BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                {
                    IP = ip,
                    RequestUrl = HttpUtility.UrlDecode(context.Request.Scheme + "://" + context.Request.Host + context.Request.Path),
                    Time = DateTime.Now
                }));
                context.Response.Redirect("/accessdeny", true);
                return;
            }

            try
            {
                var times = RedisHelper.IncrBy("Frequency:" + context.Session.Id);
                RedisHelper.Expire("Frequency:" + context.Session.Id, TimeSpan.FromMinutes(1));
                if (times > 300)
                {
                    context.Response.Redirect("/tempdeny", true);
                    return;
                }
            }
            catch
            {
                // ignore
            }

            await _next.Invoke(context);
        }
    }
}