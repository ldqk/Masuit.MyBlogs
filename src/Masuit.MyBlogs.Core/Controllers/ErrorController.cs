using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 错误页
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : Controller
    {
        /// <summary>
        /// 404
        /// </summary>
        /// <returns></returns>
        [Route("error"), Route("{*url}", Order = 99999), ResponseCache(Duration = 36000)]
        public ActionResult Index()
        {
            Response.StatusCode = 404;
            if (Request.Method.ToLower().Equals("get"))
            {
                return View();
            }
            return Json(new
            {
                StatusCode = 404,
                Success = false,
                Message = "页面未找到！"
            });
        }

        /// <summary>
        /// 503
        /// </summary>
        /// <returns></returns>
        [Route("ServiceUnavailable"), ResponseCache(Duration = 36000)]
        public ActionResult ServiceUnavailable()
        {
            Response.StatusCode = 503;
            if (Request.Method.ToLower().Equals("get"))
            {
                return View();
            }
            return Json(new
            {
                StatusCode = 503,
                Success = false,
                Message = "服务器发生错误！"
            });
        }

        /// <summary>
        /// 访问被拒绝
        /// </summary>
        /// <returns></returns>
        [Route("AccessDeny"), ResponseCache(Duration = 360000)]
        public ActionResult AccessDeny()
        {
            Response.StatusCode = 403;
            return View();
        }

        /// <summary>
        /// 临时被拒绝
        /// </summary>
        /// <returns></returns>
        [Route("TempDeny"), ResponseCache(Duration = 360000)]
        public ActionResult TempDeny()
        {
            Response.StatusCode = 403;
            return View();
        }

        /// <summary>
        /// 网站升级中
        /// </summary>
        /// <returns></returns>
        [Route("ComingSoon"), ResponseCache(Duration = 360000)]
        public ActionResult ComingSoon()
        {
            return View();
        }
    }
}