using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Common;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Net;
using Masuit.Tools.NoSQL;
using Masuit.Tools.Security;
using Models.DTO;
using Models.Enum;
using Models.ViewModel;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    /// <summary>
    /// 基本父控制器
    /// </summary>
    [MyExceptionFilter, MyActionFilter]
    public class BaseController : Controller
    {
        public IUserInfoBll UserInfoBll { get; set; }

        public IMenuBll MenuBll { get; set; }

        public ILinksBll LinksBll { get; set; }

        public IContactsBll ContactsBll { get; set; }
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

        /// <summary>创建 JsonResult 对象，该对象使用指定 JSON 请求行为将指定对象序列化为 JavaScript 对象表示法 (JSON) 格式。</summary>
        /// <returns>将指定对象序列化为 JSON 格式的结果对象。</returns>
        /// <param name="data">要序列化的 JavaScript 对象图。</param>
        /// <param name="behavior">JSON 请求行为。</param>
        protected new JsonResult Json(object data, JsonRequestBehavior behavior)
        {
            return new JsonResult()
            {
                Data = data,
                ContentType = "application/json",
                ContentEncoding = Encoding.UTF8,
                JsonRequestBehavior = behavior,
                MaxJsonLength = Int32.MaxValue,
                RecursionLimit = Int32.MaxValue
            };
        }

        /// <summary>在调用操作方法前调用。</summary>
        /// <param name="filterContext">有关当前请求和操作的信息。</param>
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.HttpContext.Request.HttpMethod.Equals("GET", StringComparison.InvariantCultureIgnoreCase)) //get方式的多半是页面
            {
                UserInfoOutputDto user = filterContext.HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
#if DEBUG
                user = UserInfoBll.GetByUsername("masuit").Mapper<UserInfoOutputDto>();
                Session.SetByRedis(SessionKey.UserInfo, user);
#endif
                if (user == null && Request.Cookies.Count > 2) //执行自动登录
                {
                    string name = CookieHelper.GetCookieValue("username");
                    string pwd = CookieHelper.GetCookieValue("password")?.DesDecrypt(ConfigurationManager.AppSettings["BaiduAK"]);
                    var userInfo = UserInfoBll.Login(name, pwd);
                    if (userInfo != null)
                    {
                        CookieHelper.SetCookie("username", name, DateTime.Now.AddDays(7));
                        CookieHelper.SetCookie("password", CookieHelper.GetCookieValue("password"), DateTime.Now.AddDays(7));
                        Session.SetByRedis(SessionKey.UserInfo, userInfo);
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
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            if (filterContext.HttpContext.Request.HttpMethod.Equals("POST", StringComparison.InvariantCultureIgnoreCase) && filterContext.Result is ViewResult)
            {
                filterContext.Result = ResultData(null, false, "该URL仅支持Get请求方式", false, HttpStatusCode.MethodNotAllowed);
                return;
            }

            #region 准备页面数据模型

            ViewBag.menus = MenuBll.LoadEntitiesFromL2CacheNoTracking<MenuOutputDto>(m => m.Status == Status.Available).OrderBy(m => m.Sort).ToList(); //菜单
            PageFootViewModel model = new PageFootViewModel //页脚
            {
                Links = LinksBll.LoadPageEntitiesFromCacheNoTracking<int, LinksOutputDto>(1, 40, out int _, l => l.Status == Status.Available, l => l.Id, false, 1).ToList(),
                Contacts = ContactsBll.LoadEntitiesFromL2CacheNoTracking<int, ContactsOutputDto>(l => l.Status == Status.Available, l => l.Id, false).ToList()
            };
            ViewBag.Footer = model;

            #endregion
        }
    }
}