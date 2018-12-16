using Masuit.Tools;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Masuit.MyBlogs.WebApp.Models.UEditor
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

        public override void Process()
        {
            _sources = Request.Form.GetValues("source[]");
            if (_sources == null || _sources.Length == 0)
            {
                WriteJson(new
                {
                    state = "参数错误：没有指定抓取源"
                });
                return;
            }
            _crawlers = _sources.Select(x => new Crawler(x, Server).Fetch()).ToArray();
            WriteJson(new
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

        private HttpServerUtility Server { get; set; }


        public Crawler(string sourceUrl, HttpServerUtility server)
        {
            SourceUrl = sourceUrl;
            Server = server;
        }

        public Crawler Fetch()
        {
            if (!(SourceUrl.IsExternalAddress()))
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
                var savePath = Server.MapPath(ServerUrl);
                if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                }
                try
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            File.WriteAllBytes(savePath, ms.GetBuffer());
                        }
                        //var (url, success) = CommonHelper.UploadImage(savePath);
                        //if (success)
                        //{
                        //    ServerUrl = url;
                        //    BackgroundJob.Enqueue(() => File.Delete(savePath));
                        //}
                    }
                    State = "SUCCESS";
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