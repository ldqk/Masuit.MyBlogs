using Aliyun.OSS;
using Hangfire;
using Masuit.MyBlogs.Core.Configs;
using Masuit.Tools.Html;
using Masuit.Tools.Systems;
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

        /// <summary>
        /// 图床客户端
        /// </summary>
        /// <param name="httpClientFactory"></param>
        public ImagebedClient(IHttpClientFactory httpClientFactory)
        {
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
        public (string url, bool success) UploadImage(Stream stream, string file)
        {
            return UploadOss(stream, file);
        }

        /// <summary>
        /// 阿里云Oss图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private (string url, bool success) UploadOss(Stream stream, string file)
        {
            var objectName = DateTime.Now.ToString("yyyyMMdd") + "/" + SnowFlake.NewId + Path.GetExtension(file);
            return Policy.Handle<Exception>().Retry(5, (e, i) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            }).Wrap(Policy<(string url, bool success)>.Handle<Exception>().Fallback(() => UploadGitlab(stream, file).Result)).Execute(() =>
            {
                var result = OssClient.PutObject(AppConfig.AliOssConfig.BucketName, objectName, stream);
                return result.HttpStatusCode == HttpStatusCode.OK ? (AppConfig.AliOssConfig.BucketDomain + "/" + objectName, true) : UploadGitlab(stream, file).Result;
            });
        }

        /// <summary>
        /// gitlab图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<(string url, bool success)> UploadGitlab(Stream stream, string file)
        {
            if (AppConfig.GitlabConfigs.Any())
            {
                var gitlab = AppConfig.GitlabConfigs.OrderBy(c => Guid.NewGuid()).FirstOrDefault() ?? throw new Exception("没有可用的gitlab相关配置，请先在appsettings.json中的Imgbed:Gitlabs节点下配置gitlab");
                if (stream.Length > gitlab.FileLimitSize)
                {
                    return AppConfig.AliOssConfig.Enabled ? UploadOss(stream, file) : ("", false);
                }

                if (gitlab.ApiUrl.Contains("gitee.com"))
                {
                    return await UploadGitee(gitlab, stream, file);
                }

                string path = $"{DateTime.Now:yyyy/MM/dd}/{SnowFlake.NewId + Path.GetExtension(file)}";
                _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", gitlab.AccessToken);
                using var resp = await _httpClient.PostAsJsonAsync(gitlab.ApiUrl.Contains("/v3/") ? gitlab.ApiUrl : gitlab.ApiUrl + HttpUtility.UrlEncode(path), new
                {
                    file_path = path,
                    branch_name = gitlab.Branch,
                    branch = gitlab.Branch,
                    author_email = CommonHelper.SystemSettings["ReceiveEmail"],
                    author_name = SnowFlake.NewId,
                    encoding = "base64",
                    content = Convert.ToBase64String(stream.ToByteArray()),
                    commit_message = "上传一张图片"
                });
                if (resp.IsSuccessStatusCode || (await resp.Content.ReadAsStringAsync()).Contains("already exists"))
                {
                    return (gitlab.RawUrl + path, true);
                }
            }

            return await UploadSmms(stream, file);
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
            string path = $"{DateTime.Now:yyyy/MM/dd}/{Path.GetFileName(file)}";
            using var resp = await _httpClient.PostAsJsonAsync(config.ApiUrl + HttpUtility.UrlEncode(path), new
            {
                access_token = config.AccessToken,
                content = Convert.ToBase64String(stream.ToByteArray()),
                message = "上传一张图片"
            });
            if (resp.IsSuccessStatusCode || (await resp.Content.ReadAsStringAsync()).Contains("already exists"))
            {
                return (config.RawUrl + path, true);
            }

            return await UploadSmms(stream, file);
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
                var (url, success) = UploadImage(stream, path);
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