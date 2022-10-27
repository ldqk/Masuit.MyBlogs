using CacheManager.Core;
using FreeRedis;
using Markdig;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Controllers;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public class FirewallAttribute : IAsyncActionFilter
{
    public ICacheManager<int> CacheManager { get; set; }

    public IFirewallRepoter FirewallRepoter { get; set; }

    public IMemoryCache MemoryCache { get; set; }
    public IRedisClient RedisClient { get; set; }

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        if (CommonHelper.SystemSettings.TryGetValue("BlockHeaderValues", out var v) && v.Length > 0)
        {
            var strs = v.Split("|", StringSplitOptions.RemoveEmptyEntries);
            if (request.Headers.Values.Any(values => strs.Any(s => values.Contains(s))))
            {
                context.Result = new NotFoundResult();
                return Task.CompletedTask;
            }
        }

        request.Headers.Values.Contains("");
        var ip = context.HttpContext.Connection.RemoteIpAddress.ToString();
        var tokenValid = request.Cookies.ContainsKey("FullAccessToken") && request.Cookies["Email"].MDString(AppConfig.BaiduAK).Equals(request.Cookies["FullAccessToken"]);

        //黑名单
        if (ip.IsDenyIpAddress() && !tokenValid)
        {
            AccessDeny(ip, request, "黑名单IP地址");
            context.Result = new BadRequestObjectResult("您当前所在的网络环境不支持访问本站！");
            return Task.CompletedTask;
        }

        //bypass
        if (CommonHelper.SystemSettings.GetOrAdd("FirewallEnabled", "true") == "false" || context.ActionDescriptor.EndpointMetadata.Any(o => o is MyAuthorizeAttribute or AllowAccessFirewallAttribute) || tokenValid)
        {
            return next();
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
                Content = Markdown.ToHtml(Template.Create(msg).Set("browser", agent.Browser + " " + agent.BrowserVersion).Set("os", agent.Platform).Render()),
                ContentType = ContentType.Html + "; charset=utf-8",
                StatusCode = 403
            };
            return Task.CompletedTask;
        }

        //搜索引擎
        if (Regex.IsMatch(request.Method, "OPTIONS|HEAD", RegexOptions.IgnoreCase) || request.IsRobot())
        {
            return next();
        }

        // 反爬虫
        if (CacheManager.GetOrAdd(nameof(FirewallController.AntiCrawler) + ":" + ip, 0) > 3)
        {
            context.Result = new ContentResult
            {
                ContentType = ContentType.Html + "; charset=utf-8",
                StatusCode = 429,
                Content = "检测到访问异常，请在10分钟后再试！"
            };
            return Task.CompletedTask;
        }

        //安全模式
        if (request.Query[SessionKey.SafeMode].Count > 0)
        {
            request.Cookies.TryGetValue(SessionKey.HideCategories, out var s);
            context.HttpContext.Response.Cookies.Append(SessionKey.HideCategories, request.Query[SessionKey.SafeMode] + "," + s, new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
        }

        //白名单地区
        var ipLocation = ip.GetIPLocation();
        var (location, network, pos) = ipLocation;
        pos += ipLocation.Coodinate;
        var allowedAreas = CommonHelper.SystemSettings.GetOrAdd("AllowedArea", "").Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
        if (allowedAreas.Any() && pos.Contains(allowedAreas))
        {
            return next();
        }

        //黑名单地区
        var denyAreas = CommonHelper.SystemSettings.GetOrAdd("DenyArea", "").Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
        if (denyAreas.Any())
        {
            if (string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(network) || pos.Contains(denyAreas) || denyAreas.Intersect(pos.Split("|")).Any()) // 未知地区的，未知网络的，禁区的
            {
                AccessDeny(ip, request, "访问地区限制");
                throw new AccessDenyException("访问地区限制");
            }
        }

        //挑战模式
        if (context.HttpContext.Session.TryGetValue("js-challenge", out _) || request.Path.ToUriComponent().Contains("."))
        {
            return next();
        }

        try
        {
            if (request.Cookies.TryGetValue(SessionKey.ChallengeBypass, out var time) && time.AESDecrypt(AppConfig.BaiduAK).ToDateTime() > DateTime.Now)
            {
                context.HttpContext.Session.Set("js-challenge", 1);
                return next();
            }
        }
        catch
        {
            context.HttpContext.Response.Cookies.Delete(SessionKey.ChallengeBypass);
        }

        if (Challenge(context, out var completedTask))
        {
            return completedTask;
        }

        //限流
        return ThrottleLimit(ip, request, next);
    }

    private static bool Challenge(ActionExecutingContext context, out Task completedTask)
    {
        var mode = CommonHelper.SystemSettings.GetOrAdd(SessionKey.ChallengeMode, "");
        if (mode == SessionKey.JSChallenge)
        {
            context.Result = new ViewResult()
            {
                ViewName = "/Views/Shared/JSChallenge.cshtml"
            };
            completedTask = Task.CompletedTask;
            return true;
        }

        if (mode == SessionKey.CaptchaChallenge)
        {
            context.Result = new ViewResult()
            {
                ViewName = "/Views/Shared/CaptchaChallenge.cshtml"
            };
            completedTask = Task.CompletedTask;
            return true;
        }

        if (mode == SessionKey.CloudflareTurnstileChallenge)
        {
            context.Result = new ViewResult()
            {
                ViewName = "/Views/Shared/CloudflareTurnstileChallenge.cshtml"
            };
            completedTask = Task.CompletedTask;
            return true;
        }

        completedTask = Task.CompletedTask;
        return false;
    }

    private Task ThrottleLimit(string ip, HttpRequest request, ActionExecutionDelegate next)
    {
        var times = CacheManager.AddOrUpdate("Frequency:" + ip, 1, i => i + 1, 5);
        CacheManager.Expire("Frequency:" + ip, ExpirationMode.Absolute, TimeSpan.FromSeconds(CommonHelper.SystemSettings.GetOrAdd("LimitIPFrequency", "60").ToInt32()));
        var limit = CommonHelper.SystemSettings.GetOrAdd("LimitIPRequestTimes", "90").ToInt32();
        if (times <= limit)
        {
            return next();
        }

        if (times > limit * 1.2)
        {
            CacheManager.Expire("Frequency:" + ip, TimeSpan.FromMinutes(CommonHelper.SystemSettings.GetOrAdd("BanIPTimespan", "10").ToInt32()));
            AccessDeny(ip, request, "访问频次限制");
            throw new TempDenyException("访问频次限制");
        }

        return next();
    }

    private async void AccessDeny(string ip, HttpRequest request, string remark)
    {
        var path = HttpUtility.UrlDecode(request.Path + request.QueryString, Encoding.UTF8);
        RedisClient.IncrBy("interceptCount", 1);
        RedisClient.LPush("intercept", new IpIntercepter()
        {
            IP = ip,
            RequestUrl = HttpUtility.UrlDecode(request.Scheme + "://" + request.Host + path),
            Time = DateTime.Now,
            Referer = request.Headers[HeaderNames.Referer],
            UserAgent = request.Headers[HeaderNames.UserAgent],
            Remark = remark,
            Address = request.Location(),
            HttpVersion = request.Protocol,
            Headers = new
            {
                request.Protocol,
                request.Headers
            }.ToJsonString()
        });
        var limit = CommonHelper.SystemSettings.GetOrAdd("LimitIPInterceptTimes", "30").ToInt32();
        var list = RedisClient.LRange<IpIntercepter>("intercept", 0, -1);
        var key = "FirewallRepoter:" + FirewallRepoter.ReporterName + ":" + ip;
        if (list.Count(x => x.IP == ip) >= limit && !MemoryCache.TryGetValue(key, out _))
        {
            LogManager.Info($"准备上报IP{ip}到{FirewallRepoter.ReporterName}");
            await FirewallRepoter.ReportAsync(IPAddress.Parse(ip)).ContinueWith(_ =>
            {
                MemoryCache.Set(key, 1, TimeSpan.FromDays(1));
                LogManager.Info($"访问频次限制，已上报IP{ip}至：" + FirewallRepoter.ReporterName);
            });
        }
    }
}
