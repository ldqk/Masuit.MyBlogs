using Microsoft.AspNetCore.Builder;

namespace Masuit.MyBlogs.Core.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFirewall(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirewallMiddleware>();
        }
        public static IApplicationBuilder UseException(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
        public static IApplicationBuilder UseRequestIntercept(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestInterceptMiddleware>();
        }
    }
}
