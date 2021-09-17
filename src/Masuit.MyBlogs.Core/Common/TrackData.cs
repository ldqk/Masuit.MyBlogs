using Masuit.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Masuit.MyBlogs.Core.Common
{
    public static class TrackData
    {
        /// <summary>
        /// 请求日志
        /// </summary>
        public static ConcurrentDictionary<string, RequestLog> RequestLogs { get; } = new();

        /// <summary>
        /// 刷写日志
        /// </summary>
        public static void DumpLog()
        {
            var logPath = Path.Combine(AppContext.BaseDirectory + "logs", DateTime.Now.ToString("yyyyMMdd"), "req.txt").CreateFileIfNotExist();
            File.WriteAllLines(logPath, RequestLogs.Values.SelectMany(g => g.RequestUrls).GroupBy(s => s).ToDictionary(x => x.Key, x => x.Count()).OrderBy(x => x.Key).ThenByDescending(x => x.Value).Select(g => g.Value + "\t" + g.Key), Encoding.UTF8);
            File.AppendAllLines(logPath, new[] { "", $"累计处理请求数：{RequestLogs.Sum(kv => kv.Value.Count)}" });

            logPath = Path.Combine(AppContext.BaseDirectory + "logs", DateTime.Now.ToString("yyyyMMdd"), "ua.txt").CreateFileIfNotExist();
            File.WriteAllLines(logPath, RequestLogs.Values.SelectMany(g => g.UserAgents).Where(s => !string.IsNullOrEmpty(s)).Select(UserAgent.Parse).Where(ua => !(ua.IsBrowser || ua.IsMobile)).GroupBy(s => s.ToString()).ToDictionary(x => x.Key, x => x.Count()).OrderBy(x => x.Key).ThenByDescending(x => x.Value).Select(g => g.Value + "\t" + g.Key), Encoding.UTF8);

            logPath = Path.Combine(AppContext.BaseDirectory + "logs", DateTime.Now.ToString("yyyyMMdd"), "raw.json").CreateFileIfNotExist();
            File.WriteAllText(logPath, RequestLogs.ToJsonString(new JsonSerializerSettings() { Formatting = Formatting.Indented }), Encoding.UTF8);

            logPath = Path.Combine(AppContext.BaseDirectory + "logs", DateTime.Now.ToString("yyyyMMdd"), "ip.txt").CreateFileIfNotExist();
            File.WriteAllLines(logPath, RequestLogs.Keys.Select(s => new { s, loc = s.GetIPLocation() }).OrderBy(x => x.loc).Select(g => g.s + "\t" + g.loc), Encoding.UTF8);
            RequestLogs.Clear();
        }

        private static string CreateFileIfNotExist(this string filepath)
        {
            var fileInfo = new FileInfo(filepath);
            if (!fileInfo.Exists)
            {
                fileInfo.Directory.Create();
            }

            return filepath;
        }
    }

    public class RequestLog
    {
        public HashSet<string> UserAgents { get; } = new();
        public HashSet<string> RequestUrls { get; } = new();
        public int Count { get; set; }
    }
}