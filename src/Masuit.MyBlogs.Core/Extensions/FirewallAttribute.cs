using CacheManager.Core;
using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class FirewallAttribute : ActionFilterAttribute
    {
        public ICacheManager<int> CacheManager { get; set; }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var ip = context.HttpContext.Connection.RemoteIpAddress.ToString();
            var trueip = request.Headers[AppConfig.TrueClientIPHeader].ToString();
            if (!string.IsNullOrEmpty(trueip) && ip != trueip)
            {
                ip = trueip;
            }

            var tokenValid = request.Cookies["Email"].MDString3(AppConfig.BaiduAK).Equals(request.Cookies["FullAccessToken"]);
            if (ip.IsDenyIpAddress() && !tokenValid)
            {
                AccessDeny(ip, request, "黑名单IP地址");
                context.Result = new BadRequestObjectResult("您当前所在的网络环境不支持访问本站！");
                return;
            }

            if (CommonHelper.SystemSettings.GetOrAdd("FirewallEnabled", "true") == "false" || context.Filters.Any(m => m.ToString().Contains(nameof(AllowAccessFirewallAttribute))) || tokenValid)
            {
                return;
            }

            var ua = request.Headers[HeaderNames.UserAgent] + "";
            var blocked = CommonHelper.SystemSettings.GetOrAdd("UserAgentBlocked", "").Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (ua.Contains(blocked))
            {
                var agent = UserAgent.Parse(ua);
                AccessDeny(ip, request, $"UA黑名单({agent.Browser} {agent.BrowserVersion}/{agent.Platform})");
                var msg = CommonHelper.SystemSettings.GetOrAdd("UserAgentBlockedMsg", "当前浏览器不支持访问本站");
                context.Result = new ContentResult()
                {
                    Content = Template.Create(msg).Set("browser", agent.Browser + " " + agent.BrowserVersion).Set("os", agent.Platform).Render(),
                    ContentType = ContentType.Html,
                    StatusCode = 403
                };
                return;
            }
            if (ip.IsInDenyArea() && !tokenValid)
            {
                AccessDeny(ip, request, "访问地区限制");
                throw new AccessDenyException("访问地区限制");
            }

            if (Regex.IsMatch(request.Method, "OPTIONS|HEAD", RegexOptions.IgnoreCase) || request.IsRobot())
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
                AccessDeny(ip, request, "访问频次限制");
            }

            throw new TempDenyException("访问地区限制");
        }

        private void AccessDeny(string ip, HttpRequest request, string remark)
        {
            var path = HttpUtility.UrlDecode(request.Path + request.QueryString, Encoding.UTF8);
            BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
            {
                IP = ip,
                RequestUrl = HttpUtility.UrlDecode(request.Scheme + "://" + request.Host + path),
                Time = DateTime.Now,
                UserAgent = request.Headers[HeaderNames.UserAgent],
                Remark = remark
            }));
        }
    }
}