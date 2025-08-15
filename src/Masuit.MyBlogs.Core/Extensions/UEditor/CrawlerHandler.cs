using Masuit.Tools.Mime;
using System.Net;
using System.Text.RegularExpressions;
using Masuit.Tools.Files;
using Polly;
using SixLabors.ImageSharp;

namespace Masuit.MyBlogs.Core.Extensions.UEditor;

/// <summary>
/// Crawler 的摘要说明
/// </summary>
public class CrawlerHandler(HttpContext context) : Handler(context)
{
    private readonly HttpClient _httpClient = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
    private readonly IConfiguration _configuration = context.RequestServices.GetRequiredService<IConfiguration>();

    public override async Task<string> Process()
    {
        var form = await Request.ReadFormAsync();
        string[] sources = form["source[]"];
        if (sources is { Length: > 0 })
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(sources.Length * 2));
            return WriteJson(new
            {
                state = "SUCCESS",
                list = await sources.SelectAsync(async s =>
                {
                    var crawler = new Crawler(s, _httpClient, _configuration, Context);
                    var fetch = await Policy<Crawler>.Handle<ObjectDisposedException>().RetryAsync(3).WrapAsync(Policy<Crawler>.Handle<ObjectDisposedException>().FallbackAsync(crawler)).ExecuteAsync(() => crawler.Fetch(cts.Token));
                    return new
                    {
                        state = fetch.State,
                        source = fetch.SourceUrl,
                        url = fetch.ServerUrl
                    };
                })
            });
        }

        return WriteJson(new
        {
            state = "参数错误：没有指定抓取源"
        });
    }
}

public class Crawler(string sourceUrl, HttpClient httpClient, IConfiguration configuration, HttpContext httpContext)
{
    public string SourceUrl { get; set; } = sourceUrl;

    public string ServerUrl { get; set; }

    public string State { get; set; }

    public async Task<Crawler> Fetch(CancellationToken token)
    {
        if (!SourceUrl.IsExternalAddress())
        {
            State = "INVALID_URL";
            return this;
        }

        httpClient.DefaultRequestHeaders.Referrer = new Uri(SourceUrl);
        var stream = await httpClient.GetAsync(configuration["HttpClientProxy:UriPrefix"] + SourceUrl, token).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var response = task.Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    State = "远程地址返回了错误的状态吗：" + response.StatusCode;
                    return new PooledMemoryStream();
                }

                var fileName = Path.GetFileNameWithoutExtension(SourceUrl).Next(s => Regex.Matches(s, @"\w+").LastOrDefault()?.Value);
                ServerUrl = PathFormatter.Format(fileName, CommonHelper.SystemSettings.GetOrAdd("UploadPath", "upload") + UeditorConfig.GetString("catcherPathFormat")) + MimeMapper.ExtTypes[response.Content.Headers.ContentType?.MediaType ?? "image/jpeg"][0];
                return response.Content.ReadAsStreamAsync().Result;
            }

            State = "远程请求失败";
            return new PooledMemoryStream();
        });
        if (stream.Length == 0)
        {
            return this;
        }

        var format = await Image.DetectFormatAsync(stream, token).ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);
        stream.Position = 0;
        if (format != null)
        {
            ServerUrl = ServerUrl.Replace(Path.GetExtension(ServerUrl), "." + format.Name.ToLower());
            if (!Regex.IsMatch(format.Name, "JPEG|PNG|Webp|GIF", RegexOptions.IgnoreCase))
            {
                using var image = await Image.LoadAsync(stream, token);
                await image.SaveAsJpegAsync(stream, token);
                ServerUrl = ServerUrl.Replace(Path.GetExtension(ServerUrl), ".jpg");
            }
        }

        var savePath = AppContext.BaseDirectory + "wwwroot" + ServerUrl;
        var (url, success) = await httpContext.RequestServices.GetRequiredService<ImagebedClient>().UploadImage(stream, savePath, token);
        if (success)
        {
            ServerUrl = url;
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            await stream.SaveFileAsync(savePath);
        }

        State = "SUCCESS";
        return this;
    }
}