using System;
using System.Text;
using System.Web.Mvc;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Models.ViewModel;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    /// <summary>
    /// 管理页的父控制器
    /// </summary>
    [Authority, ValidateInput(false)]
    public class AdminController : Controller
    {
        public IUserInfoBll UserInfoBll { get; set; }

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
    }
}