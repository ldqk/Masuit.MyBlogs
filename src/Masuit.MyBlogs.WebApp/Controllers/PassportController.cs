using System;
using System.Configuration;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Common;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.MyBlogs.WebApp.Models.Hangfire;
using Masuit.Tools;
using Masuit.Tools.Net;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Models.DTO;
using Models.Enum;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class PassportController : Controller
    {
        public IUserInfoBll UserInfoBll { get; set; }
        public ILoginRecordBll LoginRecordBll { get; set; }

        public PassportController(IUserInfoBll userInfoBll, ILoginRecordBll loginRecordBll)
        {
            UserInfoBll = userInfoBll;
            LoginRecordBll = loginRecordBll;
        }

        public ActionResult ResultData(object data, bool isTrue = true, string message = "")
        {
            return Content(JsonConvert.SerializeObject(new
            {
                Success = isTrue,
                Message = message,
                Data = data
            }, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            }), "application/json");
        }

        public ActionResult Login()
        {
            string from = Request["from"];
            if (!string.IsNullOrEmpty(from))
            {
                from = Server.UrlDecode(from);
                CookieHelper.SetCookie("refer", from);
            }
            if (Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) != null)
            {
                if (string.IsNullOrEmpty(from))
                {
                    return RedirectToAction("Index", "Home");
                }
                return Redirect(from);
            }
            if (Request.Cookies.Count > 2)
            {
                string name = CookieHelper.GetCookieValue("username");
                string pwd = CookieHelper.GetCookieValue("password")?.DesDecrypt(ConfigurationManager.AppSettings["BaiduAK"]);
                var userInfo = UserInfoBll.Login(name, pwd);
                if (userInfo != null)
                {
                    CookieHelper.SetCookie("username", name, DateTime.Now.AddDays(7));
                    CookieHelper.SetCookie("password", CookieHelper.GetCookieValue("password"), DateTime.Now.AddDays(7));
                    Session.SetByRedis(SessionKey.UserInfo, userInfo);
                    HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.LoginRecord), "default", userInfo, Request.UserHostAddress, LoginType.Default);
                    if (string.IsNullOrEmpty(from))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    return Redirect(from);
                }
            }
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string valid, string remem)
        {
            string validSession = Session.GetByRedis<string>("valid") ?? String.Empty; //将验证码从Session中取出来，用于登录验证比较
            if (String.IsNullOrEmpty(validSession) || !valid.Trim().Equals(validSession, StringComparison.InvariantCultureIgnoreCase))
            {
                return ResultData(null, false, "验证码错误");
            }
            Session.RemoveByRedis("valid"); //验证成功就销毁验证码Session，非常重要
            if (String.IsNullOrEmpty(username.Trim()) || String.IsNullOrEmpty(password.Trim()))
            {
                return ResultData(null, false, "用户名或密码不能为空");
            }
            var userInfo = UserInfoBll.Login(username, password);
            if (userInfo != null)
            {
                Session.SetByRedis(SessionKey.UserInfo, userInfo);
                if (remem.Trim().Contains(new[] { "on", "true" })) //是否记住登录
                {
                    HttpCookie userCookie = new HttpCookie("username", Server.UrlEncode(username.Trim()));
                    Response.Cookies.Add(userCookie);
                    userCookie.Expires = DateTime.Now.AddDays(7);
                    HttpCookie passCookie = new HttpCookie("password", password.Trim().DesEncrypt(ConfigurationManager.AppSettings["BaiduAK"]))
                    {
                        Expires = DateTime.Now.AddDays(7)
                    };
                    Response.Cookies.Add(passCookie);
                }
                HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.LoginRecord), "default", userInfo, Request.UserHostAddress, LoginType.Default);
                string refer = CookieHelper.GetCookieValue("refer");
                if (string.IsNullOrEmpty(refer))
                {
                    return ResultData(null, true, "/");
                }
                return ResultData(null, true, refer);
            }
            return ResultData(null, false, "用户名或密码错误");
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateCode()
        {
            string code = Tools.Strings.ValidateCode.CreateValidateCode(6);
            Session.SetByRedis("valid", code); //将验证码生成到Session中
            System.Web.HttpContext.Current.CreateValidateGraphic(code);
            Response.ContentType = "image/jpeg";
            return File(Encoding.UTF8.GetBytes(code), "image/jpeg");
        }

        /// <summary>
        /// 检查验证码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckValidateCode(string code)
        {
            string validSession = Session.GetByRedis<string>("valid");
            if (String.IsNullOrEmpty(validSession) || !code.Trim().Equals(validSession, StringComparison.InvariantCultureIgnoreCase))
            {
                return ResultData(null, false, "验证码错误");
            }
            return ResultData(null, false, "验证码正确");
        }

        public ActionResult GetUserInfo()
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
#if DEBUG
            user = UserInfoBll.GetByUsername("masuit").Mapper<UserInfoOutputDto>();
#endif
            return ResultData(user);
        }

        public ActionResult Logout()
        {
            Session.RemoveByRedis(SessionKey.UserInfo);
            CookieHelper.SetCookie("username", String.Empty, DateTime.Now.AddDays(-1));
            CookieHelper.SetCookie("password", String.Empty, DateTime.Now.AddDays(-1));
            Session.Abandon();
            if (Request.HttpMethod.ToLower().Equals("get"))
            {
                return RedirectToAction("Index", "Home");
            }
            return ResultData(null, message: "注销成功！");
        }
    }
}