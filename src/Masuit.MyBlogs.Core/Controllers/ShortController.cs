using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools;
using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers
{
    public class ShortController : Controller
    {
        [HttpGet("short"), MyAuthorize]
        public IActionResult Short(string key, string url, int? expire)
        {
            expire ??= -1;
            var id = string.IsNullOrEmpty(key) ? url.Crc32().FromBinary(16).ToBinary(62) : key;
            RedisHelper.Set("shorturl:" + id, url, expire.Value);
            return Ok(id);
        }

        [HttpGet("{key}", Order = 100)]
        public ActionResult RedirectTo(string key)
        {
            var url = RedisHelper.Get("shorturl:" + key) ?? throw new NotFoundException("链接未找到");
            return Redirect(url);
        }
    }
}