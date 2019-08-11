using System;
using System.Linq;
using System.Web;
using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class FirewallAttribute : ActionFilterAttribute
    {
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

            string ip = context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (ip.IsDenyIpAddress() && string.IsNullOrEmpty(context.HttpContext.Session.Get<string>("AccessViewToken")))
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

            try
            {
                var times = RedisHelper.IncrBy("Frequency:" + context.HttpContext.Session.Id);
                RedisHelper.Expire("Frequency:" + context.HttpContext.Session.Id, TimeSpan.FromMinutes(1));
                if (times > 300)
                {
                    context.Result = new RedirectToActionResult("TempDeny", "Error", null);
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}