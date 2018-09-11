using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common;
using IBLL;
using Masuit.Tools;
using Masuit.Tools.Hardware;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Masuit.Tools.Systems;
using Masuit.Tools.Win32;
using Models.Entity;
using Models.Enum;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class SystemController : AdminController
    {
        public ISystemSettingBll SystemSettingBll { get; set; }

        public SystemController(IUserInfoBll userInfoBll, ISystemSettingBll systemSettingBll)
        {
            UserInfoBll = userInfoBll;
            SystemSettingBll = systemSettingBll;
        }

        public async Task<ActionResult> GetBaseInfo()
        {
            List<CpuInfo> cpuInfo = SystemInfo.GetCpuInfo();
            RamInfo ramInfo = SystemInfo.GetRamInfo();
            string osVersion = SystemInfo.GetOsVersion();
            var total = new StringBuilder();
            var free = new StringBuilder();
            var usage = new StringBuilder();
            SystemInfo.DiskTotalSpace().ForEach(kv =>
            {
                total.Append(kv.Key + kv.Value + " | ");
            });
            SystemInfo.DiskFree().ForEach(kv => free.Append(kv.Key + kv.Value + " | "));
            SystemInfo.DiskUsage().ForEach(kv => usage.Append(kv.Key + kv.Value.ToString("P") + " | "));
            IList<string> mac = SystemInfo.GetMacAddress();
            IList<string> ips = SystemInfo.GetIPAddress();
            var span = DateTime.Now - CommonHelper.StartupTime;
            var boot = DateTime.Now - SystemInfo.BootTime();

            return Content(await new
            {
                runningTime = $"{span.Days}天{span.Hours}小时{span.Minutes}分钟",
                bootTime = $"{boot.Days}天{boot.Hours}小时{boot.Minutes}分钟",
                cpuInfo,
                ramInfo,
                osVersion,
                diskInfo = new
                {
                    total = total.ToString(),
                    free = free.ToString(),
                    usage = usage.ToString()
                },
                netInfo = new
                {
                    mac,
                    ips
                }
            }.ToJsonStringAsync().ConfigureAwait(false), "application/json");
        }

        public ActionResult GetHistoryList()
        {
            return Json(new
            {
                cpu = CommonHelper.HistoryCpuLoad,
                mem = CommonHelper.HistoryMemoryUsage,
                temp = CommonHelper.HistoryCpuTemp,
                read = CommonHelper.HistoryIORead,
                write = CommonHelper.HistoryIOWrite,
                down = CommonHelper.HistoryNetReceive,
                up = CommonHelper.HistoryNetSend
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSettings()
        {
            var list = SystemSettingBll.GetAll().Select(s => new
            {
                s.Name,
                s.Value
            }).ToList();
            return ResultData(list);
        }

        [AllowAnonymous]
        public ActionResult GetSetting(string name)
        {
            var entity = SystemSettingBll.GetFirstEntity(s => s.Name.Equals(name));
            return ResultData(entity);
        }

        [ValidateInput(false)]
        public ActionResult Save(string sets)
        {
            bool b = SystemSettingBll.AddOrUpdateSaved(s => s.Name, JsonConvert.DeserializeObject<List<SystemSetting>>(sets).ToArray()) > 0;
            return ResultData(null, b, b ? "设置保存成功！" : "设置保存失败！");
        }

        public ActionResult CollectMemory()
        {
            double p = Windows.ClearMemory();
            return ResultData(null, true, "内存整理成功，当前内存使用率：" + p.ToString("N") + "%");
        }

        public ActionResult GetStatus()
        {
            Array array = Enum.GetValues(typeof(Status));
            var list = new List<object>();
            foreach (Enum e in array)
            {
                list.Add(new
                {
                    e,
                    name = e.GetDisplay()
                });
            }
            return ResultData(list);
        }

        public ActionResult MailTest(string smtp, string user, string pwd, int port, string to)
        {
            try
            {
                new Email()
                {
                    EnableSsl = true,
                    Body = "发送成功，网站邮件配置正确！",
                    SmtpServer = smtp,
                    Username = user,
                    Password = pwd,
                    SmtpPort = port,
                    Subject = "网站测试邮件",
                    Tos = to
                }.Send();
                return ResultData(null, true, "测试邮件发送成功，网站邮件配置正确！");
            }
            catch (Exception e)
            {
                return ResultData(null, false, "邮件配置测试失败！错误信息：\r\n" + e.Message + "\r\n\r\n详细堆栈跟踪：\r\n" + e.StackTrace);
            }
        }

        public ActionResult PathTest(string path)
        {
            if (path.Equals("/") || path.Equals("\\") || string.IsNullOrWhiteSpace(path))
            {
                return ResultData(null, true, "根路径正确");
            }
            try
            {
                bool b = Directory.Exists(path);
                return ResultData(null, b, b ? "根路径正确" : "路径不存在");
            }
            catch (Exception e)
            {
                LogManager.Error(GetType(), e);
                return ResultData(null, false, "路径格式不正确！错误信息：\r\n" + e.Message + "\r\n\r\n详细堆栈跟踪：\r\n" + e.StackTrace);
            }
        }
    }
}