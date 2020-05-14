using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Masuit.MyBlogs.Core.Controllers
{
    public class ActivityController : Controller
    {
        [HttpGet("sharecode")]
        public IActionResult GetCode(string q)
        {
            if (string.IsNullOrEmpty(q))
            {
                throw new NotFoundException("联系方式不能为空");
            }

            var enc = q.AESEncrypt();
            Response.Cookies.Append("ShareCode", enc, new CookieOptions()
            {
                Expires = DateTime.Now.AddYears(1),
                SameSite = SameSiteMode.Lax
            });
            return Content(enc);
        }

        public IActionResult ViewCount(string email)
        {
            return Ok(RedisHelper.SMembers("Share:" + email).Length);
        }

        public ActionResult GetActivityUsers()
        {
            var keys = RedisHelper.Keys("Share:*");
            return Json(keys);
        }
    }
}