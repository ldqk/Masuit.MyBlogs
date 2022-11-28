using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.Tools.AspNetCore.ModelBinder;
using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers;

public sealed class DefaultController : Controller
{
    /// <summary>
    /// 设置cookie
    /// </summary>
    /// <param name="pair"></param>
    /// <returns></returns>
    [HttpPost("/SetCookie"), HttpGet("/SetCookie"), AllowAccessFirewall]
    public ActionResult SetCookie([FromBodyOrDefault] NameValuePair pair)
    {
        Response.Cookies.Append(pair.Name, pair.Value, new CookieOptions
        {
            SameSite = SameSiteMode.None
        });
        return Ok();
    }
}

public class NameValuePair
{
    public string Name { get; set; }

    public string Value { get; set; }
}
