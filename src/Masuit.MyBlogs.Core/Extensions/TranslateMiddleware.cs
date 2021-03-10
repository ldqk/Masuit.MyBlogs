using Masuit.MyBlogs.Core.Common;
using Masuit.Tools;
using Masuit.Tools.Core;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 简繁转换拦截器
    /// </summary>
    public class TranslateMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next"></param>
        public TranslateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            string lang = context.Request.Query["lang"];
            lang ??= context.Request.Cookies["lang"];
            if (string.IsNullOrEmpty(lang))
            {
                if (context.Request.Location().Contains(new[] { "台湾", "香港", "澳门", "Taiwan", "TW", "HongKong", "HK" }))
                {
                    await Traditional(context);
                }
                else
                {
                    await _next(context);
                }
                return;
            }
            if (lang == "zh-cn")
            {
                await _next(context);
            }
            else
            {
                await Traditional(context);
            }
        }

        private async Task Traditional(HttpContext context)
        {
            //设置stream存放ResponseBody
            var responseOriginalBody = context.Response.Body;
            var memStream = new MemoryStream();
            context.Response.Body = memStream;

            // 执行其他中间件
            await _next(context);

            //处理执行其他中间件后的ResponseBody
            memStream.Position = 0;
            var responseReader = new StreamReader(memStream, Encoding.UTF8);
            var responseBody = await responseReader.ReadToEndAsync();
            memStream = new MemoryStream(Encoding.UTF8.GetBytes(responseBody.ToTraditional()));
            await memStream.CopyToAsync(responseOriginalBody);
            context.Response.Body = responseOriginalBody;
        }
    }
}