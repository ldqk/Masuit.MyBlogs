using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Web;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 授权验证过滤器
    /// </summary>
    public class MyAuthorizeAttribute : ActionFilterAttribute
    {
        /// <summary>在执行操作方法之前由 ASP.NET MVC 框架调用。</summary>
        /// <param name="filterContext">筛选器上下文。</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.Filters.Any(m => m.ToString().Contains(nameof(AllowAnonymousAttribute))))
            {
                return;
            }
#if !DEBUG
            UserInfoDto user = filterContext.HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
            if (user != null && user.IsAdmin)
            {
                return;
            }

            //先尝试自动登录
            if (filterContext.HttpContext.Request.Cookies.Any(x => x.Key == "username" || x.Key == "password"))
            {
                string name = filterContext.HttpContext.Request.Cookies["username"] ?? "";
                string pwd = filterContext.HttpContext.Request.Cookies["password"]?.DesDecrypt(AppConfig.BaiduAK) ?? "";

                var userInfo = (Startup.ServiceProvider.GetRequiredService<IUserInfoService>()).Login(name, pwd);
                if (userInfo != null)
                {
                    filterContext.HttpContext.Response.Cookies.Append("username", name, new CookieOptions()
                    {
                        Expires = DateTime.Now.AddDays(7),
                        SameSite = SameSiteMode.Lax
                    });
                    filterContext.HttpContext.Response.Cookies.Append("password", filterContext.HttpContext.Request.Cookies["password"], new CookieOptions()
                    {
                        Expires = DateTime.Now.AddDays(7),
                        SameSite = SameSiteMode.Lax
                    });
                    filterContext.HttpContext.Session.Set(SessionKey.UserInfo, userInfo);
                }
                else
                {
                    if (filterContext.HttpContext.Request.Method.ToLower().Equals("get"))
                    {
                        filterContext.Result = new RedirectResult("/passport/login?from=" + HttpUtility.UrlEncode(filterContext.HttpContext.Request.Path.ToString())?.Replace("#", "%23"));
                    }
                    else
                    {
                        filterContext.Result = new UnauthorizedObjectResult(new { StatusCode = 401, Success = false, IsLogin = false, Message = "未登录系统，请先登录！" });
                    }
                }
            }
            else
            {
                if (filterContext.HttpContext.Request.Method.ToLower().Equals("get"))
                {
                    filterContext.Result = new RedirectResult("/passport/login?from=" + HttpUtility.UrlEncode(filterContext.HttpContext.Request.Path.ToString()));
                }
                else
                {
                    filterContext.Result = new UnauthorizedObjectResult(new { StatusCode = 401, Success = false, IsLogin = false, Message = "未登录系统，请先登录！" });
                }
            }
#endif
        }
    }
}