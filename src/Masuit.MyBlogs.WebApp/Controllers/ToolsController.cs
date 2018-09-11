using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Masuit.Tools.Models;
using Masuit.Tools.Net;
#if DEBUG
using Masuit.Tools.Win32;
#endif
using Newtonsoft.Json;
using Quartz;
using Quartz.Spi;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    [RoutePrefix("tools")]
    public class ToolsController : BaseController
    {
        [HttpGet, Route("ip/{ip?}")]
        public async Task<ActionResult> GetIpInfo(string ip)
        {
            if (string.IsNullOrEmpty(ip))
            {
                ip = Request.UserHostAddress;
            }
            ViewBag.IP = ip;
            PhysicsAddress address = await ip.GetPhysicsAddressInfo();
            if (Request.HttpMethod.ToLower().Equals("get"))
            {
                return View(address);
            }
            return Json(address);
        }

        [HttpGet, Route("pos")]
        public async Task<ActionResult> Position(string lat, string lng)
        {
            if (string.IsNullOrEmpty(lat) || string.IsNullOrEmpty(lng))
            {
                var ip = Request.UserHostAddress;
#if DEBUG
                Random r = new Random();
                ip = $"{r.StrictNext(210)}.{r.StrictNext(255)}.{r.StrictNext(255)}.{r.StrictNext(255)}";
#endif
                PhysicsAddress address = await ip.GetPhysicsAddressInfo();
                return View(address);
            }
            using (HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri("http://api.map.baidu.com")
            })
            {
                var s = await client.GetStringAsync($"/geocoder/v2/?location={lat},{lng}&output=json&pois=1&ak={ConfigurationManager.AppSettings["BaiduAK"]}");
                PhysicsAddress physicsAddress = JsonConvert.DeserializeObject<PhysicsAddress>(s);
                return View(physicsAddress);
            }
        }

        [HttpGet, Route("addr")]
        public async Task<ActionResult> Address(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                var ip = Request.UserHostAddress;
#if DEBUG
                Random r = new Random();
                ip = $"{r.StrictNext(210)}.{r.StrictNext(255)}.{r.StrictNext(255)}.{r.StrictNext(255)}";
#endif
                PhysicsAddress address = await ip.GetPhysicsAddressInfo();
                if (address.Status == 0)
                {
                    ViewBag.Address = address.AddressResult.FormattedAddress;
                    if (Request.HttpMethod.ToLower().Equals("get"))
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
                var s = await client.GetStringAsync($"/geocoder/v2/?output=json&address={addr}&ak={ConfigurationManager.AppSettings["BaiduAK"]}");
                var physicsAddress = JsonConvert.DeserializeAnonymousType(s, new
                {
                    status = 0,
                    result = new
                    {
                        location = new Location()
                    }
                });
                if (Request.HttpMethod.ToLower().Equals("get"))
                {
                    return View(physicsAddress.result.location);
                }
                return Json(physicsAddress.result.location);
            }
        }

        [Route("vip")]
        public ActionResult Videos()
        {
            return View();
        }

        public ActionResult Cron()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Cron(string cron)
        {
            //时间表达式
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(cron).Build();
            IList<DateTimeOffset> dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, 5);
            List<string> list = new List<string>();
            foreach (DateTimeOffset dtf in dates)
            {
                list.Add(TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local).ToString());
            }
            if (list.Any())
            {
                return ResultData(list);
            }
            list.Add($"{cron} 不是合法的cron表达式");
            return ResultData(list);
        }
    }
}