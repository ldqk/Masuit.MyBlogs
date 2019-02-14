using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 管理页的父控制器
    /// </summary>
    [Authority, ApiExplorerSettings(IgnoreApi = true)]
    public class AdminController : Controller
    {
        /// <summary>
        /// UserInfoService
        /// </summary>
        public IUserInfoService UserInfoService { get; set; }

        /// <summary>
        /// 返回结果json
        /// </summary>
        /// <param name="data">响应数据</param>
        /// <param name="success">响应状态</param>
        /// <param name="message">响应消息</param>
        /// <param name="isLogin">登录状态</param>
        /// <returns></returns>
        public ActionResult ResultData(object data, bool success = true, string message = "", bool isLogin = true)
        {
            return Content(JsonConvert.SerializeObject(new
            {
                IsLogin = isLogin,
                Success = success,
                Message = message,
                Data = data
            }, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), "application/json", Encoding.UTF8);
        }

        /// <summary>
        /// 分页响应结果
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="total">总条数</param>
        /// <returns></returns>
        public ActionResult PageResult(object data, int pageCount, int total)
        {
            return Content(JsonConvert.SerializeObject(new PageDataModel(data, pageCount, total), new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            }), "application/json", Encoding.UTF8);
        }

        /// <summary>
        /// 性能计数器
        /// </summary>
        private Stopwatch Stopwatch { get; set; } = new Stopwatch();

        /// <summary>在调用操作方法前调用。</summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Stopwatch.Start();
            base.OnActionExecuting(filterContext);
        }

        /// <summary>在调用操作方法后调用。</summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (Stopwatch.Elapsed > TimeSpan.FromSeconds(5))
            {
                var req = filterContext.HttpContext.Request;
                LogManager.Info($"执行请求{req.Host}耗时{Stopwatch.ElapsedMilliseconds}ms，客户端用户代理：{req.Headers["User-Agent"]}，客户端IP：{filterContext.HttpContext.Connection.RemoteIpAddress.MapToIPv4()}");
            }
        }
    }
}