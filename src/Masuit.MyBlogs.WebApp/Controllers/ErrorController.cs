using System.Web.Mvc;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        [Route("error")]
        public ActionResult Index()
        {
            //Response.StatusCode = 404;
            if (Request.HttpMethod.ToLower().Equals("get"))
            {
                return View();
            }
            return Json(new
            {
                StatusCode = 404,
                Success = false,
                Message = "页面未找到！"
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("ServiceUnavailable")]
        public ActionResult ServiceUnavailable()
        {
            //Response.StatusCode = 503;
            if (Request.HttpMethod.ToLower().Equals("get"))
            {
                return View();
            }
            return Json(new
            {
                StatusCode = 503,
                Success = false,
                Message = "服务器发生错误！"
            }, JsonRequestBehavior.AllowGet);
        }
    }
}