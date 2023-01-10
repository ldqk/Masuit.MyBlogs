using FreeRedis;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers;

public sealed class ShortController : Controller
{
	public IRedisClient RedisHelper { get; set; }

	[HttpGet("short"), MyAuthorize, AllowAccessFirewall]
	public IActionResult Short(string key, string url, int? expire)
	{
		expire ??= -1;
		var id = string.IsNullOrEmpty(key) ? url.Crc32().FromBase(16).ToBase(62) : key;
		RedisHelper.Set("shorturl:" + id, url, expire.Value);
		return Ok(id);
	}

	[HttpGet("{key}", Order = 100), AllowAccessFirewall]
	public ActionResult RedirectTo(string key)
	{
		var url = RedisHelper.Get("shorturl:" + key) ?? throw new NotFoundException("链接未找到");
		return Redirect(url);
	}
}
