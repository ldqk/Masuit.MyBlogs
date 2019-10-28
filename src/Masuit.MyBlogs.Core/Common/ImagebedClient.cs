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
        /// <param name="httpClient"></param>
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
        public async Task<(string url, bool success)> UploadImage(Stream stream, string file)
        {
            if (AppConfig.GitlabConfigs.Any())
            {
                return await UploadGitlab(stream, file);
            }

            if (AppConfig.AliOssConfig.Enabled)
            {
                return await UploadOss(stream, file);
            }

            return await UploadSmms(stream, file);
        }

        /// <summary>
        /// gitlab图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<(string url, bool success)> UploadGitlab(Stream stream, string file)
        {
            var gitlab = AppConfig.GitlabConfigs.OrderBy(c => Guid.NewGuid()).FirstOrDefault() ?? throw new Exception("没有可用的gitlab相关配置，请先在appsettings.json中的Imgbed:Gitlabs节点下配置gitlab");
            if (stream.Length > gitlab.FileLimitSize)
            {
                return AppConfig.AliOssConfig.Enabled ? await UploadOss(stream, file) : await UploadSmms(stream, file);
            }

            if (gitlab.ApiUrl.Contains("gitee.com"))
            {
                return await UploadGitee(gitlab, stream, file);
            }

            string base64String = Convert.ToBase64String(stream.ToByteArray());
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
                content = base64String,
                commit_message = "上传一张图片"
            });
            if (resp.IsSuccessStatusCode || (await resp.Content.ReadAsStringAsync()).Contains("already exists"))
            {
                return (gitlab.RawUrl + path, true);
            }

            return AppConfig.AliOssConfig.Enabled ? await UploadOss(stream, file) : await UploadSmms(stream, file);
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
            string base64String = Convert.ToBase64String(stream.ToByteArray());
            string path = $"{DateTime.Now:yyyy/MM/dd}/{Path.GetFileName(file)}";
            using var resp = await _httpClient.PostAsJsonAsync(config.ApiUrl + HttpUtility.UrlEncode(path), new
            {
                access_token = config.AccessToken,
                content = base64String,
                message = "上传一张图片"
            });
            if (resp.IsSuccessStatusCode || (await resp.Content.ReadAsStringAsync()).Contains("already exists"))
            {
                return (config.RawUrl + path, true);
            }

            return AppConfig.AliOssConfig.Enabled ? await UploadOss(stream, file) : await UploadSmms(stream, file);
        }

        /// <summary>
        /// 阿里云Oss图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<(string url, bool success)> UploadOss(Stream stream, string file)
        {
            var objectName = DateTime.Now.ToString("yyyyMMdd") + "/" + SnowFlake.NewId + Path.GetExtension(file);
            var policy = Policy.Handle<Exception>().Retry(5, (e, i) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ResetColor();
            });
            return policy.Wrap(Policy<(string url, bool success)>.Handle<Exception>().Fallback(() => UploadSmms(stream, file).Result)).Execute(() => OssClient.PutObject(AppConfig.AliOssConfig.BucketName, objectName, stream).HttpStatusCode == HttpStatusCode.OK ? (AppConfig.AliOssConfig.BucketDomain + "/" + objectName, true) : UploadSmms(stream, file).Result);
        }

        /// <summary>
        /// 上传图片到sm图床
        /// </summary>
        /// <returns></returns>
        public async Task<(string url, bool success)> UploadSmms(Stream stream, string file)
        {
            string url = string.Empty;
            bool success = false;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Mozilla/5.0"));
            using var bc = new StreamContent(stream);
            bc.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = Path.GetFileName(file),
                Name = "smfile"
            };
            using var content = new MultipartFormDataContent { bc };
            var code = await _httpClient.PostAsync("https://sm.ms/api/upload?inajax=1&ssl=1", content).ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                {
                    return 0;
                }

                var res = t.Result;
                if (!res.IsSuccessStatusCode)
                {
                    return 2;
                }

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
            });
            if (code == 1)
            {
                success = true;
            }

            return success ? (url, true) : await UploadPeople(stream, file);
        }

        /// <summary>
        /// 上传图片到人民网图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<(string url, bool success)> UploadPeople(Stream stream, string file)
        {
            bool success = false;
            _httpClient.DefaultRequestHeaders.UserAgent.Add(ProductInfoHeaderValue.Parse("Chrome/72.0.3626.96"));
            using var sc = new StreamContent(stream);
            using var mc = new MultipartFormDataContent
            {
                { sc, "Filedata", Path.GetFileName(file) },
                {new StringContent("."+Path.GetExtension(file)),"filetype"}
            };
            var str = await _httpClient.PostAsync("http://bbs1.people.com.cn/postImageUpload.do", mc).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    var res = t.Result;
                    if (res.IsSuccessStatusCode)
                    {
                        string result = res.Content.ReadAsStringAsync().Result;
                        string url = "http://bbs1.people.com.cn" + (string)JObject.Parse(result)["imageUrl"];
                        if (url.EndsWith(Path.GetExtension(file)))
                        {
                            success = true;
                            return url;
                        }
                    }
                }

                return "";
            });
            return (str, success);
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