using Collections.Pooled;
using Dispose.Scope;
using Hangfire;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Hardware;
using Masuit.Tools.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PerformanceCounter = Masuit.MyBlogs.Core.Common.PerformanceCounter;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 系统设置
/// </summary>
public sealed class SystemController : AdminController
{
    /// <summary>
    /// 系统设置
    /// </summary>
    public ISystemSettingService SystemSettingService { get; set; }

    public IPerfCounter PerfCounter { get; set; }

    public IEmailBlocklistService EmailBlocklistService { get; set; }

    public IFirewallService FirewallService { get; set; }

    public ActionResult GetServers()
    {
        var servers = PerfCounter.CreateDataSource().Select(c => c.ServerIP).Distinct().ToArray();
        return Ok(servers);
    }

    /// <summary>
    /// 获取历史性能计数器
    /// </summary>
    /// <returns></returns>
    public IActionResult GetCounterHistory(string ip = null)
    {
        ip = ip.IfNullOrEmpty(() => SystemInfo.GetLocalUsedIP(AddressFamily.InterNetwork).ToString());
        var time = DateTime.Now.AddDays(-15).GetTotalMilliseconds();
        var counters = PerfCounter.CreateDataSource().Where(c => c.ServerIP == ip && c.Time >= time);
        var count = counters.Count();
        var ticks = count switch
        {
            <= 5000 => count,
            > 5000 and <= 10000 => 3,
            > 10000 and <= 20000 => 6,
            > 20000 and <= 50000 => 12,
            > 50000 and <= 100000 => 24,
            > 100000 and <= 200000 => 48,
            _ => 72
        } * 10000;

        var list = count < 5000 ? counters.OrderBy(c => c.Time).ToPooledListScope() : counters.GroupBy(c => c.Time / ticks).Select(g => new PerformanceCounter
        {
            Time = g.Key * ticks,
            CpuLoad = g.Average(c => c.CpuLoad),
            ProcessCpuLoad = g.Average(c => c.ProcessCpuLoad),
            DiskRead = g.Average(c => c.DiskRead),
            DiskWrite = g.Average(c => c.DiskWrite),
            Download = g.Average(c => c.Download),
            Upload = g.Average(c => c.Upload),
            MemoryUsage = g.Average(c => c.MemoryUsage),
            ProcessMemoryUsage = g.Average(c => c.ProcessMemoryUsage),
        }).OrderBy(c => c.Time).ToPooledListScope();
        return Ok(new
        {
            cpu = list.Select(c => new[]
            {
                c.Time,
                c.CpuLoad.ToDecimal(2)
            }),
            processCpu = list.Select(c => new[]
            {
                c.Time,
                c.ProcessCpuLoad.ToDecimal(2)
            }),
            mem = list.Select(c => new[]
            {
                c.Time,
                (c.MemoryUsage * 1048576 / SystemInfo.PhysicalMemory).ToDecimal(2)
            }),
            processMem = list.Select(c => new[]
            {
                c.Time,
                (c.ProcessMemoryUsage * 1048576 / SystemInfo.PhysicalMemory).ToDecimal(2)
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
        var list = CommonHelper.SystemSettings.Select(s => new
        {
            Name = s.Key.Item,
            s.Value
        }).ToPooledListScope();
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
        foreach (var item in settings)
        {
            CommonHelper.SystemSettings[item.Name] = item.Value;
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
        BackgroundJob.Enqueue<IMailSender>(sender => sender.Send(title, content + "<p style=\"color: red\">本邮件由系统自动发出，请勿回复本邮件！</p>", tos, "127.0.0.1"));
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
    public ActionResult<PooledList<JObject>> SendBox()
    {
        return RedisHelper.SUnion(RedisHelper.Keys("Email:*")).Select(JObject.Parse).OrderByDescending(o => o["time"]).ToPooledListScope();
    }

    public ActionResult BounceEmail([FromBodyOrDefault] string email)
    {
        EmailBlocklistService.AddEntitySaved(new EmailBlocklist() { Email = email });
        return Ok(new
        {
            msg = "添加成功！"
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
        var list = FirewallService.GetAll();
        return ResultData(new
        {
            interceptCount = FirewallService.TotalCount(),
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
        bool b = FirewallService.Clear();
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
        var list = ips.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
        list.Add(ip);
        await fs.WriteAllTextAsync(string.Join(",", list.Distinct()), Encoding.UTF8);
        CommonHelper.IPWhiteList = list;
        return ResultData(null);
    }

    /// <summary>
    /// 将IP添加到黑名单
    /// </summary>
    /// <param name="firewallReporter"></param>
    /// <param name="ip"></param>
    /// <returns></returns>
    public async Task<ActionResult> AddToBlackList([FromServices] IFirewallReporter firewallReporter, [FromBodyOrDefault] string ip)
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
        await firewallReporter.ReportAsync(IPAddress.Parse(ip));
        return ResultData(null);
    }

    #endregion 网站防火墙
}