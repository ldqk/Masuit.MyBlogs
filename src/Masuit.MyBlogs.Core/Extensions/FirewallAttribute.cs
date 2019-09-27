using CacheManager.Core;
using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
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

            string httpMethod = context.HttpContext.Request.Method;
            if (httpMethod.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase) || httpMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (context.HttpContext.Request.Cookies["Email"].MDString3(AppConfig.BaiduAK).Equals(context.HttpContext.Request.Cookies["FullAccessToken"]))
            {
                return;
            }

            string ip = context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (ip.IsDenyIpAddress() && string.IsNullOrEmpty(context.HttpContext.Session.Get<string>("FullAccessViewToken")))
            {
                BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                {
                    IP = ip,
                    RequestUrl = HttpUtility.UrlDecode(context.HttpContext.Request.Scheme + "://" + context.HttpContext.Request.Host + context.HttpContext.Request.Path),
                    Time = DateTime.Now
                }));
                context.Result = new RedirectToActionResult("AccessDeny", "Error", null);
                return;
            }

            if (context.HttpContext.Request.IsRobot())
            {
                return;
            }

            var times = CacheManager.AddOrUpdate("Frequency:" + ip, 1, i =>
            {
                i++;
                return i;
            }, 5);
            CacheManager.Expire("Frequency:" + ip, ExpirationMode.Sliding, TimeSpan.FromSeconds(CommonHelper.SystemSettings.GetOrAdd("LimitIPFrequency", "60").ToInt32()));
            var limit = CommonHelper.SystemSettings.GetOrAdd("LimitIPRequestTimes", "90").ToInt32();
            if (times <= limit)
            {
                return;
            }

            if (times > limit * 1.2)
            {
                CacheManager.Expire("Frequency:" + ip, ExpirationMode.Sliding, TimeSpan.FromMinutes(CommonHelper.SystemSettings.GetOrAdd("BanIPTimespan", "10").ToInt32()));
                BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                {
                    IP = ip,
                    RequestUrl = HttpUtility.UrlDecode(context.HttpContext.Request.Scheme + "://" + context.HttpContext.Request.Host + context.HttpContext.Request.Path),
                    Time = DateTime.Now
                }));
            }

            context.Result = new RedirectResult("/tempdeny");
        }
    }
}