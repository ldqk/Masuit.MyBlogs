using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// <summary>
        /// 404
        /// </summary>
        /// <returns></returns>
        [Route("error"), Route("{*url}", Order = 99999), ResponseCache(Duration = 36000)]
        public ActionResult Index()
        {
            Response.StatusCode = 404;
            string accept = Request.Headers[HeaderNames.Accept];
            return true switch
            {
                _ when accept.StartsWith("image") => File("/Assets/images/404/4044.jpg", ContentType.Jpeg),
                _ when accept.StartsWith("application/json") => Json(new
                {
                    StatusCode = 404,
                    Success = false,
                    Message = "页面未找到！"
                }),
                _ => View()
            };
        }

        /// <summary>
        /// 503
        /// </summary>
        /// <returns></returns>
        [Route("ServiceUnavailable")]
        public async Task<ActionResult> ServiceUnavailable()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (feature != null)
            {
                string err;
                var req = HttpContext.Request;
                var ip = HttpContext.Connection.RemoteIpAddress;
                req.EnableBuffering();
                req.Body.Seek(0, SeekOrigin.Begin);
                using var sr = new StreamReader(req.Body, Encoding.UTF8, false);
                var body = await sr.ReadToEndAsync();
                body = HttpUtility.UrlDecode(body);
                req.Body.Position = 0;
                switch (feature.Error)
                {
                    case DbUpdateConcurrencyException ex:
                        err = $"数据库并发更新异常，更新表：{ex.Entries.Select(e => e.Metadata.Name)}，请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}{req.QueryString}，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}\t{ex.InnerException?.Message}，请求参数：\n{body}\n堆栈信息：";
                        LogManager.Error(err, ex);
                        break;
                    case DbUpdateException ex:
                        err = $"数据库更新时异常，更新表：{ex.Entries.Select(e => e.Metadata.Name)}，请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}{req.QueryString} ，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}\t{ex.InnerException?.Message}，请求参数：\n{body}\n堆栈信息：";
                        LogManager.Error(err, ex);
                        break;
                    case AggregateException ex:
                        LogManager.Debug("↓↓↓" + ex.Message + "↓↓↓");
                        ex.Flatten().Handle(e =>
                        {
                            LogManager.Error($"异常源：{e.Source}，异常类型：{e.GetType().Name}，请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}{req.QueryString} ，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}\t", e);
                            return true;
                        });
                        if (!string.IsNullOrEmpty(body))
                        {
                            LogManager.Debug("↑↑↑请求参数：\n" + body);
                        }
                        break;
                    case NotFoundException:
                        return RedirectToAction("Index");
                    case AccessDenyException:
                        var entry = ip.GetIPLocation();
                        var tips = Template.Create(CommonHelper.SystemSettings.GetOrAdd("AccessDenyTips", @"<h4>遇到了什么问题？</h4>
                <h4>基于主观因素考虑，您所在的地区暂时不允许访问本站，如有疑问，请联系站长！或者请联系站长开通本站的访问权限！</h4>")).Set("clientip", ip.ToString()).Set("location", entry.Location).Set("network", entry.Network).Render();
                        Response.StatusCode = 403;
                        return View("AccessDeny", tips);
                    case TempDenyException:
                        Response.StatusCode = 403;
                        return View("TempDeny");
                    default:
                        LogManager.Error($"异常源：{feature.Error.Source}，异常类型：{feature.Error.GetType().Name}，请求路径：{req.Scheme}://{req.Host}{HttpUtility.UrlDecode(feature.Path)}{req.QueryString} ，客户端用户代理：{req.Headers[HeaderNames.UserAgent]}，客户端IP：{ip}，请求参数：\n{body}\n堆栈信息：", feature.Error);
                        break;
                }
            }

            Response.StatusCode = 503;
            return Request.Method.Equals(HttpMethods.Get) ? View() : Json(new
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
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall]
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
        /// <param name="userInfoService"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall, ResponseCache(Duration = 100, VaryByQueryKeys = new[] { "email" })]
        public ActionResult GetViewToken([FromServices] IUserInfoService userInfoService, string email)
        {
            if (string.IsNullOrEmpty(email) || !email.MatchEmail().isMatch)
            {
                return ResultData(null, false, "请输入正确的邮箱！");
            }

            if (RedisHelper.Exists("get:" + email))
            {
                RedisHelper.Expire("get:" + email, 120);
                return ResultData(null, false, "发送频率限制，请在2分钟后重新尝试发送邮件！请检查你的邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！");
            }

            if (!userInfoService.Any(b => b.Email == email))
            {
                return ResultData(null, false, "您目前没有权限访问这个链接，请联系站长开通访问权限！");
            }

            var token = SnowFlake.GetInstance().GetUniqueShortId(6);
            RedisHelper.Set("token:" + email, token, 86400);
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "博客访问验证码", $"{Request.Host}本次验证码是：<span style='color:red'>{token}</span>，有效期为24h，请按时使用！", email, HttpContext.Connection.RemoteIpAddress.ToString()));
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