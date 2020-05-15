using Masuit.MyBlogs.Core.Common;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 请求拦截器
    /// </summary>
    public class ActivityMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        public ActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var req = context.Request;
            var share = req.Query["share"].ToString();
            if (req.IsRobot() || string.IsNullOrEmpty(share) || share == req.Cookies["ShareCode"])
            {
                await _next.Invoke(context);
                return;
            }

            var mail = share.AESDecrypt();
            if (string.IsNullOrEmpty(mail))
            {
                await _next.Invoke(context);
                return;
            }

            var ip = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
            RedisHelper.SAddAsync("Share:" + mail, ip).ContinueWith(task => RedisHelper.Expire("Share:" + mail, TimeSpan.FromDays(8)));
            //var query = req.Query.Where(x => x.Key != "share").Select(x => x.Key + "=" + x.Value).Join("&");
            //context.Response.Redirect((req.Path + "?" + query).Trim('?'));
        }
    }
}