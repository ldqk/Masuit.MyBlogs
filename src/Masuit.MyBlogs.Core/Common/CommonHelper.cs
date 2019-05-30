using Aliyun.OSS;
using Hangfire;
using IP2Region;
using Masuit.MyBlogs.Core.Configs;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
#if !DEBUG
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Models;
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
            BanRegex = File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "ban.txt"));
            ModRegex = File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "mod.txt"));
            DenyIP = File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "denyip.txt"));
            string[] lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory + "App_Data", "DenyIPRange.txt"));
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
        public static Dictionary<string, string> SystemSettings { get; set; }

        /// <summary>
        /// 访问量
        /// </summary>
        public static double InterviewCount
        {
            get
            {
                try
                {
                    return RedisHelper.Get<double>("Interview:ViewCount");
                }
                catch
                {
                    return 1;
                }
            }
            set => RedisHelper.IncrBy("Interview:ViewCount");
        }

        /// <summary>
        /// 平均访问量
        /// </summary>
        public static double AverageCount
        {
            get
            {
                try
                {
                    return RedisHelper.Get<double>("Interview:ViewCount") / RedisHelper.Get<double>("Interview:RunningDays");
                }
                catch
                {
                    return 1;
                }
            }
        }

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

            bool denyed = DenyIP.Split(',').Contains(ip) || DenyIPRange.Any(kv => kv.Key.StartsWith(ip.Split('.')[0]) && ip.IpAddressInRange(kv.Key, kv.Value));
            if ("true" == SystemSettings["EnableDenyArea"])
            {
                using (DbSearcher searcher = new DbSearcher(Path.Combine(AppContext.BaseDirectory + "App_Data", "ip2region.db")))
                {
                    string[] region = searcher.MemorySearch(ip).Region.Split("|");
                    string[] denyAreas = SystemSettings["DenyArea"].Split(',', '，');
                    denyed = denyed || denyAreas.Intersect(region).Any();
                }
            }

            return denyed;
        }

        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Mapper<T>(this object source) where T : class => AutoMapper.Mapper.Map<T>(source);

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="tos">收件人</param>
        public static void SendMail(string title, string content, string tos)
        {
#if !DEBUG
            new Email()
            {
                EnableSsl = true,
                Body = content,
                SmtpServer = EmailConfig.Smtp,
                Username = EmailConfig.SendFrom,
                Password = EmailConfig.EmailPwd,
                SmtpPort = SystemSettings["SmtpPort"].ToInt32(),
                Subject = title,
                Tos = tos
            }.Send();
#endif
        }

        /// <summary>
        /// OSS客户端
        /// </summary>
        public static OssClient OssClient { get; set; } = new OssClient(AppConfig.AliOssConfig.EndPoint, AppConfig.AliOssConfig.AccessKeyId, AppConfig.AliOssConfig.AccessKeySecret);

        public static (string url, bool success) UploadImage(string file)
        {
            if (!AppConfig.AliOssConfig.Enabled)
            {
                return UploadSmms(file);
            }

            var objectName = DateTime.Now.ToString("yyyyMMdd") + "/" + SnowFlake.NewId + Path.GetExtension(file);
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    OssClient.PutObject(AppConfig.AliOssConfig.BucketName, objectName, file);
                    return (AppConfig.AliOssConfig.BucketDomain + "/" + objectName, true);
                }
                catch
                {
                }
            }

            return UploadSmms(file);
        }

        /// <summary>
        /// 上传图片到sm图床
        /// </summary>
        /// <returns></returns>
        public static (string url, bool success) UploadSmms(string file)
        {
            string url = string.Empty;
            bool success = false;
            using (var fs = File.OpenRead(file))
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                    using (var bc = new StreamContent(fs))
                    {
                        bc.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                        {
                            FileName = Path.GetFileName(file),
                            Name = "smfile"
                        };
                        using (var content = new MultipartFormDataContent { bc })
                        {
                            var code = httpClient.PostAsync("https://sm.ms/api/upload?inajax=1&ssl=1", content).ContinueWith(t =>
                            {
                                if (t.IsCanceled || t.IsFaulted)
                                {
                                    return 0;
                                }

                                var res = t.Result;
                                if (res.IsSuccessStatusCode)
                                {
                                    try
                                    {
                                        string s = res.Content.ReadAsStringAsync().Result;
                                        var token = JObject.Parse(s);
                                        url = (string)token["data"]["url"];
                                        return 1;
                                    }
                                    catch
                                    {
                                        return 2;
                                    }
                                }

                                return 2;
                            }).Result;
                            if (code == 1)
                            {
                                success = true;
                            }
                        }
                    }
                }
            }

            return (url, success);
        }

        /// <summary>
        /// 替换img标签的src属性
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ReplaceImgSrc(string content)
        {
            var srcs = content.MatchImgSrcs();
            foreach (string src in srcs)
            {
                if (!src.StartsWith("http"))
                {
                    var path = Path.Combine(AppContext.BaseDirectory + "wwwroot", src.Replace("/", @"\").Substring(1));
                    if (File.Exists(path))
                    {
                        var (url, success) = UploadImage(path);
                        if (success)
                        {
                            content = content.Replace(src, url);
                            BackgroundJob.Enqueue(() => File.Delete(path));
                        }
                    }
                }
            }

            return content;
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
    }
}