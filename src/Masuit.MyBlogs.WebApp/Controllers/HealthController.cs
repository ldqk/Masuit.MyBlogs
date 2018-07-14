using System.Web.Http;
using System.Web.Http.Results;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    [Route("health/{action}")]
    public class HealthController : ApiController
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