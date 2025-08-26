using AngleSharp;
using DnsClient;
using Masuit.Tools.Mime;
using Masuit.Tools.Core.Validator;
using Microsoft.Net.Http.Headers;
using System.Net;
using TimeZoneConverter;
using Configuration = AngleSharp.Configuration;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 黑科技
/// </summary>
[Route("tools")]
public sealed class ToolsController(IHttpClientFactory httpClientFactory) : BaseController
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();

    /// <summary>
    /// 获取ip地址详细信息
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    [Route("ip"), Route("ip/{ip?}", Order = 1), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "ip" })]
    public async Task<ActionResult> GetIpInfo([IsIPAddress] string ip)
    {
        ViewBag.IP = ip;
        if (!IPAddress.TryParse(ip, out var ipAddress))
        {
            ipAddress = ClientIP;
            ViewBag.IP = ClientIP;
        }

        if (ipAddress.IsPrivateIP())
        {
            return Ok("内网IP");
        }

        var loc = ipAddress.GetIPLocation();
        var asn = ipAddress.GetIPAsn();
        var nslookup = new LookupClient();
        using var cts = new CancellationTokenSource(2000);
        var domain = await nslookup.QueryReverseAsync(ipAddress, cts.Token).ContinueWith(t => t.IsCompletedSuccessfully ? t.Result.Answers.Select(r => r.ToString()).Join("; ") : "无");
        var address = new IpInfo
        {
            Location = loc.Coodinate,
            Address = loc.Address,
            Address2 = loc.Address2,
            Network = new NetworkInfo
            {
                Asn = asn.AutonomousSystemNumber,
                Router = asn.Network + "",
                Organization = loc.ISP
            },
            Network2 = loc.Network2,
            TimeZone = loc.Coodinate.TimeZone + $"  UTC{TZConvert.GetTimeZoneInfo(loc.Coodinate.TimeZone ?? "Asia/Shanghai").BaseUtcOffset.Hours:+#;-#;0}",
            IsProxy = loc.Network.Contains(["cloud", "Compute", "Serv", "Tech", "Solution", "Host", "云", "Datacenter", "Data Center", "Business", "ASN"]) || domain.Length > 1 || await IsProxy(ipAddress, cts.Token),
            Domain = domain
        };
        if (Request.Method.Equals(HttpMethods.Get) || (Request.Headers[HeaderNames.Accept] + "").StartsWith(ContentType.Json))
        {
            return View(address);
        }

        return Json(address);
    }

    /// <summary>
    /// 是否是代理ip
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<bool> IsProxy(IPAddress ip, CancellationToken cancellationToken = default)
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36 Edg/92.0.902.62");
        return await _httpClient.GetStringAsync("https://ipinfo.io/" + ip, cancellationToken).ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                var ctx = BrowsingContext.New(Configuration.Default);
                var doc = ctx.OpenAsync(res => res.Content(t.Result)).Result;
                var isAnycast = doc.DocumentElement.QuerySelectorAll(".title").Where(e => e.TextContent.Contains("Anycast")).Select(e => e.Parent).Any(n => n.TextContent.Contains("True"));
                var isproxy = doc.DocumentElement.QuerySelectorAll("#block-privacy img").Any(e => e.OuterHtml.Contains("right"));
                return isAnycast || isproxy;
            }
            return false;
        });
    }

    [HttpGet("loan")]
    public ActionResult Loan()
    {
        return View();
    }
}