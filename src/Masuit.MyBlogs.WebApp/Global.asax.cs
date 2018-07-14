using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Common;
using IBLL;
using Masuit.MyBlogs.WebApp.Models.Hangfire;
using Masuit.Tools;
using Masuit.Tools.Logging;
using Masuit.Tools.Net;
using Masuit.Tools.NoSQL;
#if DEBUG
using Masuit.Tools.Win32;
#endif
using Models.Entity;
using Z.EntityFramework.Extensions;

namespace Masuit.MyBlogs.WebApp
{
    public class Global : HttpApplication
    {
        //private static RedisHelper RedisHelper = new RedisHelper(2);
        private RedisHelper RedisHelper { get; } = RedisHelper.GetInstance();
        protected void Application_Start(object sender, EventArgs e)
        {
            LicenseManager.AddLicense("67;100-MASUIT", "809739091397182EC1ECEA8770EB4218");
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Web.Http.GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            AutofacConfig.RegisterMVC();
            StartupConfig.Startup();
        }
        protected void Session_Start(object sender, EventArgs e)
        {
            HttpRequest request = Request;
            string ua = request.UserAgent;
            string ip = request.UserHostAddress;
#if DEBUG
            Random r = new Random();
            ip = $"{r.StrictNext(235)}.{r.StrictNext(255)}.{r.StrictNext(255)}.{r.StrictNext(255)}";
#endif
            Session.Set("landDate", DateTime.Now);
            ip.MatchInetAddress(out bool success);
            if (success)
            {
                Guid uid = Guid.NewGuid();
                Session.Set("currentOnline", uid);
                Task.Factory.StartNew(s =>
                {
                    HttpRequest req = s as HttpRequest;
                    bool isNotSpider = ua != null && !ua.Contains(new[] { "DNSPod", "Baidu", "spider", "Python", "bot" });
                    if (isNotSpider) //屏蔽百度云观测以及搜索引擎爬虫
                    {
                        string refer;
                        try
                        {
                            refer = req.UrlReferrer?.AbsoluteUri ?? "直接输入网址";
                        }
                        catch (Exception)
                        {
                            refer = "直接输入网址";
                        }
                        string browserType = req.Browser.Type;
                        if (browserType.Contains("Chrome1") || browserType.Contains("Chrome2") || browserType.Contains("Chrome3") || browserType.Equals("Chrome4") || browserType.Equals("Chrome7") || browserType.Equals("Chrome9") || browserType.Contains("Chrome40") || browserType.Contains("Chrome41") || browserType.Contains("Chrome42") || browserType.Contains("Chrome43"))
                        {
                            browserType = "Chrome43-";
                        }
                        else if (browserType.Contains("IE"))
                        {
                            browserType = "InternetExplorer" + req.Browser.Version;
                        }
                        else if (browserType.Equals("Safari6") || browserType.Equals("Safari5") || browserType.Equals("Safari4") || browserType.Equals("Safari"))
                        {
                            browserType = "Safari6-";
                        }
                        Interview interview = new Interview()
                        {
                            IP = ip,
                            UserAgent = ua,
                            BrowserType = browserType,
                            OperatingSystem = req.Browser.Platform,
                            ViewTime = DateTime.Now,
                            FromUrl = refer,
                            HttpMethod = req.HttpMethod,
                            LandPage = req.Url.ToString(),
                            Uid = uid
                        };
                        HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.FlushInetAddress), args: interview);
                    }
                }, request);
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
#if !DEBUG
            if (Request.UserHostAddress == null || CommonHelper.DenyIP.Contains(Request.UserHostAddress))
            {
                Response.Write($"检测到您的IP（{Request.UserHostAddress}）异常，已被本站禁止访问，如有疑问，请联系站长！");
                Response.End();
                return;
            }
#endif
            string httpMethod = Request.HttpMethod;
            if (httpMethod.Equals("OPTIONS", StringComparison.InvariantCultureIgnoreCase) || httpMethod.Equals("HEAD", StringComparison.InvariantCultureIgnoreCase))
            {
                Response.End();
                return;
            }

            bool isSpider = Request.UserAgent != null && Request.UserAgent.Contains(new[] { "DNSPod", "Baidu", "spider", "Python", "bot" });
            if (isSpider) return;
            try
            {
                var times = RedisHelper.StringIncrement("Frequency:" + Request.UserHostAddress + ":" + Request.UserAgent);
                RedisHelper.Expire("Frequency:" + Request.UserHostAddress + ":" + Request.UserAgent, TimeSpan.FromMinutes(1));
                if (times > 200)
                {
                    Response.Write($"检测到您的IP（{Request.UserHostAddress}）访问过于频繁，已被本站暂时禁止访问，如有疑问，请联系站长！");
                    Response.End();
                    return;
                }
            }
            catch
            {
                // ignored
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            HttpException exception = ((HttpApplication)sender).Context.Error as HttpException;
            int? errorCode = exception?.GetHttpCode() ?? 503;
            switch (errorCode)
            {
                case 404:
                    Response.Redirect("/error");
                    break;
                case 503:
                    LogManager.Error(exception ?? ((HttpApplication)sender).Server.GetLastError());
                    Response.Redirect("/ServiceUnavailable");
                    break;
                default:
                    return;
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {
            var uid = Session.Get<Guid>("currentOnline");
            IInterviewBll interviewBll = AutofacConfig.Container.Resolve<IInterviewBll>();
            Interview interview = interviewBll.GetFirstEntity(i => i.Uid.Equals(uid));
            var interviewDetails = interview.InterviewDetails;
            TimeSpan ts = DateTime.Now - interviewDetails.FirstOrDefault().Time;
            if (ts.TotalMinutes > 20)
            {
                ts = ts - TimeSpan.FromMinutes(20);
            }
            string len = string.Empty;
            if (ts.Hours > 0)
            {
                len += $"{ts.Hours}小时";
            }

            if (ts.Minutes > 0)
            {
                len += $"{ts.Minutes}分钟";
            }
            len += $"{ts.Seconds}.{ts.Milliseconds}秒";
            interview.OnlineSpan = len;
            interview.OnlineSpanSeconds = ts.TotalSeconds;
            interviewBll.UpdateEntitySaved(interview);

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}