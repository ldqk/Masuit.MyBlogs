using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Masuit.MyBlogs.Core.Controllers
{
    public class ShortController : Controller
    {
        [HttpGet("short")]
        public IActionResult Short(string url)
        {
            var id = Math.Abs(url.GetHashCode()).ToBinary(62);
            RedisHelper.Set("shorturl:" + id, url);
            return Ok(id);
        }

        [HttpGet("{key}")]
        public ActionResult Redirect(string key)
        {
            var url = RedisHelper.Get("shorturl:" + key) ?? throw new NotFoundException("链接未找到");
            return RedirectPermanent(url);
        }
    }
}