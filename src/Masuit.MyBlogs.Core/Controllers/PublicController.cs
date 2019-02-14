using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

#if DEBUG
using Masuit.Tools.Win32; 
#endif

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 公共API
    /// </summary>
    public class PublicController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        public IPostService PostService { get; set; }

        /// <summary>
        /// 开放API
        /// </summary>
        /// <param name="postService"></param>
        public PublicController(IPostService postService)
        {
            PostService = postService;
        }

        /// <summary>
        /// 获取文章列表
        /// </summary>
        /// <returns></returns>
        [HttpGet, HttpPost, Route("subscribe/post"), ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public IActionResult Post()
        {
            var list = PostService.LoadPageEntitiesNoTracking(1, 10, out int _, p => p.Status == Status.Pended, p => p.ModifyDate, false).Select(p => new
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
                Link = Request.Scheme + "://" + Request.Host + "/" + p.Id
            }).ToList();
            bool callback = string.IsNullOrEmpty(Request.Query["callback"]);
            if (callback)
            {
                return Ok(list);
            }
            return Ok($"{Request.Query["callback"]}({list.ToJsonString()});");
        }

        /// <summary>
        /// 根据经纬度获取详细地理信息
        /// </summary>
        /// <param name="lat">纬度</param>
        /// <param name="lng">经度</param>
        /// <returns></returns>
        [HttpPost("tools/position"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "lat", "lng" }, VaryByHeader = HeaderNames.Cookie)]
        public async Task<PhysicsAddress> Position(string lat, string lng)
        {
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
            {
                var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
#if DEBUG
                Random r = new Random();
                ip = $"{r.StrictNext(210)}.{r.StrictNext(255)}.{r.StrictNext(255)}.{r.StrictNext(255)}";
#endif
                PhysicsAddress address = await ip.GetPhysicsAddressInfo();
                return address;
            }
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.map.baidu.com")
            };
            string s = client.GetStringAsync($"/geocoder/v2/?location={lat},{lng}&output=json&pois=1&ak={AppConfig.BaiduAK}").Result;
            PhysicsAddress physicsAddress = JsonConvert.DeserializeObject<PhysicsAddress>(s);
            return physicsAddress;
        }

        /// <summary>
        /// 根据详细地址获取经纬度
        /// </summary>
        /// <param name="addr">详细地理信息</param>
        /// <returns></returns>
        [HttpPost("tools/address"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "addr" }, VaryByHeader = HeaderNames.Cookie)]
        public async Task<Location> Address(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
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
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.map.baidu.com")
            };
            string s = client.GetStringAsync($"/geocoder/v2/?output=json&address={addr}&ak={AppConfig.BaiduAK}").Result;
            var physicsAddress = JsonConvert.DeserializeAnonymousType(s, new
            {
                status = 0,
                result = new
                {
                    location = new Location()
                }
            });
            return physicsAddress.result.location;
        }

        /// <summary>
        /// 获取ip地址详细地理信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        [HttpPost("tools/ipinfo"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "ip" }, VaryByHeader = HeaderNames.Cookie)]
        public async Task<PhysicsAddress> GetIpInfo(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            PhysicsAddress address = await ip.GetPhysicsAddressInfo();
            return address;
        }
    }
}