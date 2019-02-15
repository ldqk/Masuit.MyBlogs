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
        [Route("{*url}", Order = 99999), ResponseCache(Duration = 600)]
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
        [Route("ServiceUnavailable"), ResponseCache(Duration = 600)]
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
    }
}