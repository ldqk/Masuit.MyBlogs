using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Masuit.MyBlogs.Core.Extensions
{
    /// <summary>
    /// 异常拦截中间件
    /// </summary>
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// 异常拦截中间件
        /// </summary>
        /// <param name="next"></param>
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// 执行调用
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var err = $"异常源：{ex.Source}，异常类型：{ex.GetType().Name}，\n请求路径：{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}，请求参数：{HttpUtility.UrlDecode(context.Request.Body.ToByteArray(), Encoding.UTF8)}，客户端用户代理：{context.Request.Headers["User-Agent"]}，客户端IP：{context.Connection.RemoteIpAddress}\t{ex.InnerException?.Message}\t";
                LogManager.Error(err, ex);
                await RedirectError(context);
            }
            catch (DbUpdateException ex)
            {
                var err = $"异常源：{ex.Source}，异常类型：{ex.GetType().Name}，\n请求路径：{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}，请求参数：{HttpUtility.UrlDecode(context.Request.Body.ToByteArray(), Encoding.UTF8)}，客户端用户代理：{context.Request.Headers["User-Agent"]}，客户端IP：{context.Connection.RemoteIpAddress}\t{ex?.InnerException?.Message}\t";
                LogManager.Error(err, ex);
                await RedirectError(context);
            }
            catch (AggregateException ex)
            {
                LogManager.Debug("↓↓↓" + ex.Message + "↓↓↓");
                ex.Handle(e =>
                {
                    LogManager.Error($"异常源：{e.Source}，异常类型：{e.GetType().Name}，\n请求路径：{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}，请求参数：{HttpUtility.UrlDecode(context.Request.Body.ToByteArray(), Encoding.UTF8)}，客户端用户代理：{context.Request.Headers["User-Agent"]}，客户端IP：{context.Connection.RemoteIpAddress}\t", e);
                    return true;
                });
                await RedirectError(context);
            }
            catch (Exception ex)
            {
                //LogManager.Error(ex);
                LogManager.Error($"异常源：{ex.Source}，异常类型：{ex.GetType().Name}，\n请求路径：{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}，请求参数：{HttpUtility.UrlDecode(context.Request.Body.ToByteArray(), Encoding.UTF8)}，客户端用户代理：{context.Request.Headers["User-Agent"]}，客户端IP：{context.Connection.RemoteIpAddress}\t", ex);
                await RedirectError(context);
            }
        }

        private static async Task RedirectError(HttpContext context)
        {
            switch (context.Request.Method.ToLower())
            {
                case "get":
                    context.Response.Redirect("/ServiceUnavailable");
                    break;
                default:
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        StatusCode = 503,
                        Success = false,
                        Message = "服务器发生错误！"
                    }));
                    break;
            }
        }
    }
}