using Masuit.MyBlogs.Core.Extensions.Firewall;
using Microsoft.AspNetCore.Builder;

namespace Masuit.MyBlogs.Core.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestIntercept(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestInterceptMiddleware>();
        }
        public static IApplicationBuilder UseActivity(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ActivityMiddleware>();
        }
    }
}