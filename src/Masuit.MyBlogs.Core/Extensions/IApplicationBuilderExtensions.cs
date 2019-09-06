using Microsoft.AspNetCore.Builder;

namespace Masuit.MyBlogs.Core.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestIntercept(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestInterceptMiddleware>();
        }
    }
}