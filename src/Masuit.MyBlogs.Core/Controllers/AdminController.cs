using Dispose.Scope;
using FreeRedis;
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

    public IAccessViewTicketService AccessViewTicketService { get; set; }

    public ILoginSessionService LoginSessionService { get; set; }

    public IRedisClient RedisHelper { get; set; }

    protected UserInfoDto CurrentUser => HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo) ?? new UserInfoDto();

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
#if DEBUG
        var user = context.HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
        if (user?.IsAdmin != true)
        {
            user = UserInfoService.GetByUsername("masuit")?.ToDto();
            if (user != null)
            {
                context.HttpContext.Session.Set(SessionKey.UserInfo, user);
            }
        }
#endif
        if (ModelState.IsValid) return;
        var errmsgs = ModelState.SelectMany(kv => kv.Value.Errors.Select(e => e.ErrorMessage)).ToPooledListScope();
        for (var i = 0; i < errmsgs.Count; i++)
        {
            errmsgs[i] = i + 1 + ". " + errmsgs[i];
        }

        context.Result = ResultData(null, false, "数据校验失败，错误信息：" + string.Join(" | ", errmsgs));
    }
}