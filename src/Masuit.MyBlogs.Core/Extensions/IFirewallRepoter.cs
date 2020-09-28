using System.Net;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    public interface IFirewallRepoter
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
        Task ReportAsync(IPAddress ip);
    }
}