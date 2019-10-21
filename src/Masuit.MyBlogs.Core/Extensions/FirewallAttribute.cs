using CacheManager.Core;
using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class FirewallAttribute : ActionFilterAttribute
    {
        public ICacheManager<int> CacheManager { get; set; }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Filters.Any(m => m.ToString().Contains(nameof(AllowAccessFirewallAttribute))))
            {
                return;
            }

            var request = context.HttpContext.Request;
            var httpMethod = request.Method;
            if (httpMethod.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase) || httpMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (request.Cookies["Email"].MDString3(AppConfig.BaiduAK).Equals(request.Cookies["FullAccessToken"]))
            {
                return;
            }

            var ip = context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (ip.IsDenyIpAddress() && string.IsNullOrEmpty(context.HttpContext.Session.Get<string>("FullAccessViewToken")))
            {
                AccessDeny(context, ip, request);
                return;
            }

            if (request.IsRobot())
            {
                return;
            }

            var times = CacheManager.AddOrUpdate("Frequency:" + ip, 1, i => i + 1, 5);
            CacheManager.Expire("Frequency:" + ip, ExpirationMode.Sliding, TimeSpan.FromSeconds(CommonHelper.SystemSettings.GetOrAdd("LimitIPFrequency", "60").ToInt32()));
            var limit = CommonHelper.SystemSettings.GetOrAdd("LimitIPRequestTimes", "90").ToInt32();
            if (times <= limit)
            {
                return;
            }

            if (times > limit * 1.2)
            {
                CacheManager.Expire("Frequency:" + ip, ExpirationMode.Sliding, TimeSpan.FromMinutes(CommonHelper.SystemSettings.GetOrAdd("BanIPTimespan", "10").ToInt32()));
                var path = HttpUtility.UrlDecode(request.Path + request.QueryString, Encoding.UTF8);
                BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                {
                    IP = ip,
                    RequestUrl = HttpUtility.UrlDecode(request.Scheme + "://" + request.Host + path),
                    Time = DateTime.Now,
                    UserAgent = request.Headers[HeaderNames.UserAgent]
                }));
            }

            context.Result = new RedirectResult("/tempdeny");
        }

        private void AccessDeny(ActionExecutingContext context, string ip, HttpRequest request)
        {
            var path = HttpUtility.UrlDecode(request.Path + request.QueryString, Encoding.UTF8);
            BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
            {
                IP = ip,
                RequestUrl = HttpUtility.UrlDecode(request.Scheme + "://" + request.Host + path),
                Time = DateTime.Now,
                UserAgent = request.Headers[HeaderNames.UserAgent]
            }));
            context.Result = new RedirectToActionResult("AccessDeny", "Error", null);
        }
    }
}