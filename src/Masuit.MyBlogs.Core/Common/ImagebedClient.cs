using Aliyun.OSS;
using Hangfire;
using Masuit.MyBlogs.Core.Configs;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Logging;
using Masuit.Tools.Systems;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            if (stream.Length < 51200)
            {
                return (null, false);
            }

            file = SnowFlake.NewId + Path.GetExtension(file);
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    return await UploadGitlab(stream, file);
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
            }

            return UploadOss(stream, file);
        }

        private readonly List<string> _failedList = new List<string>();

        /// <summary>
        /// gitlab图床
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private async Task<(string url, bool success)> UploadGitlab(Stream stream, string file)
        {
            var gitlabs = AppConfig.GitlabConfigs.Where(c => c.FileLimitSize >= stream.Length && !_failedList.Contains(c.ApiUrl)).OrderBy(c => Guid.NewGuid()).ToList();
            if (gitlabs.Count > 0)
            {
                var gitlab = gitlabs[0];
                if (gitlab.ApiUrl.Contains("gitee.com"))
                {
                    return await UploadGitee(gitlab, stream, file);
                }

                var path = $"{DateTime.Now:yyyy/MM/dd}/{file}";
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

                    LogManager.Info($"图片上传到gitlab({gitlab.ApiUrl})失败。");
                    _failedList.Add(gitlab.ApiUrl);
                    throw t.Exception ?? new Exception(t.Result.ReasonPhrase);
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
            var path = $"{DateTime.Now:yyyy/MM/dd}/{file}";
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

                LogManager.Info("图片上传到gitee失败。");
                throw t.Exception ?? new Exception(t.Result.ReasonPhrase);
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
                return (null, false);
            }

            var objectName = DateTime.Now.ToString("yyyy/MM/dd/") + file;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return OssClient.PutObject(AppConfig.AliOssConfig.BucketName, objectName, stream).HttpStatusCode == HttpStatusCode.OK ? (AppConfig.AliOssConfig.BucketDomain + "/" + objectName, true) : (null, false);
                }
                catch (Exception e)
                {
                    LogManager.Info($"图片上传到oss失败，重试{i}：" + e.Message);
                }
            }

            return (null, false);
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