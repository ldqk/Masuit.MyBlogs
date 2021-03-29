using Masuit.MyBlogs.Core.Common;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions.UEditor
{
    /// <summary>
    /// Crawler 的摘要说明
    /// </summary>
    public class CrawlerHandler : Handler
    {
        public CrawlerHandler(HttpContext context) : base(context)
        {
        }

        public override async Task<string> Process()
        {
            var form = await Request.ReadFormAsync();
            string[] sources = form["source[]"];
            if (sources?.Length > 0 || sources?.Length <= 10)
            {
                return WriteJson(new
                {
                    state = "SUCCESS",
                    list = (await sources.SelectAsync(s =>
                    {
                        return new Crawler(s).Fetch().ContinueWith(t => new
                        {
                            state = t.Result.State,
                            source = t.Result.SourceUrl,
                            url = t.Result.ServerUrl
                        });
                    }))
                });
            }

            return WriteJson(new
            {
                state = "参数错误：没有指定抓取源"
            });
        }
    }

    public class Crawler
    {
        public string SourceUrl { get; set; }
        public string ServerUrl { get; set; }
        public string State { get; set; }
        private static readonly HttpClient HttpClient = new(new HttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        public Crawler(string sourceUrl)
        {
            SourceUrl = sourceUrl;
        }

        public async Task<Crawler> Fetch()
        {
            if (!SourceUrl.IsExternalAddress())
            {
                State = "INVALID_URL";
                return this;
            }
            try
            {
                using var response = await HttpClient.GetAsync(SourceUrl);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    State = "远程地址返回了错误的状态吗：" + response.StatusCode;
                    return this;
                }

                ServerUrl = PathFormatter.Format(Path.GetFileNameWithoutExtension(SourceUrl), CommonHelper.SystemSettings.GetOrAdd("UploadPath", "upload") + UeditorConfig.GetString("catcherPathFormat")) + MimeMapper.ExtTypes[response.Content.Headers.ContentType?.MediaType ?? "image/jpeg"];
                await using var stream = await response.Content.ReadAsStreamAsync();
                var savePath = AppContext.BaseDirectory + "wwwroot" + ServerUrl;
                var (url, success) = await Startup.ServiceProvider.GetRequiredService<ImagebedClient>().UploadImage(stream, savePath, default);
                if (success)
                {
                    ServerUrl = url;
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                    await File.WriteAllBytesAsync(savePath, await stream.ToArrayAsync());
                }
                State = "SUCCESS";
            }
            catch (Exception e)
            {
                State = "抓取错误：" + e.Message;
                LogManager.Error(e);
            }

            return this;
        }
    }
}