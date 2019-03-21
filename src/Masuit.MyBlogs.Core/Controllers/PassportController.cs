using Common;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 登录授权
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PassportController : Controller
    {
        /// <summary>
        /// 用户
        /// </summary>
        public IUserInfoService UserInfoService { get; set; }
        /// <summary>
        /// 登录记录
        /// </summary>
        public ILoginRecordService LoginRecordService { get; set; }

        private readonly IHostingEnvironment _env;

        /// <summary>
        /// 登录授权
        /// </summary>
        /// <param name="userInfoService"></param>
        /// <param name="loginRecordService"></param>
        /// <param name="env"></param>
        public PassportController(IUserInfoService userInfoService, ILoginRecordService loginRecordService, IHostingEnvironment env)
        {
            UserInfoService = userInfoService;
            LoginRecordService = loginRecordService;
            _env = env;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isTrue"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ActionResult ResultData(object data, bool isTrue = true, string message = "")
        {
            return Json(new
            {
                Success = isTrue,
                Message = message,
                Data = data
            });
        }

        /// <summary>
        /// 登录页
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            string from = Request.Query["from"];
            if (!string.IsNullOrEmpty(from))
            {
                from = HttpUtility.UrlDecode(from);
                Response.Cookies.Append("refer", from);
            }
            if (HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) != null)
            {
                if (string.IsNullOrEmpty(from))
                {
                    return RedirectToAction("Index", "Home");
                }
                return Redirect(from);
            }
            if (Request.Cookies.Count > 2)
            {
                string name = Request.Cookies["username"];
                string pwd = Request.Cookies["password"]?.DesDecrypt(AppConfig.BaiduAK);
                var userInfo = UserInfoService.Login(name, pwd);
                if (userInfo != null)
                {
                    Response.Cookies.Append("username", name, new CookieOptions() { Expires = DateTime.Now.AddDays(7) });
                    Response.Cookies.Append("password", Request.Cookies["password"], new CookieOptions() { Expires = DateTime.Now.AddDays(7) });
                    HttpContext.Session.Set(SessionKey.UserInfo, userInfo);
                    HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.LoginRecord), "default", userInfo, HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(), LoginType.Default);
                    if (string.IsNullOrEmpty(from))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    return Redirect(from);
                }
            }
            return View();
        }

        /// <summary>
        /// 登陆检查
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="valid"></param>
        /// <param name="remem"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string valid, string remem)
        {
            string validSession = HttpContext.Session.Get<string>("valid") ?? string.Empty; //将验证码从Session中取出来，用于登录验证比较
            if (string.IsNullOrEmpty(validSession) || !valid.Trim().Equals(validSession, StringComparison.InvariantCultureIgnoreCase))
            {
                return ResultData(null, false, "验证码错误");
            }
            HttpContext.Session.Remove("valid"); //验证成功就销毁验证码Session，非常重要
            if (string.IsNullOrEmpty(username.Trim()) || string.IsNullOrEmpty(password.Trim()))
            {
                return ResultData(null, false, "用户名或密码不能为空");
            }
            var userInfo = UserInfoService.Login(username, password);
            if (userInfo != null)
            {
                HttpContext.Session.Set(SessionKey.UserInfo, userInfo);
                if (remem.Trim().Contains(new[] { "on", "true" })) //是否记住登录
                {
                    Response.Cookies.Append("username", HttpUtility.UrlEncode(username.Trim()), new CookieOptions() { Expires = DateTime.Now.AddDays(7) });
                    Response.Cookies.Append("password", password.Trim().DesEncrypt(AppConfig.BaiduAK), new CookieOptions() { Expires = DateTime.Now.AddDays(7) });
                }
                HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.LoginRecord), "default", userInfo, HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(), LoginType.Default);
                string refer = Request.Cookies["refer"];
                return ResultData(null, true, string.IsNullOrEmpty(refer) ? "/" : refer);
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
            HttpContext.Session.Set("valid", code); //将验证码生成到Session中
            var buffer = HttpContext.CreateValidateGraphic(code);
            return this.ResumeFile(buffer, "image/jpeg");
        }

        /// <summary>
        /// 检查验证码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckValidateCode(string code)
        {
            string validSession = HttpContext.Session.Get<string>("valid");
            if (string.IsNullOrEmpty(validSession) || !code.Trim().Equals(validSession, StringComparison.InvariantCultureIgnoreCase))
            {
                return ResultData(null, false, "验证码错误");
            }
            return ResultData(null, false, "验证码正确");
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetUserInfo()
        {
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
            if (_env.IsDevelopment())
            {
                user = UserInfoService.GetByUsername("masuit").Mapper<UserInfoOutputDto>();
            }
            return ResultData(user);
        }

        /// <summary>
        /// 注销登录
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            HttpContext.Session.Remove(SessionKey.UserInfo);
            Response.Cookies.Delete("username");
            Response.Cookies.Delete("password");
            HttpContext.Session.Clear();
            return Request.Method.ToLower().Equals("get") ? RedirectToAction("Index", "Home") : ResultData(null, message: "注销成功！");
        }
    }
}