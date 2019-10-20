using Masuit.MyBlogs.Core.Configs;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

#if DEBUG
using Masuit.Tools.Win32;
#endif

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 黑科技
    /// </summary>
    [Route("tools")]
    public class ToolsController : BaseController
    {
        /// <summary>
        /// 获取ip地址详细信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        [HttpGet, Route("ip/{ip?}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "ip" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> GetIpInfo(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            ViewBag.IP = ip;
            PhysicsAddress address = await ip.GetPhysicsAddressInfo();
            if (Request.Method.ToLower().Equals("get"))
            {
                return View(address);
            }
            return Json(address);
        }

        /// <summary>
        /// 根据经纬度获取详细地理信息
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        [HttpGet, Route("pos"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "lat", "lng" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Position(string lat, string lng)
        {
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
            {
                var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
#if DEBUG
                Random r = new Random();
                ip = $"{r.Next(210)}.{r.Next(255)}.{r.Next(255)}.{r.Next(255)}";
#endif
                PhysicsAddress address = await ip.GetPhysicsAddressInfo();
                return View(address);
            }
            using (HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.map.baidu.com")
            })
            {
                var s = await client.GetStringAsync($"/geocoder/v2/?location={lat},{lng}&output=json&pois=1&ak={AppConfig.BaiduAK}");
                PhysicsAddress physicsAddress = JsonConvert.DeserializeObject<PhysicsAddress>(s);
                return View(physicsAddress);
            }
        }

        /// <summary>
        /// 详细地理信息转经纬度
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        [HttpGet, Route("addr"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "addr" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Address(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
#if DEBUG
                Random r = new Random();
                ip = $"{r.Next(210)}.{r.Next(255)}.{r.Next(255)}.{r.Next(255)}";
#endif
                PhysicsAddress address = await ip.GetPhysicsAddressInfo();
                if (address?.Status == 0)
                {
                    ViewBag.Address = address.AddressResult.FormattedAddress;
                    if (Request.Method.ToLower().Equals("get"))
                    {
                        return View(address.AddressResult.Location);
                    }
                    return Json(address.AddressResult.Location);
                }
            }
            ViewBag.Address = addr;
            using (HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.map.baidu.com")
            })
            {
                var s = await client.GetStringAsync($"/geocoder/v2/?output=json&address={addr}&ak={AppConfig.BaiduAK}");
                var physicsAddress = JsonConvert.DeserializeAnonymousType(s, new
                {
                    status = 0,
                    result = new
                    {
                        location = new Location()
                    }
                });
                if (Request.Method.ToLower().Equals("get"))
                {
                    return View(physicsAddress.result.location);
                }
                return Json(physicsAddress.result.location);
            }
        }
    }
}