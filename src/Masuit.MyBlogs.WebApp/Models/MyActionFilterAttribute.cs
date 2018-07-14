using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Common;
using Masuit.MyBlogs.WebApp.Models.Hangfire;
using Masuit.Tools;
using Masuit.Tools.Net;

namespace Masuit.MyBlogs.WebApp.Models
{
    public class MyActionFilterAttribute : ActionFilterAttribute
    {
        private HtmlTextWriter _tw;
        private StringWriter _sw;
        private StringBuilder _sb;
        private HttpWriter _output;
        public static bool EnableViewCompress { get; set; } = Boolean.Parse(ConfigurationManager.AppSettings["EnableViewCompress"]);
        /// <summary>在执行操作方法之前由 ASP.NET MVC 框架调用。</summary>
        /// <param name="filterContext">筛选器上下文。</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var req = filterContext.HttpContext.Request;
            try
            {
                if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(AuthorityAttribute), true).Length <= 0 && req.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase) && req.UserAgent != null && !req.UserAgent.Contains(new[] { "DNSPod", "Baidu", "spider", "Python", "bot" }))
                {
                    Guid uid = filterContext.HttpContext.Session.Get<Guid>("currentOnline");
                    if (uid == Guid.Empty)
                    {
                        uid = Guid.NewGuid();
                        filterContext.HttpContext.Session.Set("currentOnline", uid);
                    }
                    HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.InterviewTrace), null, uid, req.Url.ToString().Replace(":80/", "/"));
                }
            }
            catch
            {
                // ignored
            }

            #region 禁用浏览器缓存

            filterContext.HttpContext.Response.Headers.Add("Pragma", "no-cache");
            filterContext.HttpContext.Response.Headers.Add("Expires", "0");
            filterContext.HttpContext.Response.Buffer = true;
            filterContext.HttpContext.Response.ExpiresAbsolute = DateTime.Now.AddSeconds(-1);
            filterContext.HttpContext.Response.Expires = 0;
            filterContext.HttpContext.Response.CacheControl = "no-cache";
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoStore();

            #endregion

            #region 启用ETag

            filterContext.HttpContext.Response.Filter = new ETagFilter(filterContext.HttpContext.Response, filterContext.RequestContext.HttpContext.Request);

            #endregion

            #region 压缩HTML

            if (EnableViewCompress)
            {
                _sb = new StringBuilder();
                _sw = new StringWriter(_sb);
                _tw = new HtmlTextWriter(_sw);
                _output = filterContext.RequestContext.HttpContext.Response.Output as HttpWriter;
                filterContext.RequestContext.HttpContext.Response.Output = _tw;
            }

            #endregion
        }

        /// <summary>
        /// 执行html压缩
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (EnableViewCompress)
            {
                string html = _sb.ToString();
                if (filterContext.HttpContext.Response.ContentType == "text/html") html = Regex.Replace(html, @"(?<=\s)\s+(?![^<>]*</pre>)", String.Empty);
                _output.Write(html);
            }
        }
    }

    public class ETagFilter : MemoryStream
    {
        private HttpResponseBase _response;
        private HttpRequestBase _request;
        private Stream _filter;

        public ETagFilter(HttpResponseBase response, HttpRequestBase request)
        {
            _response = response;
            _request = request;
            _filter = response.Filter;
        }

        private string GetToken(Stream stream)
        {
            byte[] checksum = MD5.Create().ComputeHash(stream);
            return Convert.ToBase64String(checksum, 0, checksum.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buffer, offset, data, 0, count);
            var token = GetToken(new MemoryStream(data));

            string clientToken = _request.Headers["If-None-Match"];

            if (token != clientToken)
            {
                _response.Headers["ETag"] = token;
                _filter.Write(data, 0, count);
            }
            else
            {
                _response.SuppressContent = true;
                _response.StatusCode = 304;
                _response.StatusDescription = "Not Modified";
                _response.Headers["Content-Length"] = "0";
            }
        }
    }
}