using Masuit.MyBlogs.Core.Common;
using Masuit.Tools;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Masuit.MyBlogs.Core.Extensions.UEditor
{
    /// <summary>
    /// Crawler 的摘要说明
    /// </summary>
    public class CrawlerHandler : Handler
    {
        private string[] _sources;
        private Crawler[] _crawlers;

        public CrawlerHandler(HttpContext context) : base(context)
        {
        }

        public override string Process()
        {
            _sources = Request.Form["source[]"];
            if (_sources == null || _sources.Length == 0)
            {
                return WriteJson(new
                {
                    state = "参数错误：没有指定抓取源"
                });
            }
            _crawlers = _sources.Select(x => new Crawler(x).Fetch()).ToArray();
            return WriteJson(new
            {
                state = "SUCCESS",
                list = _crawlers.Select(x => new
                {
                    state = x.State,
                    source = x.SourceUrl,
                    url = x.ServerUrl
                })
            });
        }
    }

    public class Crawler
    {
        public string SourceUrl { get; set; }
        public string ServerUrl { get; set; }
        public string State { get; set; }

        public Crawler(string sourceUrl)
        {
            SourceUrl = sourceUrl;
        }

        public Crawler Fetch()
        {
            if (!SourceUrl.IsExternalAddress())
            {
                State = "INVALID_URL";
                return this;
            }
            var request = WebRequest.Create(SourceUrl) as HttpWebRequest;
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    State = "Url returns " + response.StatusCode + ", " + response.StatusDescription;
                    return this;
                }
                if (response.ContentType.IndexOf("image") == -1)
                {
                    State = "Url is not an image";
                    return this;
                }
                ServerUrl = PathFormatter.Format(Path.GetFileName(SourceUrl), UeditorConfig.GetString("catcherPathFormat"));

                try
                {
                    using (var stream = response.GetResponseStream())
                    {
                        var savePath = AppContext.BaseDirectory + "wwwroot" + ServerUrl;
                        using (var httpClient = new HttpClient())
                        {
                            var (url, success) = new ImagebedClient(httpClient).UploadImage(stream, savePath).Result;
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
                                using (var ms = new MemoryStream())
                                {
                                    stream.CopyTo(ms);
                                    File.WriteAllBytes(savePath, ms.GetBuffer());
                                }
                            }
                        }
                        State = "SUCCESS";
                    }
                }
                catch (Exception e)
                {
                    State = "抓取错误：" + e.Message;
                }
                return this;
            }
        }
    }
}