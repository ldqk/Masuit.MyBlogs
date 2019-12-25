using Aliyun.OSS;
using Hangfire;
using Masuit.MyBlogs.Core.Configs;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Systems;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Polly;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace Masuit.MyBlogs.Core.Common
{
    /// <summary>
    /// 图床客户端
    /// </summary>
    public class ImagebedClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        /// <summary>
        /// 图床客户端
        /// </summary>
        /// <param name="httpClientFactory"></param>
        /// <param name="config"></param>
        public ImagebedClient(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// OSS客户端
        /// </summary>
        public static OssClient OssClient { get; set; } = new OssClient(AppConfig.AliOssConfig.EndPoint, AppConfig.AliOssConfig.AccessKeyId, AppConfig.AliOssConfig.AccessKeySecret);

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<(string url, bool success)> UploadImage(Stream stream, string file)
        {
            return await UploadGitlab(stream, file);
        }

        /// <summary>
        /// gitlab图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<(string url, bool success)> UploadGitlab(Stream stream, string file)
        {
            if (AppConfig.GitlabConfigs.Any(c => c.FileLimitSize >= stream.Length))
            {
                var gitlab = AppConfig.GitlabConfigs.Where(c => c.FileLimitSize >= stream.Length).OrderBy(c => Guid.NewGuid()).FirstOrDefault();
                if (gitlab.ApiUrl.Contains("gitee.com"))
                {
                    return await UploadGitee(gitlab, stream, file);
                }

                var path = $"{DateTime.Now:yyyy/MM/dd}/{SnowFlake.NewId + Path.GetExtension(file)}";
                _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", gitlab.AccessToken);
                return await _httpClient.PostAsJsonAsync(gitlab.ApiUrl.Contains("/v3/") ? gitlab.ApiUrl : gitlab.ApiUrl + HttpUtility.UrlEncode(path), new
                {
                    file_path = path,
                    branch_name = gitlab.Branch,
                    branch = gitlab.Branch,
                    author_email = CommonHelper.SystemSettings["ReceiveEmail"],
                    author_name = SnowFlake.NewId,
                    encoding = "base64",
                    content = Convert.ToBase64String(stream.ToArray()),
                    commit_message = SnowFlake.NewId
                }).ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        using var resp = t.Result;
                        using var content = resp.Content;
                        if (resp.IsSuccessStatusCode || content.ReadAsStringAsync().Result.Contains("already exists"))
                        {
                            return (gitlab.RawUrl + path, true);
                        }
                    }

                    return UploadOss(stream, file);
                });
            }

            return UploadOss(stream, file);
        }

        /// <summary>
        /// 码云图床
        /// </summary>
        /// <param name="config"></param>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<(string url, bool success)> UploadGitee(GitlabConfig config, Stream stream, string file)
        {
            var path = $"{DateTime.Now:yyyy/MM/dd}/{Path.GetFileName(file)}";
            return await _httpClient.PostAsJsonAsync(config.ApiUrl + HttpUtility.UrlEncode(path), new
            {
                access_token = config.AccessToken,
                content = Convert.ToBase64String(stream.ToArray()),
                message = SnowFlake.NewId
            }).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    using var resp = t.Result;
                    using var content = resp.Content;
                    if (resp.IsSuccessStatusCode || content.ReadAsStringAsync().Result.Contains("already exists"))
                    {
                        return (config.RawUrl + path, true);
                    }
                }

                return UploadOss(stream, file);
            });
        }

        /// <summary>
        /// 阿里云Oss图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private (string url, bool success) UploadOss(Stream stream, string file)
        {
            if (!AppConfig.AliOssConfig.Enabled)
            {
                return UploadSmms(stream, file).Result;
            }
            var objectName = DateTime.Now.ToString("yyyyMMdd") + "/" + SnowFlake.NewId + Path.GetExtension(file);
            return Policy.Handle<Exception>().Retry(5, (e, i) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }).Wrap(Policy<(string url, bool success)>.Handle<Exception>().Fallback(() => UploadSmms(stream, file).Result)).Execute(() =>
            {
                var result = OssClient.PutObject(AppConfig.AliOssConfig.BucketName, objectName, stream);
                return result.HttpStatusCode == HttpStatusCode.OK ? (AppConfig.AliOssConfig.BucketDomain + "/" + objectName, true) : UploadSmms(stream, file).Result;
            });
        }

        /// <summary>
        /// 上传图片到sm图床
        /// </summary>
        /// <returns></returns>
        public async Task<(string url, bool success)> UploadSmms(Stream stream, string file)
        {
            string url = string.Empty;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
            using var bc = new StreamContent(stream);
            bc.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(file),
                Name = "smfile"
            };
            using var content = new MultipartFormDataContent { bc };
            var success = await _httpClient.PostAsync("https://sm.ms/api/upload?inajax=1&ssl=1", content).ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted || !t.Result.IsSuccessStatusCode)
                {
                    return false;
                }

                try
                {
                    var s = t.Result.Content.ReadAsStringAsync().Result;
                    var token = JObject.Parse(s);
                    url = (string)token["data"]["url"];
                    return true;
                }
                catch
                {
                    return false;
                }
            });

            return (url, success);
        }

        /// <summary>
        /// 替换img标签的src属性
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<string> ReplaceImgSrc(string content)
        {
            if (bool.TryParse(_config["Imgbed:EnableLocalStorage"], out var b) && b)
            {
                return content;
            }

            var srcs = content.MatchImgSrcs();
            foreach (string src in srcs)
            {
                if (src.StartsWith("http"))
                {
                    continue;
                }

                var path = Path.Combine(AppContext.BaseDirectory + "wwwroot", src.Replace("/", @"\").Substring(1));
                if (!File.Exists(path))
                {
                    continue;
                }

                await using var stream = File.OpenRead(path);
                var (url, success) = await UploadImage(stream, path);
                if (success)
                {
                    content = content.Replace(src, url);
                    BackgroundJob.Enqueue(() => File.Delete(path));
                }
            }

            return content;
        }
    }
}