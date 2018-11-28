using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using EFSecondLevelCache;
using Masuit.Tools;
#if !DEBUG
using Masuit.Tools.Models;
#endif
using Masuit.Tools.NoSQL;
using Models.Application;
using Models.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common
{
    /// <summary>
    /// 公共类库
    /// </summary>
    public static class CommonHelper
    {
        static CommonHelper()
        {
            BanRegex = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "ban.txt"));
            ModRegex = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "mod.txt"));
            DenyIP = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "denyip.txt"));
            DenyAreaIP = JsonConvert.DeserializeObject<ConcurrentDictionary<string, HashSet<string>>>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "denyareaip.txt")));
            string[] lines = File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "DenyIPRange.txt"));
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

            IPWhiteList = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "whitelist.txt")).Split(',', '，');
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
        /// 按地区禁用ip
        /// </summary>
        public static ConcurrentDictionary<string, HashSet<string>> DenyAreaIP { get; set; }

        /// <summary>
        /// ip白名单
        /// </summary>
        public static IEnumerable<string> IPWhiteList { get; set; }

        /// <summary>
        /// 每IP错误的次数统计
        /// </summary>
        public static ConcurrentDictionary<string, int> IPErrorTimes { get; set; } = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 访问量
        /// </summary>
        public static double InterviewCount
        {
            get
            {
                try
                {
                    using (var redisHelper = RedisHelper.GetInstance())
                    {
                        return redisHelper.GetString<double>("Interview:ViewCount");
                    }
                }
                catch
                {
                    return 1;
                }
            }
            set
            {
            }
        }

        /// <summary>
        /// 访问量
        /// </summary>
        public static double Todaypv
        {
            get
            {
                try
                {
                    using (var redisHelper = RedisHelper.GetInstance())
                    {
                        return redisHelper.ListRange<Interview>($"Interview:{DateTime.Today:yyyy:MM:dd}").Count;
                    }
                }
                catch
                {
                    return 1;
                }
            }
            set
            {
            }
        }

        /// <summary>
        /// 网站运营开始时间
        /// </summary>
        public static int RunningDays
        {
            get
            {
                try
                {
                    using (var redisHelper = RedisHelper.GetInstance())
                    {
                        int days = redisHelper.GetServer().Keys(0, "Interview:20*").Count();
                        return days;
                    }
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
        /// 在线人数
        /// </summary>
        public static int OnlineCount
        {
            get
            {
                try
                {
                    using (var redisHelper = RedisHelper.GetInstance(1))
                    {
                        return redisHelper.GetServer().Keys(1, "Session:*").Count();
                    }
                }
                catch
                {
                    return 1;
                }
            }
        }

        public static Dictionary<string, string> DenyIPRange { get; set; }

        /// <summary>
        /// 类型映射
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Mapper<T>(this object source) where T : class => AutoMapper.Mapper.Map<T>(source);

        /// <summary>
        /// 获取设置参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetSettings(string key)
        {
            using (var db = new DataContext())
            {
                return db.SystemSetting.Cacheable().FirstOrDefault(s => s.Name.Equals(key))?.Value;
            }
        }

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

            return DenyAreaIP.SelectMany(x => x.Value).Union(DenyIP.Split(',')).Contains(ip) || DenyIPRange.Any(kv => kv.Key.StartsWith(ip.Split('.')[0]) && ip.IpAddressInRange(kv.Key, kv.Value));
        }

        #region 性能历史数据

        public static List<object[]> HistoryCpuLoad { get; set; } = new List<object[]>();
        public static List<object[]> HistoryMemoryUsage { get; set; } = new List<object[]>();
        public static List<object[]> HistoryCpuTemp { get; set; } = new List<object[]>();
        public static List<object[]> HistoryIORead { get; set; } = new List<object[]>();
        public static List<object[]> HistoryIOWrite { get; set; } = new List<object[]>();
        public static List<object[]> HistoryNetSend { get; set; } = new List<object[]>();
        public static List<object[]> HistoryNetReceive { get; set; } = new List<object[]>();

        #endregion

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
                SmtpServer = GetSettings("smtp"),
                Username = GetSettings("EmailFrom"),
                Password = GetSettings("EmailPwd"),
                SmtpPort = GetSettings("SmtpPort").ToInt32(),
                Subject = title,
                Tos = tos
            }.Send();
#endif
        }

        ///// <summary>
        ///// 上传图片到新浪图床
        ///// </summary>
        ///// <param name="file">文件名</param>
        ///// <returns></returns>
        //public static (string, bool) UploadImage(string file)
        //{
        //    string ext = Path.GetExtension(file);
        //    if (!File.Exists(file))
        //    {
        //        return ("", false);
        //    }

        //    if (new FileInfo(file).Length > 5 * 1024 * 1024)
        //    {
        //        if (!ext.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase))
        //        {
        //            return ("", false);
        //        }

        //        using (var stream = File.OpenRead(file))
        //        {
        //            string newfile = Path.Combine(Environment.GetEnvironmentVariable("temp"), "".CreateShortToken(16) + ext);
        //            using (FileStream fs = new FileStream(newfile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //            {
        //                ImageUtilities.CompressImage(stream, fs, 100, 5120);
        //                return UploadImage(newfile);
        //            }
        //        }
        //    }

        //    string[] apis =
        //    {
        //        "https://zs.mtkan.cc/upload.php",
        //        "https://tu.zsczys.com/Zs_UP.php?type=multipart",
        //        "https://api.yum6.cn/sinaimg.php?type=multipart",
        //        "http://180.165.190.225:672/v1/upload"
        //    };
        //    int index = 0;
        //    string url = string.Empty;
        //    bool success = false;
        //    for (int i = 0; i < 30; i++)
        //    {
        //        using (HttpClient httpClient = new HttpClient())
        //        {
        //            httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
        //            httpClient.DefaultRequestHeaders.Referrer = new Uri(apis[i]);
        //            using (var stream = File.OpenRead(file))
        //            {
        //                using (var bc = new StreamContent(stream))
        //                {
        //                    bc.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        //                    {
        //                        FileName = "".MDString() + ext,
        //                        Name = "file"
        //                    };
        //                    using (var content = new MultipartFormDataContent
        //                    {
        //                        bc
        //                    })
        //                    {
        //                        var code = httpClient.PostAsync(apis[index], content).ContinueWith(t =>
        //                        {
        //                            if (t.IsCanceled || t.IsFaulted)
        //                            {
        //                                //Console.WriteLine("发送请求出错了" + t.Exception.Message);
        //                                t.Exception.InnerExceptions.ForEach(e =>
        //                                {
        //                                    Console.WriteLine("发送请求出错了：" + e);
        //                                });
        //                                return 0;
        //                            }

        //                            var res = t.Result;
        //                            if (res.IsSuccessStatusCode)
        //                            {
        //                                try
        //                                {
        //                                    string s = res.Content.ReadAsStringAsync().Result;
        //                                    var token = JObject.Parse(s);
        //                                    switch (index)
        //                                    {
        //                                        case 0:
        //                                            s = (string)token["original_pic"];
        //                                            if (!s.Contains("Array.jpg"))
        //                                            {
        //                                                url = s;
        //                                                return 1;
        //                                            }

        //                                            return 2;
        //                                        case 1:
        //                                            s = (string)token["code"];
        //                                            if (s.Equals("200"))
        //                                            {
        //                                                url = (string)token["pid"];
        //                                                return 1;
        //                                            }

        //                                            return 2;
        //                                        case 2:
        //                                            s = (string)token["code"];
        //                                            if (s.Equals("200"))
        //                                            {
        //                                                url = ((string)token["url"]).Replace("thumb150", "large");
        //                                                return 1;
        //                                            }

        //                                            return 2;
        //                                        case 3:
        //                                            try
        //                                            {
        //                                                url = "https://wx2.sinaimg.cn/large/" + token["wbpid"] + "." + token["type"];
        //                                                return 1;
        //                                            }
        //                                            catch
        //                                            {
        //                                                return 2;
        //                                            }
        //                                    }
        //                                }
        //                                catch (Exception e)
        //                                {
        //                                    Console.WriteLine(e.Message);
        //                                    return 2;
        //                                }
        //                            }

        //                            return 2;
        //                        }).Result;
        //                        if (code == 1)
        //                        {
        //                            success = true;
        //                            break;
        //                        }

        //                        if (code == 0)
        //                        {
        //                            if (i % 10 == 9)
        //                            {
        //                                index++;
        //                                if (index == apis.Length)
        //                                {
        //                                    Console.WriteLine("所有上传接口都挂掉了，重试sm.ms图床");
        //                                    return UploadImageFallback(file);
        //                                }

        //                                Console.WriteLine("正在准备重试图片上传接口：" + apis[index]);
        //                                continue;
        //                            }
        //                        }

        //                        if (code == 2)
        //                        {
        //                            index++;
        //                            if (index == apis.Length)
        //                            {
        //                                Console.WriteLine("所有上传接口都挂掉了，重试sm.ms图床");
        //                                return UploadImageFallback(file);
        //                            }

        //                            Console.WriteLine("正在准备重试图片上传接口：" + apis[index]);
        //                            continue;
        //                        }

        //                        Console.WriteLine("准备重试上传图片");
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return (url.Replace("/thumb150/", "/large/"), success);
        //}

        /// <summary>
        /// 上传图片到sm图床
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns></returns>
        private static (string, bool) UploadImageFallback(string file)
        {
            string url = String.Empty;
            bool success = false;
            string ext = Path.GetExtension(file);
            for (int i = 0; i < 10; i++)
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                    using (var stream = File.OpenRead(file))
                    {
                        using (var bc = new StreamContent(stream))
                        {
                            bc.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = "".CreateShortToken() + ext,
                                Name = "smfile"
                            };
                            using (var content = new MultipartFormDataContent
                            {
                                bc
                            })
                            {
                                var code = httpClient.PostAsync("https://sm.ms/api/upload?inajax=1&ssl=1", content).ContinueWith(t =>
                                {
                                    if (t.IsCanceled || t.IsFaulted)
                                    {
                                        //Console.WriteLine("发送请求出错了" + t.Exception);
                                        t.Exception.InnerExceptions.ForEach(e =>
                                        {
                                            Console.WriteLine("发送请求出错了：" + e);
                                        });
                                        return 0;
                                    }

                                    var res = t.Result;
                                    if (res.IsSuccessStatusCode)
                                    {
                                        string s = res.Content.ReadAsStringAsync().Result;
                                        var token = JObject.Parse(s);
                                        url = (string)token["data"]["url"];
                                        return 1;
                                    }

                                    return 2;
                                }).Result;
                                if (code == 1)
                                {
                                    success = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (success)
            {
                return (url, success);
            }

            return UploadImageFallback2(file);
        }

        /// <summary>
        /// 上传图片到百度图床
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns></returns>
        private static (string, bool) UploadImageFallback2(string file)
        {
            string url = String.Empty;
            bool success = false;
            string ext = Path.GetExtension(file);
            for (int i = 0; i < 10; i++)
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                    using (var stream = File.OpenRead(file))
                    {
                        using (var bc = new StreamContent(stream))
                        {
                            bc.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = "".CreateShortToken() + ext,
                                Name = "uploaded_file[]"
                            };
                            using (var content = new MultipartFormDataContent
                            {
                                bc
                            })
                            {
                                var code = httpClient.PostAsync("https://upload.cc/image_upload", content).ContinueWith(t =>
                                {
                                    if (t.IsCanceled || t.IsFaulted)
                                    {
                                        //Console.WriteLine("发送请求出错了" + t.Exception);
                                        t.Exception.InnerExceptions.ForEach(e =>
                                        {
                                            Console.WriteLine("发送请求出错了：" + e);
                                        });
                                        return 0;
                                    }

                                    var res = t.Result;
                                    if (res.IsSuccessStatusCode)
                                    {
                                        string s = res.Content.ReadAsStringAsync().Result;
                                        var token = JObject.Parse(s);
                                        if ((int)token["code"] == 100)
                                        {
                                            url = "https://upload.cc/" + (string)token["success_image"][0]["url"];
                                            return 1;
                                        }
                                    }

                                    return 2;
                                }).Result;
                                if (code == 1)
                                {
                                    success = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (success)
            {
                return (url, success);
            }

            return UploadImageFallback3(file);
        }

        /// <summary>
        /// 上传图片到百度图床
        /// </summary>
        /// <param name="file">文件名</param>
        /// <returns></returns>
        private static (string, bool) UploadImageFallback3(string file)
        {
            string url = String.Empty;
            bool success = false;
            string ext = Path.GetExtension(file);
            for (int i = 0; i < 10; i++)
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
                    using (var stream = File.OpenRead(file))
                    {
                        using (var bc = new StreamContent(stream))
                        {
                            bc.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                            {
                                FileName = "".CreateShortToken() + ext,
                                Name = "img"
                            };
                            using (var content = new MultipartFormDataContent
                            {
                                bc
                            })
                            {
                                var code = httpClient.PostAsync("http://upload.otar.im/api/upload/imgur", content).ContinueWith(t =>
                                {
                                    if (t.IsCanceled || t.IsFaulted)
                                    {
                                        //Console.WriteLine("发送请求出错了" + t.Exception);
                                        t.Exception.InnerExceptions.ForEach(e =>
                                        {
                                            Console.WriteLine("发送请求出错了：" + e);
                                        });
                                        return 0;
                                    }

                                    var res = t.Result;
                                    if (res.IsSuccessStatusCode)
                                    {
                                        string s = res.Content.ReadAsStringAsync().Result;
                                        var token = JObject.Parse(s);
                                        url = (string)token["url"];
                                        return 1;
                                    }

                                    return 2;
                                }).Result;
                                if (code == 1)
                                {
                                    success = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return (url, success);
        }

        public static string ReplaceImgSrc(string content)
        {
            //var mc = Regex.Matches(content, "<img.+?src=\"(.+?)\".+?>", RegexOptions.Multiline);
            //foreach (Match m in mc)
            //{
            //    var src = m.Groups[1].Value;
            //    if (src.StartsWith("/"))
            //    {
            //        var path = Path.Combine(AppContext.BaseDirectory, src.Replace("/", @"\").Substring(1));
            //        if (File.Exists(path))
            //        {
            //            var (url, success) = UploadImage(path);
            //            if (success)
            //            {
            //                content = content.Replace(src, url);
            //                BackgroundJob.Enqueue(() => File.Delete(path));
            //            }
            //        }
            //    }
            //}

            return content;
        }
    }
}