using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Masuit.Tools;
#if !DEBUG
using System.IO;
using Common;
#endif
using Masuit.Tools.Logging;

namespace Masuit.MyBlogs.WebApp.Models
{
    /// <summary>
    /// 自定义异常过滤器
    /// </summary>
    public class MyExceptionFilterAttribute : HandleErrorAttribute
    {
        /// <summary>在发生异常时调用。</summary>
        public override void OnException(ExceptionContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            string requestUrl = request.Url?.ToString();
#if !DEBUG
            if (request.UserHostAddress != null)
            {
                CommonHelper.IPErrorTimes.AddOrUpdate(request.UserHostAddress + request.UserAgent, 0, (s, i) => CommonHelper.IPErrorTimes[request.UserHostAddress + request.UserAgent] + 1);
                if (CommonHelper.IPErrorTimes[request.UserHostAddress + request.UserAgent] > 100)//同一IP错误请求100次即视为恶意请求
                {
                    CommonHelper.DenyIP = string.Join(",", (CommonHelper.DenyIP + "," + request.UserHostAddress).Split(',').Distinct());
                    File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "denyip.txt"), CommonHelper.DenyIP);
                    CommonHelper.IPErrorTimes[request.UserHostAddress + request.UserAgent] = 0;
                }
            }
#endif
            switch (filterContext.Exception)
            {
                case AggregateException ex:
                    LogManager.Debug("↓↓↓" + ex.Message + "↓↓↓");
                    ex.Handle(e =>
                    {
                        LogManager.Error($"异常源：{e.Source}，异常类型：{e.GetType().Name}，\n请求路径：{requestUrl}，客户端用户代理：{request.UserAgent}，客户端IP：{request.UserHostAddress}\t", e);
                        if (e.InnerException != null)
                        {
                            LogManager.Error("内部异常：", e.InnerException);
                        }
                        return true;
                    });
                    break;
                case HttpException ex:
                    LogManager.Error($"发生http请求异常\t异常源：{ex.Source}，\n请求路径：{requestUrl}，客户端用户代理：{request.UserAgent}，客户端IP：{request.UserHostAddress}\t错误代码：{ex.GetHttpCode()}", ex);
                    if (ex.InnerException != null)
                    {
                        LogManager.Error("内部异常：", ex.InnerException);
                    }
                    break;
                case DbEntityValidationException ex:
                    List<string> errmsgs = new List<string>();
                    var errors = ex.EntityValidationErrors.SelectMany(r => r.ValidationErrors).ToList();
                    LogManager.Debug($"发生数据模型校验错误异常\t异常源：{ex.Source}，\n请求路径：{requestUrl}，客户端用户代理：{request.UserAgent}，客户端IP：{request.UserHostAddress}", errors.ToJsonString());
                    if (filterContext.HttpContext.Request.HttpMethod.ToLower().Equals("get"))
                    {
                        filterContext.Result = new RedirectResult("/ServiceUnavailable"); //new ErrorController().ServiceUnavailable();
                    }
                    else
                    {
                        errors.ForEach(e => errmsgs.Add(e.ErrorMessage));
                        if (errmsgs.Count > 1)
                        {
                            for (var i = 0; i < errmsgs.Count; i++)
                            {
                                errmsgs[i] = i + 1 + ". " + errmsgs[i];
                            }
                        }
                        filterContext.Result = new JsonResult()
                        {
                            ContentEncoding = Encoding.UTF8,
                            ContentType = "application/json",
                            Data = new
                            {
                                Data = errors,
                                StatusCode = 500,
                                Success = false,
                                IsLogin = true,
                                Message = "数据校验失败，错误信息：" + string.Join(" | ", errmsgs)
                            },
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet
                        };
                    }
                    filterContext.ExceptionHandled = true; //设置异常已经处理
                    return;
                default:
                    LogManager.Error($"异常源：{filterContext.Exception.Source}，异常类型：{filterContext.Exception.GetType().Name}，\n请求路径：{requestUrl}，客户端用户代理：{request.UserAgent}，客户端IP：{request.UserHostAddress}\t", filterContext.Exception);
                    if (filterContext.Exception.InnerException != null)
                    {
                        LogManager.Error("内部异常：", filterContext.Exception.InnerException);
                    }
                    break;
            }

            //filterContext.HttpContext.Response.StatusCode = 503;
            if (filterContext.HttpContext.Request.HttpMethod.ToLower().Equals("get"))
            {
                filterContext.Result = new RedirectResult("/ServiceUnavailable"); //new ErrorController().ServiceUnavailable();
            }
            else
            {
                filterContext.Result = new JsonResult()
                {
                    ContentEncoding = Encoding.UTF8,
                    ContentType = "application/json",
                    Data = (new
                    {
                        StatusCode = 500,
                        Success = false,
                        Message = "服务器发生了内部错误！如有疑问，请联系网站管理员或开发者。"
                    }),
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            filterContext.ExceptionHandled = true; //设置异常已经处理
        }
    }
}