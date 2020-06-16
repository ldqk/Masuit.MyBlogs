using AutoMapper;
using Hangfire;
using HtmlAgilityPack;
using IP2Region;
using Masuit.Tools;
using Masuit.Tools.Media;
using Masuit.Tools.Models;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

#if !DEBUG
#endif

namespace Masuit.MyBlogs.Core.Common
{
    /// <summary>
    /// 公共类库
    /// </summary>
    public static class CommonHelper
    {
        static CommonHelper()
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                while (true)
                {
                    BanRegex = File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "ban.txt"), Encoding.UTF8);
                    ModRegex = File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "mod.txt"), Encoding.UTF8);
                    DenyIP = File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "denyip.txt"), Encoding.UTF8);
                    string[] lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory + "App_Data", "DenyIPRange.txt"), Encoding.UTF8);
                    DenyIPRange = new Dictionary<string, string>();
                    foreach (string line in lines)
                    {
                        try
                        {
                            var strs = line.Split(' ');
                            DenyIPRange[strs[0]] = strs[1];
                        }
                        catch (IndexOutOfRangeException)
                        {
                        }
                    }

                    IPWhiteList = File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "whitelist.txt")).Split(',', '，').ToList();
                    Console.WriteLine("刷新公共数据...");
                    Thread.Sleep(TimeSpan.FromMinutes(10));
                }
            });
        }

        /// <summary>
        /// 敏感词
        /// </summary>
        public static string BanRegex { get; set; }

        /// <summary>
        /// 审核词
        /// </summary>
        public static string ModRegex { get; set; }

        /// <summary>
        /// 全局禁止IP
        /// </summary>
        public static string DenyIP { get; set; }

        /// <summary>
        /// ip白名单
        /// </summary>
        public static List<string> IPWhiteList { get; set; }

        /// <summary>
        /// 每IP错误的次数统计
        /// </summary>
        public static ConcurrentDictionary<string, int> IPErrorTimes { get; set; } = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 系统设定
        /// </summary>
        public static ConcurrentDictionary<string, string> SystemSettings { get; set; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 网站启动时间
        /// </summary>
        public static DateTime StartupTime { get; set; } = DateTime.Now;

        /// <summary>
        /// IP黑名单地址段
        /// </summary>
        public static Dictionary<string, string> DenyIPRange { get; set; }

        /// <summary>
        /// 判断IP地址是否被黑名单
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsDenyIpAddress(this string ip)
        {
            if (IPWhiteList.Contains(ip))
            {
                return false;
            }

            return DenyIP.Contains(ip) || DenyIPRange.AsParallel().Any(kv => kv.Key.StartsWith(ip.Split('.')[0]) && ip.IpAddressInRange(kv.Key, kv.Value));
        }

        /// <summary>
        /// 是否是禁区
        /// </summary>
        /// <param name="ips"></param>
        /// <returns></returns>
        public static bool IsInDenyArea(this string ips)
        {
            var denyAreas = SystemSettings.GetOrAdd("DenyArea", "").Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
            if (denyAreas.Any())
            {
                foreach (var item in ips.Split(','))
                {
                    var pos = GetIPLocation(item);
                    return pos.Contains(denyAreas) || denyAreas.Intersect(pos.Split("|")).Any();
                }
            }

            return false;
        }

        private static readonly DbSearcher IPSearcher = new DbSearcher(Path.Combine(AppContext.BaseDirectory + "App_Data", "ip2region.db"));
        private static readonly DatabaseReader MaxmindReader = new DatabaseReader(Path.Combine(AppContext.BaseDirectory + "App_Data", "GeoLite2-City.mmdb"));

        public static string GetIPLocation(this string ips)
        {
            return ips.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s =>
            {
                var ip = IPAddress.Parse(s.Trim());
                switch (ip.AddressFamily)
                {
                    case AddressFamily.InterNetwork when ip.IsPrivateIP():
                    case AddressFamily.InterNetworkV6 when ip.IsPrivateIP():
                        return "内网IP";
                    case AddressFamily.InterNetwork:
                        return IPSearcher.MemorySearch(ip.ToString())?.Region;
                    default:
                        var response = MaxmindReader.City(ip);
                        return response.Country.Names.GetValueOrDefault("zh-CN") + response.City.Names.GetValueOrDefault("zh-CN");
                }
            }).Join(" , ");
        }

        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Mapper<T>(this object source) where T : class => Startup.ServiceProvider.GetRequiredService<IMapper>().Map<T>(source);

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="tos">收件人</param>
        [AutomaticRetry(Attempts = 1)]
        public static void SendMail(string title, string content, string tos)
        {
#if !DEBUG
            new Email()
            {
                EnableSsl = bool.Parse(SystemSettings.GetOrAdd("EnableSsl", "true")),
                Body = content,
                SmtpServer = SystemSettings["SMTP"],
                Username = SystemSettings["EmailFrom"],
                Password = SystemSettings["EmailPwd"],
                SmtpPort = SystemSettings["SmtpPort"].ToInt32(),
                Subject = title,
                Tos = tos
            }.Send();
#endif
        }

        /// <summary>
        /// 是否是机器人访问
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsRobot(this HttpRequest req)
        {
            return req.Headers[HeaderNames.UserAgent].ToString().Contains(new[]
            {
                "DNSPod",
                "Baidu",
                "spider",
                "Python",
                "bot"
            });
        }

        /// <summary>
        /// 清理html的img标签的除src之外的其他属性
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ClearImgAttributes(this string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.Descendants("img");
            foreach (var node in nodes)
            {
                node.Attributes.RemoveWhere(a => !new[] { "src", "data-original", "width", "style", "class" }.Contains(a.Name));
            }

            return doc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// 将html的img标签的src属性名替换成data-original
        /// </summary>
        /// <param name="html"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string ReplaceImgAttribute(this string html, string title)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodes = doc.DocumentNode.Descendants("img");
            foreach (var node in nodes)
            {
                if (node.Attributes.Contains("src"))
                {
                    string src = node.Attributes["src"].Value;
                    node.Attributes.Remove("src");
                    node.Attributes.Add("data-original", src);
                    node.Attributes.Add("alt", SystemSettings["Title"]);
                    node.Attributes.Add("title", title);
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// 获取文章摘要
        /// </summary>
        /// <param name="html"></param>
        /// <param name="length">截取长度</param>
        /// <param name="min">摘要最少字数</param>
        /// <returns></returns>
        public static string GetSummary(this string html, int length = 150, int min = 10)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var summary = doc.DocumentNode.Descendants("p").FirstOrDefault(n => n.InnerText.Length > min)?.InnerText ?? "没有摘要";
            if (summary.Length > length)
            {
                return summary.Substring(0, length) + "...";
            }

            return summary;
        }

        public static string TrimQuery(this string path)
        {
            return path.Split('&').Where(s => s.Split('=', StringSplitOptions.RemoveEmptyEntries).Length == 2).Join("&");
        }

        /// <summary>
        /// 添加水印
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Stream AddWatermark(this Stream stream)
        {
            if (!string.IsNullOrEmpty(SystemSettings.GetOrAdd("Watermark", string.Empty)))
            {
                try
                {
                    var watermarker = new ImageWatermarker(stream)
                    {
                        SkipWatermarkForSmallImages = true,
                        SmallImagePixelsThreshold = 40000
                    };
                    return watermarker.AddWatermark(SystemSettings["Watermark"], Color.LightGray, WatermarkPosition.BottomRight, 30);
                }
                catch
                {
                    //
                }
            }
            return stream;
        }
    }
}