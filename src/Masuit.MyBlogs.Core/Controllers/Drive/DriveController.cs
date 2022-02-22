using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (CommonHelper.SystemSettings.GetOrAdd("CloseSite", "false") == "true")
            {
                context.Result = RedirectToAction("ComingSoon", "Error");
                return Task.CompletedTask;
            }
            return next();
        }
    }
}
