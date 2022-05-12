using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Text;
using PerformanceCounter = Masuit.MyBlogs.Core.Common.PerformanceCounter;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 系统设置
    /// </summary>
    public class SystemController : AdminController
    {
        /// <summary>
        /// 系统设置
        /// </summary>
        public ISystemSettingService SystemSettingService { get; set; }

        /// <summary>
        /// 获取历史性能计数器
        /// </summary>
        /// <returns></returns>
        public IActionResult GetCounterHistory()
        {
            var counters = PerfCounter.List.OrderBy(c => c.Time);
            var list = counters.Count() < 5000 ? counters : counters.GroupBy(c => c.Time / 60000).Select(g => new PerformanceCounter
            {
                Time = g.Key * 60000,
                CpuLoad = g.OrderBy(c => c.CpuLoad).Skip(1).Take(g.Count() - 2).Select(c => c.CpuLoad).DefaultIfEmpty().Average(),
                DiskRead = g.OrderBy(c => c.DiskRead).Skip(1).Take(g.Count() - 2).Select(c => c.DiskRead).DefaultIfEmpty().Average(),
                DiskWrite = g.OrderBy(c => c.DiskWrite).Skip(1).Take(g.Count() - 2).Select(c => c.DiskWrite).DefaultIfEmpty().Average(),
                Download = g.OrderBy(c => c.Download).Skip(1).Take(g.Count() - 2).Select(c => c.Download).DefaultIfEmpty().Average(),
                Upload = g.OrderBy(c => c.Upload).Skip(1).Take(g.Count() - 2).Select(c => c.Upload).DefaultIfEmpty().Average(),
                MemoryUsage = g.OrderBy(c => c.MemoryUsage).Skip(1).Take(g.Count() - 2).Select(c => c.MemoryUsage).DefaultIfEmpty().Average()
            });
            return Ok(new
            {
                cpu = list.Select(c => new[]
                {
                    c.Time,
                    c.CpuLoad.ToDecimal(2)
                }),
                mem = list.Select(c => new[]
                {
                    c.Time,
                    c.MemoryUsage.ToDecimal(2)
                }),
                read = list.Select(c => new[]
                {
                    c.Time,
                    c.DiskRead.ToDecimal(2)
                }),
                write = list.Select(c => new[]
                {
                    c.Time,
                    c.DiskWrite.ToDecimal(2)
                }),
                down = list.Select(c => new[]
                {
                    c.Time,
                    c.Download.ToDecimal(2)
                }),
                up = list.Select(c => new[]
                {
                    c.Time,
                    c.Upload.ToDecimal(2)
                })
            });
        }

        /// <summary>
        /// 获取设置信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSettings()
        {
            var list = SystemSettingService.GetAll().Select(s => new
            {
                s.Name,
                s.Value
            }).ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<ActionResult> Save([FromBody] List<SystemSetting> settings)
        {
            var b = await SystemSettingService.AddOrUpdateSavedAsync(s => s.Name, settings) > 0;
            var dic = settings.ToDictionary(s => s.Name, s => s.Value); //同步设置
            foreach (var (key, value) in dic)
            {
                CommonHelper.SystemSettings[key] = value;
            }

            return ResultData(null, b, b ? "设置保存成功！" : "设置保存失败！");
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 邮件测试
        /// </summary>
        /// <param name="smtp"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <param name="port"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public ActionResult MailTest([FromBodyOrDefault] string smtp, [FromBodyOrDefault] string user, [FromBodyOrDefault] string pwd, [FromBodyOrDefault] int port, [FromBodyOrDefault] string to, [FromBodyOrDefault] bool ssl)
        {
            try
            {
                new Email()
                {
                    EnableSsl = ssl,
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

        /// <summary>
        /// 发送一封系统邮件
        /// </summary>
        /// <param name="tos"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public ActionResult SendMail([Required(ErrorMessage = "收件人不能为空"), FromBodyOrDefault] string tos, [Required(ErrorMessage = "邮件标题不能为空"), FromBodyOrDefault] string title, [Required(ErrorMessage = "邮件内容不能为空"), FromBodyOrDefault] string content)
        {
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(title, content + "<p style=\"color: red\">本邮件由系统自动发出，请勿回复本邮件！</p>", tos, "127.0.0.1"));
            return Ok();
        }

        /// <summary>
        /// 路径测试
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult PathTest([FromBodyOrDefault] string path)
        {
            if (!(path.EndsWith("/") || path.EndsWith("\\")))
            {
                return ResultData(null, false, "路径不存在");
            }

            if (path.Equals("/") || path.Equals("\\"))
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
                LogManager.Error(GetType(), e.Demystify());
                return ResultData(null, false, "路径格式不正确！错误信息：\r\n" + e.Message + "\r\n\r\n详细堆栈跟踪：\r\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 发件箱记录
        /// </summary>
        /// <returns></returns>
        public ActionResult<List<JObject>> SendBox()
        {
            return RedisHelper.SUnion(RedisHelper.Keys("Email:*")).Select(JObject.Parse).OrderByDescending(o => o["time"]).ToList();
        }

        public ActionResult BounceEmail([FromServices] IMailSender mailSender, [FromBodyOrDefault] string email)
        {
            var msg = mailSender.AddRecipient(email);
            return Ok(new
            {
                msg
            });
        }

        #region 网站防火墙

        /// <summary>
        /// 获取全局IP黑名单
        /// </summary>
        /// <returns></returns>
        public ActionResult IpBlackList()
        {
            return ResultData(CommonHelper.DenyIP);
        }

        /// <summary>
        /// 获取IP地址段黑名单
        /// </summary>
        /// <returns></returns>
        public ActionResult GetIPRangeBlackList()
        {
            return ResultData(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "DenyIPRange.txt")).ShareReadWrite().ReadAllText(Encoding.UTF8));
        }

        /// <summary>
        /// 设置IP地址段黑名单
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> SetIPRangeBlackList([FromBodyOrDefault] string content)
        {
            var file = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "DenyIPRange.txt")).ShareReadWrite();
            await file.WriteAllTextAsync(content, Encoding.UTF8, false);
            CommonHelper.DenyIPRange.Clear();
            var lines = (await file.ReadAllLinesAsync(Encoding.UTF8)).Where(s => s.Split(' ').Length > 2);
            foreach (var line in lines)
            {
                try
                {
                    var strs = line.Split(' ');
                    CommonHelper.DenyIPRange[strs[0]] = strs[1];
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            return ResultData(null);
        }

        /// <summary>
        /// 全局IP白名单
        /// </summary>
        /// <returns></returns>
        public ActionResult IpWhiteList()
        {
            return ResultData(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "whitelist.txt")).ShareReadWrite().ReadAllText(Encoding.UTF8));
        }

        /// <summary>
        /// 设置IP黑名单
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<ActionResult> SetIpBlackList([FromBodyOrDefault] string content)
        {
            CommonHelper.DenyIP = content + "";
            await new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "denyip.txt")).ShareReadWrite().WriteAllTextAsync(CommonHelper.DenyIP, Encoding.UTF8);
            return ResultData(null);
        }

        /// <summary>
        /// 设置IP白名单
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<ActionResult> SetIpWhiteList([FromBodyOrDefault] string content)
        {
            await new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "whitelist.txt")).ShareReadWrite().WriteAllTextAsync(content, Encoding.UTF8);
            CommonHelper.IPWhiteList.Add(content);
            return ResultData(null);
        }

        /// <summary>
        /// 获取拦截日志
        /// </summary>
        /// <returns></returns>
        public ActionResult InterceptLog()
        {
            var list = RedisHelper.LRange<IpIntercepter>("intercept", 0, -1);
            return ResultData(new
            {
                interceptCount = RedisHelper.Get("interceptCount"),
                list,
                ranking = list.GroupBy(i => i.IP).Where(g => g.Count() > 1).Select(g =>
                {
                    var start = g.Min(t => t.Time);
                    var end = g.Max(t => t.Time);
                    return new
                    {
                        g.Key,
                        g.First().Address,
                        Start = start,
                        End = end,
                        Continue = start.GetDiffTime(end),
                        Count = g.Count()
                    };
                }).OrderByDescending(a => a.Count).Take(30)
            });
        }

        /// <summary>
        /// 清除拦截日志
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearInterceptLog()
        {
            bool b = RedisHelper.Del("intercept") > 0;
            return ResultData(null, b, b ? "拦截日志清除成功！" : "拦截日志清除失败！");
        }

        /// <summary>
        /// 将IP添加到白名单
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<ActionResult> AddToWhiteList([FromBodyOrDefault] string ip)
        {
            if (!ip.MatchInetAddress())
            {
                return ResultData(null, false);
            }

            var fs = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "whitelist.txt")).ShareReadWrite();
            string ips = await fs.ReadAllTextAsync(Encoding.UTF8, false);
            List<string> list = ips.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
            list.Add(ip);
            await fs.WriteAllTextAsync(string.Join(",", list.Distinct()), Encoding.UTF8);
            CommonHelper.IPWhiteList = list;
            return ResultData(null);
        }

        /// <summary>
        /// 将IP添加到黑名单
        /// </summary>
        /// <param name="firewallRepoter"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<ActionResult> AddToBlackList([FromServices] IFirewallRepoter firewallRepoter, [FromBodyOrDefault] string ip)
        {
            if (!ip.MatchInetAddress())
            {
                return ResultData(null, false);
            }

            CommonHelper.DenyIP += "," + ip;
            var basedir = AppDomain.CurrentDomain.BaseDirectory;
            await new FileInfo(Path.Combine(basedir, "App_Data", "denyip.txt")).ShareReadWrite().WriteAllTextAsync(CommonHelper.DenyIP, Encoding.UTF8);
            CommonHelper.IPWhiteList.Remove(ip);
            await new FileInfo(Path.Combine(basedir, "App_Data", "whitelist.txt")).ShareReadWrite().WriteAllTextAsync(string.Join(",", CommonHelper.IPWhiteList.Distinct()), Encoding.UTF8);
            await firewallRepoter.ReportAsync(IPAddress.Parse(ip));
            return ResultData(null);
        }

        #endregion 网站防火墙
    }
}
