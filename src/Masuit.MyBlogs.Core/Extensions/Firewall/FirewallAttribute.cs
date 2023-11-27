using FreeRedis;
using Markdig;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Controllers;
using Masuit.Tools.Mime;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public sealed class FirewallAttribute : IAsyncActionFilter
{
    public IFirewallRepoter FirewallRepoter { get; set; }

    public IMemoryCache MemoryCache { get; set; }

    public IRedisClient RedisClient { get; set; }

    private static readonly char[] Separator = { ',', '|', '，' };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        if (CommonHelper.SystemSettings.TryGetValue("BlockHeaderValues", out var v) && v.Length > 0)
        {
            var strs = v.Split("|", StringSplitOptions.RemoveEmptyEntries);
            if (request.Headers.Values.Any(values => strs.Any(s => values.Contains(s))))
            {
                context.Result = new NotFoundResult();
                return;
            }
        }

        request.Headers.Values.Contains("");
        var ip = context.HttpContext.Connection.RemoteIpAddress.ToString();
        var tokenValid = request.Cookies.ContainsKey("FullAccessToken") && request.Cookies["Email"].MDString(AppConfig.BaiduAK).Equals(request.Cookies["FullAccessToken"]);

        //黑名单
        if (ip.IsDenyIpAddress() && !tokenValid)
        {
            await AccessDeny(ip, request, "黑名单IP地址");
            context.Result = new BadRequestObjectResult("您当前所在的网络环境不支持访问本站！");
            return;
        }

        //bypass
        if (CommonHelper.SystemSettings.GetOrAdd("FirewallEnabled", "true") == "false" || context.ActionDescriptor.EndpointMetadata.Any(o => o is MyAuthorizeAttribute or AllowAccessFirewallAttribute) || tokenValid)
        {
            await next();
            return;
        }

        //UserAgent
        var ua = request.Headers[HeaderNames.UserAgent] + "";
        var blocked = CommonHelper.SystemSettings.GetOrAdd("UserAgentBlocked", "").Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (ua.Contains(blocked))
        {
            var agent = UserAgent.Parse(ua);
            await AccessDeny(ip, request, $"UA黑名单({agent.Browser} {agent.BrowserVersion}/{agent.Platform})");
            var msg = CommonHelper.SystemSettings.GetOrAdd("UserAgentBlockedMsg", "当前浏览器不支持访问本站");
            context.Result = new ContentResult
            {
                Content = Markdown.ToHtml(Template.Create(msg).Set("browser", agent.Browser + " " + agent.BrowserVersion).Set("os", agent.Platform).Render()),
                ContentType = ContentType.Html + "; charset=utf-8",
                StatusCode = 403
            };
            return;
        }

        //搜索引擎
        if (Regex.IsMatch(request.Method, "OPTIONS|HEAD", RegexOptions.IgnoreCase) || request.IsRobot())
        {
            await next();
            return;
        }

        // 反爬虫
        if (await RedisClient.GetAsync<int>(nameof(FirewallController.AntiCrawler) + ":" + ip) > 3)
        {
            context.Result = new ContentResult
            {
                ContentType = ContentType.Html + "; charset=utf-8",
                StatusCode = 429,
                Content = "检测到访问异常，请在10分钟后再试！"
            };
            return;
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
        var ipLocation = context.HttpContext.Connection.RemoteIpAddress.GetIPLocation();
        var (location, network, pos) = ipLocation;
        pos += ipLocation.Coodinate;
        var allowedAreas = CommonHelper.SystemSettings.GetOrAdd("AllowedArea", "").Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (allowedAreas.Length != 0 && pos.Contains(allowedAreas))
        {
            await next();
            return;
        }

        //黑名单地区
        var denyAreas = CommonHelper.SystemSettings.GetOrAdd("DenyArea", "").Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        if (denyAreas.Length != 0 && (string.IsNullOrWhiteSpace(location) || string.IsNullOrWhiteSpace(network) || pos.Contains(denyAreas) || denyAreas.Intersect(pos.Split("|")).Any()))
        {
            // 未知地区的，未知网络的，禁区的
            await AccessDeny(ip, request, "访问地区限制");
            throw new AccessDenyException("访问地区限制");
        }

        //挑战模式
        if (context.HttpContext.Session.TryGetValue("js-challenge", out _) || request.Path.ToUriComponent().Contains('.'))
        {
            await next();
            return;
        }

        try
        {
            if (request.Cookies.TryGetValue(SessionKey.ChallengeBypass, out var time) && time.AESDecrypt(AppConfig.BaiduAK).ToDateTime() > DateTime.Now)
            {
                context.HttpContext.Session.Set("js-challenge", 1);
                await next();
                return;
            }
        }
        catch
        {
            context.HttpContext.Response.Cookies.Delete(SessionKey.ChallengeBypass);
        }

        if (Challenge(context))
        {
            return;
        }

        //限流
        await ThrottleLimit(ip, request, next);
    }

    private static bool Challenge(ActionExecutingContext context)
    {
#if DEBUG
        return false;
#endif
        var rule = CommonHelper.SystemSettings.GetOrAdd("ChallengeRule", "");
        var regions = CommonHelper.SystemSettings.GetOrAdd("ChallengeRegions", "");
        var limitMode = CommonHelper.SystemSettings.GetOrAdd("ChallengeRegionLimitMode", "");
        if (rule == "Region")
        {
            var match = Regex.IsMatch(context.HttpContext.Request.Location(), regions, RegexOptions.IgnoreCase);
            switch (limitMode)
            {
                case "1": // 以内
                    if (match)
                    {
                        return ChallengeHandle(context);
                    }
                    break;

                case "2": // 以外
                    if (!match)
                    {
                        return ChallengeHandle(context);
                    }
                    break;
            }
            return false;
        }

        return ChallengeHandle(context);
    }

    private static bool ChallengeHandle(ActionExecutingContext context)
    {
        var mode = CommonHelper.SystemSettings.GetOrAdd(SessionKey.ChallengeMode, "");
        if (mode == SessionKey.JSChallenge)
        {
            context.Result = new ViewResult
            {
                ViewName = "/Views/Shared/JSChallenge.cshtml"
            };
            return true;
        }

        if (mode == SessionKey.CaptchaChallenge)
        {
            context.Result = new ViewResult
            {
                ViewName = "/Views/Shared/CaptchaChallenge.cshtml"
            };
            return true;
        }

        if (mode == SessionKey.CloudflareTurnstileChallenge)
        {
            context.Result = new ViewResult
            {
                ViewName = "/Views/Shared/CloudflareTurnstileChallenge.cshtml"
            };
            return true;
        }

        return false;
    }

    private async Task ThrottleLimit(string ip, HttpRequest request, ActionExecutionDelegate next)
    {
        var times = await RedisClient.IncrAsync("Frequency:" + ip);
        await RedisClient.ExpireAsync("Frequency:" + ip, TimeSpan.FromSeconds(CommonHelper.SystemSettings.GetOrAdd("LimitIPFrequency", "60").ToInt32()));
        var limit = CommonHelper.SystemSettings.GetOrAdd("LimitIPRequestTimes", "90").ToInt32();
        if (times <= limit)
        {
            await next();
        }

        if (times > limit * 1.2)
        {
            await RedisClient.ExpireAsync("Frequency:" + ip, TimeSpan.FromMinutes(CommonHelper.SystemSettings.GetOrAdd("BanIPTimespan", "10").ToInt32()));
            await AccessDeny(ip, request, "访问频次限制");
            throw new TempDenyException("访问频次限制");
        }

        await next();
    }

    private async Task AccessDeny(string ip, HttpRequest request, string remark)
    {
        var path = HttpUtility.UrlDecode(request.Path + request.QueryString, Encoding.UTF8);
        await RedisClient.IncrByAsync("interceptCount", 1);
        await RedisClient.LPushAsync("intercept", new IpIntercepter
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
        var key = "FirewallRepoter:" + FirewallRepoter.ReporterName + ":" + ip;
        if (!MemoryCache.TryGetValue(key, out _) && (await RedisClient.LRangeAsync<IpIntercepter>("intercept", 0, -1)).Count(x => x.IP == ip) >= CommonHelper.SystemSettings.GetOrAdd("LimitIPInterceptTimes", "30").ToInt32())
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
