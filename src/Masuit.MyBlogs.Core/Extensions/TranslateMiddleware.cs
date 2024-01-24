using Masuit.Tools.Mime;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using System.Text;
using System.Text.RegularExpressions;
using Yarp.ReverseProxy.Configuration;

namespace Masuit.MyBlogs.Core.Extensions;

/// <summary>
/// 简繁转换拦截器
/// </summary>
public sealed class TranslateMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="next"></param>
    public TranslateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context, IProxyConfigProvider proxy)
    {
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/_blazor") || path.StartsWith("/api") || path.StartsWith("/file") || path.StartsWith("/download") || context.Request.IsRobot())
        {
            return _next(context);
        }

        //if (proxy.GetConfig().Routes.Any(c => RouteMatch(c.Match.Path, path)))
        //{
        //    return _next(context);
        //}

        string lang = context.Request.Query["lang"];
        lang ??= context.Request.Cookies["lang"];
        if (string.IsNullOrEmpty(lang))
        {
            return context.Request.Location().Address.Contains(new[] { "台湾", "香港", "澳门", "Taiwan", "TW", "HongKong", "HK" }) ? Traditional(context) : _next(context);
        }
        return lang == "zh-cn" ? _next(context) : Traditional(context);
    }

    private static bool RouteMatch(string pattern, string input)
    {
        string regexPattern = "^" + pattern.Replace("*", ".*?").Replace("?", ".");
        regexPattern = Regex.Replace(regexPattern, @"\{\*\*.*?\}", ".*");
        regexPattern = Regex.Replace(regexPattern, @"\{.*?\}", ".*?");
        return Regex.IsMatch(input, regexPattern);
    }

    private async Task Traditional(HttpContext context)
    {
        var accept = context.Request.Headers["Accept"].ToString();
        if (accept.StartsWith("text") || accept.Contains(ContentType.Json))
        {
            //设置stream存放ResponseBody
            var responseOriginalBody = context.Response.Body;
            await using var memStream = new PooledMemoryStream();
            context.Response.Body = memStream;

            // 执行其他中间件
            await _next(context);

            //处理执行其他中间件后的ResponseBody
            memStream.Seek(0, SeekOrigin.Begin);
            using var responseReader = new StreamReader(memStream, Encoding.UTF8);
            var responseBody = await responseReader.ReadToEndAsync();
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.WriteAsync(Encoding.UTF8.GetBytes(ChineseConverter.Convert(responseBody, ChineseConversionDirection.SimplifiedToTraditional)).AsMemory());
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(responseOriginalBody);
            context.Response.Body = responseOriginalBody;
        }
        else
        {
            await _next(context);
        }
    }
}
