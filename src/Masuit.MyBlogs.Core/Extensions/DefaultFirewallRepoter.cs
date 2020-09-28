using System.Net;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class DefaultFirewallRepoter : IFirewallRepoter
    {
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