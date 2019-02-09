using Masuit.Tools.Core.Net;
using Masuit.Tools.NoSQL;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 请求拦截器
    /// </summary>
    public class RequestInterceptMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedisHelper _redisHelper;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        /// <param name="redisHelper"></param>
        public RequestInterceptMiddleware(RequestDelegate next, RedisHelper redisHelper)
        {
            _next = next;
            _redisHelper = redisHelper;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Session.TryGetValue(context.Connection.Id, out _))
            {
                context.Session.Set(context.Connection.Id, context.Connection.Id);
                _redisHelper.StringIncrement("Interview:ViewCount");
            }
            await _next.Invoke(context);
        }
    }
}