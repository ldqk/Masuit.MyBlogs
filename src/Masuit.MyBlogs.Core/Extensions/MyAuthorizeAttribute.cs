using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Web;

namespace Masuit.MyBlogs.Core.Extensions;

/// <summary>
/// 授权验证过滤器
/// </summary>
public sealed class MyAuthorizeAttribute : ActionFilterAttribute
{
    /// <summary>在执行操作方法之前由 ASP.NET MVC 框架调用。</summary>
    /// <param name="filterContext">筛选器上下文。</param>
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
#if DEBUG
        return;
#endif
        if (filterContext.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            return;
        }
        var user = filterContext.HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
        if (user?.IsAdmin == true)
        {
            return;
        }

        if (filterContext.HttpContext.Request.Method.Equals(HttpMethods.Get))
        {
            filterContext.Result = new RedirectResult("/passport/login?from=" + HttpUtility.UrlEncode(filterContext.HttpContext.Request.Path.ToString()));
        }
        else
        {
            filterContext.Result = new UnauthorizedObjectResult(new { StatusCode = 401, Success = false, IsLogin = false, Message = "未登录系统，请先登录！" });
        }
    }
}