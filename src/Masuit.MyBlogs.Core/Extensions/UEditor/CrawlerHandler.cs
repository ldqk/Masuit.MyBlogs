using Masuit.MyBlogs.Core.Common;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private static readonly HashSet<string> _queue = new HashSet<string>();

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
                    list = sources.Except(_queue).AsParallel().Select(s =>
                    {
                        _queue.Add(s);
                        return new Crawler(s).Fetch().Result;
                    }).Select(x =>
                    {
                        _queue.Remove(x.SourceUrl);
                        return new
                        {
                            state = x.State,
                            source = x.SourceUrl,
                            url = x.ServerUrl
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

    public class Crawler
    {
        public string SourceUrl { get; set; }
        public string ServerUrl { get; set; }
        public string State { get; set; }
        private readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler()
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
                using var response = await _httpClient.GetAsync(SourceUrl);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    State = "Url returns " + response.StatusCode;
                    return this;
                }

                ServerUrl = PathFormatter.Format(Path.GetFileName(SourceUrl), CommonHelper.SystemSettings.GetOrAdd("UploadPath", "upload").Trim('/', '\\') + UeditorConfig.GetString("catcherPathFormat"));
                var mediaType = response.Content.Headers.ContentType.MediaType;
                await using var stream = await response.Content.ReadAsStreamAsync();
                var savePath = Path.Combine(AppContext.BaseDirectory + "wwwroot", ServerUrl);
                if (string.IsNullOrEmpty(Path.GetExtension(savePath)))
                {
                    savePath += MimeMapper.ExtTypes[mediaType];
                    ServerUrl += MimeMapper.ExtTypes[mediaType];
                }

                var (url, success) = await Startup.ServiceProvider.GetRequiredService<ImagebedClient>().UploadImage(stream, savePath);
                if (success)
                {
                    ServerUrl = url;
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                    await File.WriteAllBytesAsync(savePath, stream.ToArray());
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