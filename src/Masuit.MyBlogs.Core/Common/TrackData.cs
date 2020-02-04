using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;

namespace Masuit.MyBlogs.Core.Common
{
    public class TrackData
    {
        /// <summary>
        /// 请求日志
        /// </summary>
        public static ConcurrentDictionary<string, int> RequestLogs { get; } = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 刷写日志
        /// </summary>
        public static void DumpLog()
        {
            var logPath = Path.Combine(AppContext.BaseDirectory + "logs", "req" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            if (!File.Exists(logPath))
            {
                File.Create(logPath).Dispose();
            }

            File.WriteAllLines(logPath, RequestLogs.OrderBy(x => x.Key).ThenByDescending(x => x.Value).Select(x => x.Value + "\t" + x.Key), Encoding.UTF8);
            File.AppendAllLines(logPath, new[] { "", $"累计处理请求数：{RequestLogs.Sum(kv => kv.Value)}" });
            RequestLogs.Clear();
        }
    }
}