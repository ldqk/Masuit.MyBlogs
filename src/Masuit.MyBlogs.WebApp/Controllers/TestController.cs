using System;
using System.Web.Mvc;
using Common;
using Hangfire;
using Masuit.Tools.Html;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class TestController : Controller
    {
        [ValidateInput(false), Route("xss")]
        public ActionResult Xss(string text, bool clear = false)
        {
            ViewBag.Text = clear ? text.HtmlSantinizerStandard() : text;
            return View();
        }

        [Route("getip")]
        public ActionResult IP()
        {
            BackgroundJob.Enqueue(() => CommonHelper.SendMail("获取到IP" + Request.UserHostAddress, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "获取到IP" + Request.UserHostAddress, "550084490@qq.com"));
            return Redirect("/");
        }
    }
}