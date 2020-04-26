using Masuit.MyBlogs.Core.Common;
using Masuit.Tools;
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

        public override string Process()
        {
            string[] sources = Request.Form["source[]"];
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
                using var response = _httpClient.GetAsync(SourceUrl).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    State = "Url returns " + response.StatusCode;
                    return this;
                }

                ServerUrl = PathFormatter.Format(Path.GetFileName(SourceUrl), CommonHelper.SystemSettings.GetOrAdd("UploadPath", "upload").Trim('/', '\\') + UeditorConfig.GetString("catcherPathFormat"));
                var stream = response.Content.ReadAsStreamAsync().Result;
                var savePath = AppContext.BaseDirectory + "wwwroot" + ServerUrl;
                stream = stream.AddWatermark();
                var (url, success) = Startup.ServiceProvider.GetRequiredService<ImagebedClient>().UploadImage(stream, savePath).Result;
                if (success)
                {
                    ServerUrl = url;
                }
                else
                {
                    if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                    }

                    var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    await File.WriteAllBytesAsync(savePath, ms.GetBuffer());
                }
                stream.Close();
                await stream.DisposeAsync();
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