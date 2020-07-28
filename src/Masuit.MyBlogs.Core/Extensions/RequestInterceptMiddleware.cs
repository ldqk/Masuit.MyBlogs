using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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
            var request = context.Request;
            var path = HttpUtility.UrlDecode(request.Path + request.QueryString, Encoding.UTF8);
            var requestUrl = HttpUtility.UrlDecode(request.Scheme + "://" + request.Host + path);
            var match = Regex.Match(path ?? "", CommonHelper.BanRegex);
            if (match.Length > 0)
            {
                BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                {
                    IP = context.Connection.RemoteIpAddress.ToString(),
                    RequestUrl = requestUrl,
                    Time = DateTime.Now,
                    UserAgent = request.Headers[HeaderNames.UserAgent],
                    Remark = $"检测到敏感词拦截：{match.Value}"
                }));
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("参数不合法！", Encoding.UTF8);
                return;
            }

            if (!context.Session.TryGetValue("session", out _) && !context.Request.IsRobot())
            {
                context.Session.Set("session", 0);
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
                        context.Response.StatusCode = 504;
                        await context.Response.WriteAsync("您的浏览器不支持访问本站！", Encoding.UTF8);
                        return;
                    }
                }
            }

            TrackData.RequestLogs.AddOrUpdate(requestUrl, 1, (s, i) => i + 1);
            await _next.Invoke(context);
        }
    }
}