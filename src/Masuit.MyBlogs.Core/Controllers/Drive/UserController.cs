using Hangfire;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Drive;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Masuit.MyBlogs.Core.Controllers.Drive
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class UserController : Controller
    {
        public IUserInfoService UserInfoService { get; set; }

        /// <summary>
        /// 验证 返回Token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateModel model)
        {
            var user = UserInfoService.Login(model.Username, model.Password);
            if (user is null)
            {
                return BadRequest(new ErrorResponse()
                {
                    message = "错误的用户名或密码"
                });
            }
            Response.Cookies.Append("username", HttpUtility.UrlEncode(model.Username.Trim()), new CookieOptions()
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            Response.Cookies.Append("password", model.Password.Trim().DesEncrypt(AppConfig.BaiduAK), new CookieOptions()
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            HttpContext.Session.Set(SessionKey.UserInfo, user);
            BackgroundJob.Enqueue<IHangfireBackJob>(job => job.LoginRecord(user, HttpContext.Connection.RemoteIpAddress.ToString(), LoginType.Default));
            return Ok(new
            {
                error = false,
                id = model.Username,
                username = model.Username,
                token = model.Username
            });
        }
    }

    public class AuthenticateModel
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
