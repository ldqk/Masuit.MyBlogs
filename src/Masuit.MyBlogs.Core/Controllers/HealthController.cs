using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 健康检查
    /// </summary>
    public class HealthController : Controller
    {
        /// <summary>
        /// 心跳检测
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("health")]
        public OkResult Check()
        {
            return Ok();
        }
    }
}