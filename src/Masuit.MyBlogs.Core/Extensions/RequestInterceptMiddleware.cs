using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 请求拦截器
    /// </summary>
    public class RequestInterceptMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        public RequestInterceptMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Session.TryGetValue("session", out _) && !context.Request.IsRobot())
            {
                context.Session.Set("session", 0);
                CommonHelper.InterviewCount++;
                var referer = context.Request.Headers[HeaderNames.Referer].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    try
                    {
                        new Uri(referer);//判断是不是一个合法的referer
                        if (!referer.Contains(context.Request.Host.Value) && !referer.Contains(new[] { "baidu.com", "google", "sogou", "so.com", "bing.com", "sm.cn" }))
                        {
                            HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(IHangfireBackJob.UpdateLinkWeight), args: referer);
                        }
                    }
                    catch
                    {
                        await context.Response.WriteAsync("您的浏览器不支持访问本站！", Encoding.UTF8);
                        return;
                    }
                }
            }

            await _next.Invoke(context);
        }
    }
}