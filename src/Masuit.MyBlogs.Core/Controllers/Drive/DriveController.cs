using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers.Drive
{
    public class DriveController : Controller
    {
        [HttpGet("/drive")]
        public IActionResult Index()
        {
            return View();
        }
    }
}