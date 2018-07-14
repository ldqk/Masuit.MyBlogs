using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;
using Masuit.Tools;
using Masuit.Tools.Logging;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Models
{
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>Raises the exception event.</summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            LogManager.Error(actionExecutedContext.Exception);
            var request = actionExecutedContext.Request;
            string requestUrl = request.RequestUri.ToString();
            switch (actionExecutedContext.Exception)
            {
                case AggregateException ex:
                    LogManager.Debug("↓↓↓" + ex.Message + "↓↓↓");
                    ex.Handle(e =>
                    {
                        LogManager.Error($"异常源：{e.Source}，异常类型：{e.GetType().Name}，\n请求路径：{requestUrl}，客户端用户代理：{request.Headers.UserAgent}\t", e);
                        if (e.InnerException != null)
                        {
                            LogManager.Error("内部异常：", e.InnerException);
                        }
                        return true;
                    });
                    break;
                case HttpException ex:
                    LogManager.Error($"发生http请求异常\t异常源：{ex.Source}，\n请求路径：{requestUrl}\t错误代码：{ex.GetHttpCode()}，客户端用户代理：{request.Headers.UserAgent}\t", ex);
                    if (ex.InnerException != null)
                    {
                        LogManager.Error("内部异常：", ex.InnerException);
                    }
                    break;
                case DbEntityValidationException ex:
                    List<string> errmsgs = new List<string>();
                    var errors = ex.EntityValidationErrors.SelectMany(r => r.ValidationErrors).ToList();
                    LogManager.Debug($"发生数据模型校验错误异常\t异常源：{ex.Source}，\n请求路径：{requestUrl}，客户端用户代理：{request.Headers.UserAgent}\t", errors.ToJsonString());
                    errors.ForEach(e => errmsgs.Add(e.ErrorMessage));
                    if (errmsgs.Count > 1)
                    {
                        for (var i = 0; i < errmsgs.Count; i++)
                        {
                            errmsgs[i] = i + 1 + ". " + errmsgs[i];
                        }
                    }
                    actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new
                        {
                            Data = errors,
                            StatusCode = 500,
                            Success = false,
                            IsLogin = true,
                            Message = "数据校验失败，错误信息：" + string.Join(" | ", errmsgs)
                        }))
                    };
                    return;
                default:
                    LogManager.Error($"异常源：{actionExecutedContext.Exception.Source}，异常类型：{actionExecutedContext.Exception.GetType().Name}，\n请求路径：{requestUrl}，客户端用户代理：{request.Headers.UserAgent}\t", actionExecutedContext.Exception);
                    if (actionExecutedContext.Exception.InnerException != null)
                    {
                        LogManager.Error("内部异常：", actionExecutedContext.Exception.InnerException);
                    }
                    break;
            }
            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    StatusCode = 500,
                    Success = false,
                    Message = "服务器发生错误！"
                }))
            };
        }
    }
}