using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;

namespace Masuit.MyBlogs.Core.Controllers
{
    public class FirewallController : Controller
    {
        /// <summary>
        /// JS挑战，5秒盾
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost("/challenge"), AutoValidateAntiforgeryToken]
        public ActionResult JsChallenge(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length < 20)
            {
                return BadRequest("请求token无效");
            }

            try
            {
                var privateKey = HttpContext.Session.Get<string>("challenge-private-key") ?? throw new NotFoundException("请求私钥无效");
                var crypto = HttpContext.Session.Get<string>("challenge-value") ?? throw new NotFoundException("请求私钥无效");
                if (token.RSADecrypt(privateKey) == crypto)
                {
                    HttpContext.Session.Set("js-challenge", 1);
                    HttpContext.Session.Remove("challenge-private-key");
                    HttpContext.Session.Remove("challenge-value");
                    Response.Cookies.Delete("challenge-key");
                }

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// 验证码挑战
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost("/captcha"), AutoValidateAntiforgeryToken]
        public ActionResult CaptchaChallenge(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length < 4)
            {
                return BadRequest("验证码无效");
            }

            if (code.Equals(HttpContext.Session.Get<string>("challenge-captcha"), StringComparison.CurrentCultureIgnoreCase))
            {
                HttpContext.Session.Set("js-challenge", 1);
                HttpContext.Session.Remove("challenge-captcha");
            }

            return Redirect(Request.Headers[HeaderNames.Referer]);
        }

        /// <summary>
        /// 验证码
        /// </summary>
        /// <returns></returns>
        [HttpGet("/challenge-captcha.jpg")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult CaptchaChallenge()
        {
            string code = ValidateCode.CreateValidateCode(6);
            HttpContext.Session.Set("challenge-captcha", code);
            var buffer = HttpContext.CreateValidateGraphic(code);
            return this.ResumeFile(buffer, ContentType.Jpeg, "验证码.jpg");
        }
    }
}