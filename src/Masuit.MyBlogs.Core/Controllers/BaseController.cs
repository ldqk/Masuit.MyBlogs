using AutoMapper;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 基本父控制器
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true), ServiceFilter(typeof(FirewallAttribute))]
    public class BaseController : Controller, IResultFilter
    {
        /// <summary>
        /// UserInfoService
        /// </summary>
        public IUserInfoService UserInfoService { get; set; }

        /// <summary>
        /// MenuService
        /// </summary>
        public IMenuService MenuService { get; set; }

        /// <summary>
        /// LinksService
        /// </summary>
        public ILinksService LinksService { get; set; }

        public IAdvertisementService AdsService { get; set; }

        public UserInfoOutputDto CurrentUser => HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();

        /// <summary>
        /// 客户端的真实IP
        /// </summary>
        public string ClientIP => HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        public IMapper Mapper { get; set; }
        public MapperConfiguration MapperConfig { get; set; }
        public Stopwatch Stopwatch { get; } = new Stopwatch();
        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="success">响应状态</param>
        /// <param name="message">响应消息</param>
        /// <param name="isLogin">登录状态</param>
        /// <param name="code">http响应码</param>
        /// <returns></returns>
        public ActionResult ResultData(object data, bool success = true, string message = "", bool isLogin = true, HttpStatusCode code = HttpStatusCode.OK)
        {
            return Ok(new
            {
                IsLogin = isLogin,
                Success = success,
                Message = message,
                Data = data,
                code
            });
        }

        /// <summary>
        /// 分页响应数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="total">总条数</param>
        /// <returns></returns>
        public ActionResult PageResult(object data, int pageCount, int total)
        {
            return Ok(new PageDataModel(data, pageCount, total));
        }

        /// <summary>在调用操作方法前调用。</summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Stopwatch.Start();
            base.OnActionExecuting(filterContext);
            var user = filterContext.HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
#if DEBUG
            user = UserInfoService.GetByUsername("masuit").Mapper<UserInfoOutputDto>();
            filterContext.HttpContext.Session.Set(SessionKey.UserInfo, user);
#endif
            if (CommonHelper.SystemSettings.GetOrAdd("CloseSite", "false") == "true" && user?.IsAdmin != true)
            {
                filterContext.Result = RedirectToAction("ComingSoon", "Error");
            }

            if (Request.Method.Equals(HttpMethods.Post) && CommonHelper.SystemSettings.GetOrAdd("DataReadonly", "false") == "true" && !filterContext.Filters.Any(m => m.ToString().Contains(nameof(MyAuthorizeAttribute))))
            {
                filterContext.Result = ResultData("网站当前处于数据写保护状态，无法提交任何数据，如有疑问请联系网站管理员！", false, "网站当前处于数据写保护状态，无法提交任何数据，如有疑问请联系网站管理员！", user != null, HttpStatusCode.BadRequest);
            }

            if (user == null && Request.Cookies.Any(x => x.Key == "username" || x.Key == "password")) //执行自动登录
            {
                string name = Request.Cookies["username"];
                string pwd = Request.Cookies["password"]?.DesDecrypt(AppConfig.BaiduAK);
                var userInfo = UserInfoService.Login(name, pwd);
                if (userInfo != null)
                {
                    Response.Cookies.Append("username", name, new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7)
                    });
                    Response.Cookies.Append("password", Request.Cookies["password"], new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7)
                    });
                    filterContext.HttpContext.Session.Set(SessionKey.UserInfo, userInfo);
                }
            }

            if (ModelState.IsValid) return;
            var errmsgs = ModelState.SelectMany(kv => kv.Value.Errors.Select(e => e.ErrorMessage)).ToList();
            if (errmsgs.Any())
            {
                for (var i = 0; i < errmsgs.Count; i++)
                {
                    errmsgs[i] = i + 1 + ". " + errmsgs[i];
                }
            }

            filterContext.Result = ResultData(errmsgs, false, "数据校验失败，错误信息：" + errmsgs.Join(" | "), user != null, HttpStatusCode.BadRequest);
        }

        /// <summary>在调用操作方法后调用。</summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (filterContext.HttpContext.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                if (filterContext.Result is ViewResult)
                {
                    filterContext.Result = ResultData(null, false, "该URL仅支持Get请求方式", false, HttpStatusCode.MethodNotAllowed);
                }
                return;
            }

            #region 准备页面数据模型

            ViewBag.menus = MenuService.GetQueryFromCache<MenuOutputDto>(m => m.Status == Status.Available).OrderBy(m => m.Sort).ToList(); //菜单
            var model = new PageFootViewModel //页脚
            {
                Links = LinksService.GetQueryFromCache<LinksOutputDto>(l => l.Status == Status.Available).OrderByDescending(l => l.Recommend).ThenByDescending(l => l.Weight).ThenByDescending(l => new Random().Next()).Take(40).ToList()
            };
            ViewBag.Footer = model;

            #endregion

            ViewData["ActionElapsed"] = Stopwatch.ElapsedMilliseconds + "ms";
            if (Stopwatch.ElapsedMilliseconds > 5000)
            {
                LogManager.Debug($"请求路径：{Request.Scheme}://{Request.Host}{HttpUtility.UrlDecode(Request.Path)}执行耗时{Stopwatch.ElapsedMilliseconds}ms");
            }
        }

        /// <summary>Called after the action result executes.</summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ResultExecutedContext" />.</param>
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        /// <summary>Called before the action result executes.</summary>
        /// <param name="context">The <see cref="T:Microsoft.AspNetCore.Mvc.Filters.ResultExecutingContext" />.</param>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            Stopwatch.Restart();
            ViewData["ResultWatch"] = Stopwatch;
        }
    }
}