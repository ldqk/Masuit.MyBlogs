using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools;
using Masuit.Tools.Models;
using Masuit.Tools.Net;
using Masuit.Tools.Win32;
using Models.Enum;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    /// <summary>
    /// 公共API
    /// </summary>
    [Route("public/{action}"), WebApiExceptionFilter]
    public class PublicController : ApiController
    {
        public IPostBll PostBll { get; set; }
        public PublicController(IPostBll postBll)
        {
            PostBll = postBll;
        }

        /// <summary>
        /// 获取文章列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost, Route("subscribe/post")]
        public IHttpActionResult Post()
        {
            var list = PostBll.LoadPageEntitiesNoTracking(1, 10, out int _, p => p.Status == Status.Pended, p => p.ModifyDate, false).Select(p => new
            {
                p.Id,
                p.Title,
                p.Author,
                p.PostDate,
                p.ModifyDate,
                p.Label,
                Category = p.Category.Name
            }).ToList().Select(p => new
            {
                p.Title,
                p.Author,
                p.PostDate,
                p.ModifyDate,
                Tags = p.Label,
                p.Category,
                Link = HttpContext.Current.Request.Url?.Scheme + "://" + HttpContext.Current.Request.Url?.Authority + "/" + p.Id
            }).ToList();
            bool callback = string.IsNullOrEmpty(HttpContext.Current.Request["callback"]);
            if (callback)
            {
                return Content(HttpStatusCode.OK, list);
            }
            return Content(HttpStatusCode.OK, $"{HttpContext.Current.Request["callback"]}({list.ToJsonString()});");
        }

        /// <summary>
        /// 根据经纬度获取详细地理信息
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        [HttpPost, Route("tools/position")]
        public async Task<PhysicsAddress> Position(string lat, string lng)
        {
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
            {
                var ip = HttpContext.Current.Request.UserHostAddress;
#if DEBUG
                Random r = new Random();
                ip = $"{r.StrictNext(210)}.{r.StrictNext(255)}.{r.StrictNext(255)}.{r.StrictNext(255)}";
#endif
                PhysicsAddress address = await ip.GetPhysicsAddressInfo();
                return address;
            }
            HttpClient client = new HttpClient() { BaseAddress = new Uri("http://api.map.baidu.com") };
            string s = client.GetStringAsync($"/geocoder/v2/?location={lat},{lng}&output=json&pois=1&ak={ConfigurationManager.AppSettings["BaiduAK"]}").Result;
            PhysicsAddress physicsAddress = JsonConvert.DeserializeObject<PhysicsAddress>(s);
            return physicsAddress;
        }

        /// <summary>
        /// 根据详细地址获取经纬度
        /// </summary>
        /// <param name="addr">详细地理信息</param>
        /// <returns></returns>

        [HttpPost, Route("tools/address")]
        public async Task<Location> Address(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                var ip = HttpContext.Current.Request.UserHostAddress;
#if DEBUG
                Random r = new Random();
                ip = $"{r.StrictNext(210)}.{r.StrictNext(255)}.{r.StrictNext(255)}.{r.StrictNext(255)}";
#endif
                PhysicsAddress address = await ip.GetPhysicsAddressInfo();
                if (address.Status == 0)
                {
                    return address.AddressResult.Location;
                }
            }
            HttpClient client = new HttpClient() { BaseAddress = new Uri("http://api.map.baidu.com") };
            string s = client.GetStringAsync($"/geocoder/v2/?output=json&address={addr}&ak={ConfigurationManager.AppSettings["BaiduAK"]}").Result;
            var physicsAddress = JsonConvert.DeserializeAnonymousType(s, new { status = 0, result = new { location = new Location() } });
            return physicsAddress.result.location;
        }

        /// <summary>
        /// 获取ip地址详细地理信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        [HttpPost, Route("tools/ipinfo")]
        public async Task<PhysicsAddress> GetIpInfo(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.UserHostAddress;
            }
            PhysicsAddress address = await ip.GetPhysicsAddressInfo();
            return address;
        }
    }
}
