using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Common;
using Hangfire;
using Masuit.MyBlogs.WebApp.Models.Hangfire;
using Masuit.Tools;
using Masuit.Tools.Logging;
using Masuit.Tools.NoSQL;
#if DEBUG
#endif
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
            RedisHelper.StringIncrement("Interview:ViewCount");
            return;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
#if !DEBUG
            if (Request.UserHostAddress != null && Request.UserHostAddress.IsDenyIpAddress())
            {
                Response.Write($"检测到您的IP（{Request.UserHostAddress}）异常，已被本站禁止访问，如有疑问，请联系站长！");
                BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
                {
                    IP = Request.UserHostAddress,
                    RequestUrl = Request.Url.ToString(),
                    Time = DateTime.Now
                }));
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
                if (times > 300)
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
        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}