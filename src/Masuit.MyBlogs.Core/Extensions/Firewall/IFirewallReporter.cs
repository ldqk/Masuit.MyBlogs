using System.Net;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public interface IFirewallReporter
{
    string ReporterName { get; set; }

    /// <summary>
    /// 上报IP
    /// </summary>
    /// <param name="ip"></param>
    void Report(IPAddress ip);

    /// <summary>
    /// 上报IP
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    Task<bool> ReportAsync(IPAddress ip);
}