using System.Net;

namespace Masuit.MyBlogs.Core.Extensions.Firewall
{
    public sealed class DefaultFirewallRepoter : IFirewallRepoter
    {
        public string ReporterName { get; set; }

        /// <summary>
        /// 上报IP
        /// </summary>
        /// <param name="ip"></param>
        public void Report(IPAddress ip)
        {
        }

        /// <summary>
        /// 上报IP
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public Task ReportAsync(IPAddress ip)
        {
            return Task.CompletedTask;
        }
    }
}