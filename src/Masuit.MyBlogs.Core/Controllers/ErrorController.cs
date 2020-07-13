using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.Tools;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Net;
using System.Web;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 错误页
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {
        public IUserInfoService UserInfoService { get; set; }

        /// <summary>
        /// 404
        /// </summary>
        /// <returns></returns>
        [Route("error"), Route("{*url}", Order = 99999), ResponseCache(Duration = 36000)]
        public ActionResult Index()
        {
            Response.StatusCode = 404;
            return Request.Method.Equals(HttpMethods.Get) ? (ActionResult)View() : Json(new
            {
                StatusCode = 404,
                Success = false,
                Message = "页面未找到！"
            });
        }

        /// <summary>
        /// 503
        /// </summary>
        /// <returns></returns>
        [Route("ServiceUnavailable")]
        public ActionResult ServiceUnavailable()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (feature != null)
            {
                string err;
                var req = HttpContext.Request;
                var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                var trueip = Request.Headers[AppConfig.TrueClientIPHeader].ToString();
                if (!string.IsNullOrEmpty(trueip) && ip != trueip)
                {
                    ip = trueip;
                }

                switch (feature.Error)
                {
                    case DbUpdateConcurrencyException ex:
                        err = $"数据库并发更新异常，更新表：{ex.Entries.Select(e => e.Metadata.Name)}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}\t{ex.InnerException?.Message}\t";
                        LogManager.Error(err, ex);
                        break;
                    case DbUpdateException ex:
                        err = $"数据库更新时异常，更新表：{ex.Entries.Select(e => e.Metadata.Name)}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}\t{ex.InnerException?.Message}\t";
                        LogManager.Error(err, ex);
                        break;
                    case AggregateException ex:
                        LogManager.Debug("↓↓↓" + ex.Message + "↓↓↓");
                        ex.Flatten().Handle(e =>
                        {
                            LogManager.Error($"异常源：{e.Source}，异常类型：{e.GetType().Name}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}\t", e);
                            return true;
                        });
                        break;
                    case NotFoundException ex:
                        Response.StatusCode = 404;
                        return Request.Method.Equals(HttpMethods.Get) ? (ActionResult)View("Index") : Json(new
                        {
                            StatusCode = 404,
                            Success = false,
                            ex.Message
                        });
                    case AccessDenyException _:
                        var (location, network) = IPAddress.Parse(ip).GetIPLocation();
                        var tips = Template.Create(CommonHelper.SystemSettings.GetOrAdd("AccessDenyTips", @"<h4>遇到了什么问题？</h4>
                <h4>基于主观因素考虑，您所在的地区暂时不允许访问本站，如有疑问，请联系站长！或者请联系站长开通本站的访问权限！</h4>")).Set("clientip", ip).Set(nameof(location), location).Set(nameof(network), network).Render();
                        Response.StatusCode = 403;
                        return View("AccessDeny", tips);
                    case TempDenyException _:
                        Response.StatusCode = 403;
                        return View("TempDeny");
                    default:
                        LogManager.Error($"异常源：{feature.Error.Source}，异常类型：{feature.Error.GetType().Name}，\n请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}\t", feature.Error);
                        break;
                }
            }

            Response.StatusCode = 503;
            return Request.Method.Equals(HttpMethods.Get) ? (ActionResult)View() : Json(new
            {
                StatusCode = 503,
                Success = false,
                Message = "服务器发生错误！"
            });
        }

        /// <summary>
        /// 网站升级中
        /// </summary>
        /// <returns></returns>
        [Route("ComingSoon"), ResponseCache(Duration = 360000)]
        public ActionResult ComingSoon()
        {
            return View();
        }

        /// <summary>
        /// 检查访问密码
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall, ResponseCache(Duration = 115, VaryByQueryKeys = new[] { "email", "token" })]
        public ActionResult CheckViewToken(string email, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return ResultData(null, false, "请输入访问密码！");
            }

            var s = RedisHelper.Get("token:" + email);
            if (!token.Equals(s))
            {
                return ResultData(null, false, "访问密码不正确！");
            }

            Response.Cookies.Append("Email", email, new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            Response.Cookies.Append("FullAccessToken", email.MDString3(AppConfig.BaiduAK), new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            return ResultData(null);
        }

        /// <summary>
        /// 检查授权邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall, ResponseCache(Duration = 115, VaryByQueryKeys = new[] { "email" })]
        public ActionResult GetViewToken(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.MatchEmail())
            {
                return ResultData(null, false, "请输入正确的邮箱！");
            }

            if (RedisHelper.Exists("get:" + email))
            {
                RedisHelper.Expire("get:" + email, 120);
                return ResultData(null, false, "发送频率限制，请在2分钟后重新尝试发送邮件！请检查你的邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！");
            }

            if (!UserInfoService.Any(b => b.Email == email))
            {
                return ResultData(null, false, "您目前没有权限访问这个链接，请联系站长开通访问权限！");
            }

            var token = SnowFlake.GetInstance().GetUniqueShortId(6);
            RedisHelper.Set("token:" + email, token, 86400);
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "博客访问验证码", $"{Request.Host}本次验证码是：<span style='color:red'>{token}</span>，有效期为24h，请按时使用！", email));
            RedisHelper.Set("get:" + email, token, 120);
            return ResultData(null);

        }

        /// <summary>
        /// 响应数据
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="success">响应状态</param>
        /// <param name="message">响应消息</param>
        /// <returns></returns>
        public ActionResult ResultData(object data, bool success = true, string message = "")
        {
            return Ok(new
            {
                Success = success,
                Message = message,
                Data = data
            });
        }
    }
}