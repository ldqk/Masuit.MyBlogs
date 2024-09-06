using FreeRedis;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Masuit.MyBlogs.Core.Extensions;

public class DistributedLockFilterAttribute : Attribute, IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var redis = context.HttpContext.RequestServices.GetRequiredService<IRedisClient>();
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        var action = context.RouteData.Values["action"]?.ToString() ?? "";
        var key = context.HttpContext.Request.Cookies["SessionID"] ?? context.HttpContext.Connection.RemoteIpAddress.ToString();
        var lockKey = $"{key}:{controller}_{action}";
        return redis.Lock(lockKey, () => next());
    }
}