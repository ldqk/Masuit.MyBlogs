using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TimeZoneConverter;

#if DEBUG
#endif

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 黑科技
    /// </summary>
    [Route("tools")]
    public class ToolsController : BaseController
    {
        private readonly HttpClient _httpClient;

        public ToolsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// 获取ip地址详细信息
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        [Route("ip/{ip?}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "ip" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> GetIpInfo(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                ip = ClientIP;
            }

            ViewBag.IP = ip;
            var location = CommonHelper.MaxmindReader.City(ip).Location;
            var address = await ip.GetPhysicsAddressInfo() ?? new PhysicsAddress()
            {
                Status = 0,
                AddressResult = new AddressResult()
                {
                    AddressComponent = new AddressComponent(),
                    FormattedAddress = ip.GetIPLocation(),
                    Location = new Location()
                    {
                        Lng = location.Longitude.Value,
                        Lat = location.Latitude.Value
                    }
                }
            };
            address.AddressResult.Pois.Add(new Pois
            {
                AddressDetail = $"{ip.GetIPLocation()}（UTC{TZConvert.GetTimeZoneInfo(location.TimeZone).BaseUtcOffset.Hours:+#;-#;0}，本地数据库）"
            });
            if (Request.Method.Equals(HttpMethods.Get))
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
        [HttpGet("pos"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "lat", "lng" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Position(string lat, string lng)
        {
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
            {
                var ip = ClientIP;
#if DEBUG
                var r = new Random();
                ip = $"{r.Next(210)}.{r.Next(255)}.{r.Next(255)}.{r.Next(255)}";
#endif
                var address = await ip.GetPhysicsAddressInfo();
                return View(address);
            }

            var s = await _httpClient.GetStringAsync($"http://api.map.baidu.com/geocoder/v2/?location={lat},{lng}&output=json&pois=1&ak={AppConfig.BaiduAK}");
            var physicsAddress = JsonConvert.DeserializeObject<PhysicsAddress>(s);
            return View(physicsAddress);
        }

        /// <summary>
        /// 详细地理信息转经纬度
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        [Route("addr"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "addr" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Address(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                var ip = ClientIP;
#if DEBUG
                Random r = new Random();
                ip = $"{r.Next(210)}.{r.Next(255)}.{r.Next(255)}.{r.Next(255)}";
#endif
                var address = await ip.GetPhysicsAddressInfo();
                if (address?.Status == 0)
                {
                    ViewBag.Address = address.AddressResult.FormattedAddress;
                    if (Request.Method.Equals(HttpMethods.Get))
                    {
                        return View(address.AddressResult.Location);
                    }

                    return Json(address.AddressResult.Location);
                }
            }

            ViewBag.Address = addr;
            var s = await _httpClient.GetStringAsync($"http://api.map.baidu.com/geocoder/v2/?output=json&address={addr}&ak={AppConfig.BaiduAK}");
            var physicsAddress = JsonConvert.DeserializeObject<PhysicsAddress>(s);
            if (Request.Method.Equals(HttpMethods.Get))
            {
                return View(physicsAddress?.AddressResult?.Location);
            }

            return Json(physicsAddress?.AddressResult?.Location);
        }
    }
}