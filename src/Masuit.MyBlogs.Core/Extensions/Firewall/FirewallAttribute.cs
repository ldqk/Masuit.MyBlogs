using CacheManager.Core;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace Masuit.MyBlogs.Core.Extensions.Firewall
{
    public class FirewallAttribute : ActionFilterAttribute
    {
        public ICacheManager<int> CacheManager { get; set; }
        public IFirewallRepoter FirewallRepoter { get; set; }

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var ip = context.HttpContext.Connection.RemoteIpAddress.ToString();
            var tokenValid = request.Cookies["Email"].MDString3(AppConfig.BaiduAK).Equals(request.Cookies["FullAccessToken"]);

            //黑名单
            if (ip.IsDenyIpAddress() && !tokenValid)
            {
                AccessDeny(ip, request, "黑名单IP地址");
                context.Result = new BadRequestObjectResult("您当前所在的网络环境不支持访问本站！");
                return;
            }

            //bypass
            if (CommonHelper.SystemSettings.GetOrAdd("FirewallEnabled", "true") == "false" || context.Filters.Any(m => m.ToString().Contains(new[] { nameof(AllowAccessFirewallAttribute), nameof(MyAuthorizeAttribute) })) || tokenValid)
            {
                return;
            }

            //UserAgent
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

            //搜索引擎
            if (Regex.IsMatch(request.Method, "OPTIONS|HEAD", RegexOptions.IgnoreCase) || request.IsRobot())
            {
                return;
            }

            DenyArea(ip, request);//禁区
            Challenge(context, request);//挑战模式
            ThrottleLimit(ip, request);//限流
        }

        private void DenyArea(string ip, HttpRequest request)
        {
            if (ip.IsInDenyArea())
            {
                AccessDeny(ip, request, "访问地区限制");
                throw new AccessDenyException("访问地区限制");
            }
        }

        private void ThrottleLimit(string ip, HttpRequest request)
        {
            var times = CacheManager.AddOrUpdate("Frequency:" + ip, 1, i => i + 1, 5);
            CacheManager.Expire("Frequency:" + ip, ExpirationMode.Absolute, TimeSpan.FromSeconds(CommonHelper.SystemSettings.GetOrAdd("LimitIPFrequency", "60").ToInt32()));
            var limit = CommonHelper.SystemSettings.GetOrAdd("LimitIPRequestTimes", "90").ToInt32();
            if (times <= limit)
            {
                return;
            }

            if (times > limit * 1.2)
            {
                CacheManager.Expire("Frequency:" + ip, TimeSpan.FromMinutes(CommonHelper.SystemSettings.GetOrAdd("BanIPTimespan", "10").ToInt32()));
                AccessDeny(ip, request, "访问频次限制");
            }

            throw new TempDenyException("访问频次限制");
        }

        private static void Challenge(ActionExecutingContext context, HttpRequest request)
        {
            if (!context.HttpContext.Session.TryGetValue("js-challenge", out _))
            {
                try
                {
                    if (request.Cookies.TryGetValue(SessionKey.ChallengeBypass, out var time) && time.AESDecrypt(AppConfig.BaiduAK).ToDateTime() > DateTime.Now)
                    {
                        context.HttpContext.Session.Set("js-challenge", 1);
                        return;
                    }
                }
                catch
                {
                    context.HttpContext.Response.Cookies.Delete(SessionKey.ChallengeBypass);
                }

                var mode = CommonHelper.SystemSettings.GetOrAdd(SessionKey.ChallengeMode, "");
                if (mode == SessionKey.JSChallenge)
                {
                    context.Result = new ViewResult()
                    {
                        ViewName = "/Views/Shared/JSChallenge.cshtml"
                    };
                }

                if (mode == SessionKey.CaptchaChallenge)
                {
                    context.Result = new ViewResult()
                    {
                        ViewName = "/Views/Shared/CaptchaChallenge.cshtml"
                    };
                }
            }
        }

        private async void AccessDeny(string ip, HttpRequest request, string remark)
        {
            var path = HttpUtility.UrlDecode(request.Path + request.QueryString, Encoding.UTF8);
            await RedisHelper.IncrByAsync("interceptCount");
            await RedisHelper.LPushAsync("intercept", new IpIntercepter()
            {
                IP = ip,
                RequestUrl = HttpUtility.UrlDecode(request.Scheme + "://" + request.Host + path),
                Time = DateTime.Now,
                Referer = request.Headers[HeaderNames.Referer],
                UserAgent = request.Headers[HeaderNames.UserAgent],
                Remark = remark,
                Address = request.Location(),
                HttpVersion = request.Protocol,
                Headers = request.Headers.ToJsonString()
            });
            var limit = CommonHelper.SystemSettings.GetOrAdd("LimitIPInterceptTimes", "30").ToInt32();
            await RedisHelper.LRangeAsync<IpIntercepter>("intercept", 0, -1).ContinueWith(async t =>
            {
                if (t.Result.Count(x => x.IP == ip) >= limit)
                {
                    LogManager.Info($"准备上报IP{ip}到{FirewallRepoter.ReporterName}");
                    await FirewallRepoter.ReportAsync(IPAddress.Parse(ip)).ContinueWith(_ => LogManager.Info($"访问频次限制，已上报IP{ip}至：" + FirewallRepoter.ReporterName));
                }
            });
        }
    }
}