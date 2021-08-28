using Masuit.MyBlogs.Core.Extensions.Firewall;
using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers.Drive
{
    [ServiceFilter(typeof(FirewallAttribute))]
    public class DriveController : Controller
    {
        [HttpGet("/drive")]
        public IActionResult Index()
        {
            return View();
        }
    }
}