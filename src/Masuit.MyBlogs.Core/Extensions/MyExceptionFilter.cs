using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Web;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class MyExceptionFilter : ExceptionFilterAttribute
    {
        /// <inheritdoc />
        public override void OnException(ExceptionContext context)
        {
            base.OnException(context);
            string err;
            var req = context.HttpContext.Request;
            switch (context.Exception)
            {
                case DbUpdateConcurrencyException ex:
                    err = $"异常源：{ex.Source}，异常类型：{ex.GetType().Name}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(req.Path)}，客户端用户代理：{req.Headers["User-Agent"]}，客户端IP：{context.HttpContext.Connection.RemoteIpAddress}\t{ex.InnerException?.Message}\t";
                    LogManager.Error(err, ex);
                    break;
                case DbUpdateException ex:
                    err = $"异常源：{ex.Source}，异常类型：{ex.GetType().Name}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(req.Path)}，客户端用户代理：{req.Headers["User-Agent"]}，客户端IP：{context.HttpContext.Connection.RemoteIpAddress}\t{ex?.InnerException?.Message}\t";
                    LogManager.Error(err, ex);
                    break;
                case AggregateException ex:
                    LogManager.Debug("↓↓↓" + ex.Message + "↓↓↓");
                    ex.Handle(e =>
                    {
                        LogManager.Error($"异常源：{e.Source}，异常类型：{e.GetType().Name}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(req.Path)}，客户端用户代理：{req.Headers["User-Agent"]}，客户端IP：{context.HttpContext.Connection.RemoteIpAddress}\t", e);
                        return true;
                    });
                    break;
                case NotFoundException _:
                    context.Result = new RedirectToActionResult("Index", "Error", new { });
                    context.ExceptionHandled = true;
                    return;
                default:
                    LogManager.Error($"异常源：{context.Exception.Source}，异常类型：{context.Exception.GetType().Name}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(req.Path)}，客户端用户代理：{req.Headers["User-Agent"]}，客户端IP：{context.HttpContext.Connection.RemoteIpAddress}\t", context.Exception);
                    break;
            }
#if !DEBUG
            context.Result = new RedirectToActionResult("ServiceUnavailable", "Error", null);
            context.ExceptionHandled = true; 
#endif
        }
    }
}