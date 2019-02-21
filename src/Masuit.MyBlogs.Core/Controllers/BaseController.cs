using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Masuit.Tools.NoSQL;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

#if DEBUG
using Common;
#endif

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 基本父控制器
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class BaseController : Controller
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

        /// <summary>
        /// ContactsService
        /// </summary>
        public IContactsService ContactsService { get; set; }

        /// <summary>
        /// RedisHelper
        /// </summary>
        public RedisHelper RedisHelper { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="success">响应状态</param>
        /// <param name="message">响应消息</param>
        /// <param name="isLogin">登录状态</param>
        /// <param name="code">http响应码</param>
        /// <returns></returns>
        public ContentResult ResultData(object data, bool success = true, string message = "", bool isLogin = true, HttpStatusCode code = HttpStatusCode.OK)
        {
            return Content(JsonConvert.SerializeObject(new
            {
                IsLogin = isLogin,
                Success = success,
                Message = message,
                Data = data,
                code
            }, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }), "application/json", Encoding.UTF8);
        }

        /// <summary>
        /// 分页响应数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="pageCount">总页数</param>
        /// <param name="total">总条数</param>
        /// <returns></returns>
        public ContentResult PageResult(object data, int pageCount, int total)
        {
            return Content(JsonConvert.SerializeObject(new PageDataModel(data, pageCount, total), new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            }), "application/json", Encoding.UTF8);
        }

        /// <summary>在调用操作方法前调用。</summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.HttpContext.Request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase)) //get方式的多半是页面
            {
                UserInfoOutputDto user = filterContext.HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
#if DEBUG
                user = UserInfoService.GetByUsername("masuit").Mapper<UserInfoOutputDto>();
                filterContext.HttpContext.Session.SetByRedis(SessionKey.UserInfo, user);
#endif
                if (user == null && Request.Cookies.Count > 2) //执行自动登录
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
                        filterContext.HttpContext.Session.SetByRedis(SessionKey.UserInfo, userInfo);
                    }
                }
            }
            else
            {
                if (ModelState.IsValid) return;
                List<string> errmsgs = new List<string>();
                ModelState.ForEach(kv => kv.Value.Errors.ForEach(error => errmsgs.Add(error.ErrorMessage)));
                if (errmsgs.Count > 1)
                {
                    for (var i = 0; i < errmsgs.Count; i++)
                    {
                        errmsgs[i] = i + 1 + ". " + errmsgs[i];
                    }
                }
                filterContext.Result = ResultData(errmsgs, false, "数据校验失败，错误信息：" + string.Join(" | ", errmsgs), true, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>在调用操作方法后调用。</summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (filterContext.HttpContext.Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) && filterContext.Result is ViewResult)
            {
                filterContext.Result = ResultData(null, false, "该URL仅支持Get请求方式", false, HttpStatusCode.MethodNotAllowed);
                return;
            }

            #region 准备页面数据模型

            ViewBag.menus = MenuService.LoadEntitiesFromL2Cache<MenuOutputDto>(m => m.Status == Status.Available).OrderBy(m => m.Sort).ToList(); //菜单
            PageFootViewModel model = new PageFootViewModel //页脚
            {
                Links = LinksService.LoadPageEntitiesFromL2Cache<bool, LinksOutputDto>(1, 40, out int _, l => l.Status == Status.Available, l => l.Recommend, false).ToList(),
                Contacts = ContactsService.LoadEntitiesFromL2Cache<int, ContactsOutputDto>(l => l.Status == Status.Available, l => l.Id, false).ToList()
            };
            ViewBag.Footer = model;

            #endregion
        }
    }
}