using Dispose.Scope;
using FreeRedis;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 管理页的父控制器
/// </summary>
[MyAuthorize, ApiExplorerSettings(IgnoreApi = true)]
public class AdminController : Controller
{
    /// <summary>
    /// UserInfoService
    /// </summary>
    public IUserInfoService UserInfoService { get; set; }

    public IRedisClient RedisHelper { get; set; }

    /// <summary>
    /// 返回结果json
    /// </summary>
    /// <param name="data">响应数据</param>
    /// <param name="success">响应状态</param>
    /// <param name="message">响应消息</param>
    /// <param name="isLogin">登录状态</param>
    /// <returns></returns>
    internal ActionResult ResultData(object data, bool success = true, string message = "", bool isLogin = true)
    {
        return Ok(new
        {
            IsLogin = isLogin,
            Success = success,
            Message = message,
            Data = data
        });
    }

    /// <summary>在调用操作方法前调用。</summary>
    /// <param name="context">有关当前请求和操作的信息。</param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        var user = context.HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
#if DEBUG
        user = UserInfoService.GetByUsername("masuit").ToDto();
        context.HttpContext.Session.Set(SessionKey.UserInfo, user);
#endif
        if (user == null && Request.Cookies.Any(x => x.Key == "username" || x.Key == "password")) //执行自动登录
        {
            string name = Request.Cookies["username"];
            string pwd = Request.Cookies["password"]?.DesDecrypt(AppConfig.BaiduAK);
            var userInfo = UserInfoService.Login(name, pwd);
            if (userInfo != null)
            {
                Response.Cookies.Append("username", name, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    SameSite = SameSiteMode.Lax
                });
                Response.Cookies.Append("password", Request.Cookies["password"], new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    SameSite = SameSiteMode.Lax
                });
                context.HttpContext.Session.Set(SessionKey.UserInfo, userInfo);
            }
        }
        if (ModelState.IsValid) return;
        var errmsgs = ModelState.SelectMany(kv => kv.Value.Errors.Select(e => e.ErrorMessage)).ToPooledListScope();
        for (var i = 0; i < errmsgs.Count; i++)
        {
            errmsgs[i] = i + 1 + ". " + errmsgs[i];
        }

        context.Result = ResultData(null, false, "数据校验失败，错误信息：" + string.Join(" | ", errmsgs));
    }
}