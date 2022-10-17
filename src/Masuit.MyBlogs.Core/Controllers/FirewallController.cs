using CacheManager.Core;
using FreeRedis;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core.Controllers;

public class FirewallController : Controller
{
    public IRedisClient RedisClient { get; set; }
    private readonly HttpClient _httpClient;

    public FirewallController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// JS挑战，5秒盾
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPost("/challenge"), AutoValidateAntiforgeryToken]
    public ActionResult JsChallenge()
    {
        try
        {
            HttpContext.Session.Set("js-challenge", 1);
            Response.Cookies.Append(SessionKey.ChallengeBypass, DateTime.Now.AddSeconds(new Random().Next(60, 86400)).ToString("yyyy-MM-dd HH:mm:ss").AESEncrypt(AppConfig.BaiduAK), new CookieOptions()
            {
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddDays(1)
            });
            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    /// <summary>
    /// 验证码
    /// </summary>
    /// <returns></returns>
    [HttpGet("/challenge-captcha.jpg")]
    [ResponseCache(NoStore = true, Duration = 0)]
    public ActionResult CaptchaChallenge()
    {
        string code = ValidateCode.CreateValidateCode(6);
        HttpContext.Session.Set("challenge-captcha", code);
        var buffer = HttpContext.CreateValidateGraphic(code);
        return this.ResumeFile(buffer, ContentType.Jpeg, "验证码.jpg");
    }

    /// <summary>
    /// 验证码挑战
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost("/captcha"), AutoValidateAntiforgeryToken]
    public ActionResult CaptchaChallenge(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length < 4)
        {
            return BadRequest("验证码无效");
        }

        if (code.Equals(HttpContext.Session.Get<string>("challenge-captcha"), StringComparison.CurrentCultureIgnoreCase))
        {
            HttpContext.Session.Set("js-challenge", 1);
            HttpContext.Session.Remove("challenge-captcha");
            Response.Cookies.Append(SessionKey.ChallengeBypass, DateTime.Now.AddSeconds(new Random().Next(60, 86400)).ToString("yyyy-MM-dd HH:mm:ss").AESEncrypt(AppConfig.BaiduAK), new CookieOptions()
            {
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddDays(1)
            });
        }

        return Redirect(Request.Headers[HeaderNames.Referer]);
    }

    /// <summary>
    /// CloudflareTurnstile验证
    /// </summary>
    /// <returns></returns>
    [HttpPost("/turnstile-handler"), AutoValidateAntiforgeryToken]
    public async Task<ActionResult> CloudflareTurnstileHandler()
    {
        var form = await Request.ReadFormAsync();
        if (form.ContainsKey("cf-turnstile-response"))
        {
            var token = form["cf-turnstile-response"][0];
            const string url = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
            using var encodedContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new("secret",CommonHelper.SystemSettings["TurnstileSecretKey"]),
                new("response",token),
                new("remoteip",HttpContext.Connection.RemoteIpAddress.ToString()),
            });
            var resp = await _httpClient.PostAsync(url, encodedContent);
            var result = await resp.Content.ReadFromJsonAsync<TurnstileResult>();
            if (result.Success)
            {
                HttpContext.Session.Set("js-challenge", 1);
                Response.Cookies.Append(SessionKey.ChallengeBypass, DateTime.Now.AddSeconds(new Random().Next(60, 86400)).ToString("yyyy-MM-dd HH:mm:ss").AESEncrypt(AppConfig.BaiduAK), new CookieOptions()
                {
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddDays(1)
                });
            }
        }

        return Redirect(Request.Headers[HeaderNames.Referer]);
    }

    /// <summary>
    /// 反爬虫检测
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cacheManager"></param>
    /// <param name="env"></param>
    /// <returns></returns>
    [HttpGet("/craw/{id}")]
    [ServiceFilter(typeof(FirewallAttribute))]
    public IActionResult AntiCrawler(string id, [FromServices] ICacheManager<int> cacheManager, [FromServices] IWebHostEnvironment env)
    {
        if (Request.IsRobot())
        {
            return Ok();
        }

        var ip = HttpContext.Connection.RemoteIpAddress.ToString();
        RedisClient.LPush("intercept", new IpIntercepter()
        {
            IP = ip,
            RequestUrl = Request.GetDisplayUrl(),
            Time = DateTime.Now,
            Referer = Request.Headers[HeaderNames.Referer],
            UserAgent = Request.Headers[HeaderNames.UserAgent],
            Remark = "检测到异常爬虫行为",
            Address = Request.Location(),
            HttpVersion = Request.Protocol,
            Headers = new
            {
                Request.Protocol,
                Request.Headers
            }.ToJsonString()
        });
        cacheManager.AddOrUpdate("AntiCrawler:" + ip, 1, i => i + 1, 5);
        cacheManager.Expire("AntiCrawler:" + ip, ExpirationMode.Sliding, TimeSpan.FromMinutes(10));
        if (cacheManager.Get<int>("AntiCrawler:" + ip) > 3)
        {
            Response.StatusCode = 429;
            return Content("");
        }

        var sitemap = Path.Combine(env.WebRootPath, "sitemap.txt");
        return System.IO.File.Exists(sitemap) ? Redirect(System.IO.File.ReadLines(sitemap).OrderByRandom().FirstOrDefault() ?? "/") : Redirect("/");
    }
}

public class TurnstileResult
{
    public bool Success { get; set; }
    [JsonProperty("error-codes")]
    public string[] ErrorCodes { get; set; }
    [JsonProperty("challenge_ts")]
    public DateTime ChallengeTime { get; set; }
    public string Hostname { get; set; }
    public string Action { get; set; }
    public string Cdata { get; set; }
}
