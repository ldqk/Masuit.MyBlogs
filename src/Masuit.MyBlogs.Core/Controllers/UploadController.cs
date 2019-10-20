using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions.UEditor;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Masuit.Tools.Logging;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 文件上传
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UploadController : Controller
    {
        public IWebHostEnvironment HostEnvironment { get; set; }

        private readonly ImagebedClient _imagebedClient;

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <param name="HostEnvironment"></param>
        public UploadController(ImagebedClient imagebedClient)
        {
            _imagebedClient = imagebedClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isTrue"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public ActionResult ResultData(object data, bool isTrue = true, string message = "")
        {
            return Content(JsonConvert.SerializeObject(new
            {
                Success = isTrue,
                Message = message,
                Data = data
            }, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            }), "application/json", Encoding.UTF8);
        }

        #region Word上传转码

        /// <summary>
        /// 上传Word转码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadWord()
        {
            var files = Request.Form.Files;
            if (files.Count <= 0)
            {
                return ResultData(null, false, "请先选择您需要上传的文件!");
            }

            var file = files[0];
            string fileName = file.FileName;
            if (fileName == null)
            {
                return ResultData(null, false, "请先选择您需要上传的文件!");
            }
            if (!Regex.IsMatch(Path.GetExtension(fileName), "doc|docx"))
            {
                return ResultData(null, false, "文件格式不支持，只能上传doc或者docx的文档!");
            }

            string upload = HostEnvironment.WebRootPath + "/upload";
            if (!Directory.Exists(upload))
            {
                Directory.CreateDirectory(upload);
            }

            string resourceName = string.Empty.CreateShortToken(9);
            string ext = Path.GetExtension(fileName);
            string docPath = Path.Combine(upload, resourceName + ext);
            using (FileStream fs = new FileStream(docPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                file.CopyTo(fs);
            }
            string htmlDir = docPath.Replace(".docx", "").Replace(".doc", "");
            DocumentConvert.Doc2Html(docPath, htmlDir);
            string htmlfile = Path.Combine(htmlDir, "index.html");
            string html = System.IO.File.ReadAllText(htmlfile).ReplaceHtmlImgSource("/upload/" + resourceName).ClearHtml().HtmlSantinizerStandard();
            ThreadPool.QueueUserWorkItem(state => System.IO.File.Delete(htmlfile));
            if (html.Length < 10)
            {
                Directory.Delete(htmlDir, true);
                System.IO.File.Delete(docPath);
                return ResultData(null, false, "读取文件内容失败，请检查文件的完整性，建议另存后重新上传！");
            }

            if (html.Length > 1000000)
            {
                Directory.Delete(htmlDir, true);
                System.IO.File.Delete(docPath);
                return ResultData(null, false, "文档内容超长，服务器拒绝接收，请优化文档内容后再尝试重新上传！");
            }

            return ResultData(new
            {
                Title = Path.GetFileNameWithoutExtension(fileName),
                Content = html,
                ResourceName = resourceName + ext
            });
        }

        #endregion

        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [Route("download")]
        [Route("download/{path}")]
        public ActionResult Download(string path)
        {
            if (string.IsNullOrEmpty(path)) return Content("null");
            var file = Path.Combine(HostEnvironment.WebRootPath + "/upload", path.Trim('.', '/', '\\'));
            if (System.IO.File.Exists(file))
            {
                return this.ResumePhysicalFile(file, Path.GetFileName(file));
            }

            return Content("null");
        }

        /// <summary>
        /// UEditor文件上传处理
        /// </summary>
        /// <returns></returns>
        [Route("fileuploader")]
        public ActionResult UeditorFileUploader()
        {
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            Handler action = new NotSupportedHandler(HttpContext);
            switch (Request.Query["action"])//通用
            {
                case "config":
                    action = new ConfigHandler(HttpContext);
                    break;
                case "uploadimage":
                    action = new UploadHandler(HttpContext, new UploadConfig()
                    {
                        AllowExtensions = UeditorConfig.GetStringList("imageAllowFiles"),
                        PathFormat = UeditorConfig.GetString("imagePathFormat"),
                        SizeLimit = UeditorConfig.GetInt("imageMaxSize"),
                        UploadFieldName = UeditorConfig.GetString("imageFieldName")
                    });
                    break;
                case "uploadscrawl":
                    action = new UploadHandler(HttpContext, new UploadConfig()
                    {
                        AllowExtensions = new[] { ".png" },
                        PathFormat = UeditorConfig.GetString("scrawlPathFormat"),
                        SizeLimit = UeditorConfig.GetInt("scrawlMaxSize"),
                        UploadFieldName = UeditorConfig.GetString("scrawlFieldName"),
                        Base64 = true,
                        Base64Filename = "scrawl.png"
                    });
                    break;
                case "catchimage":
                    action = new CrawlerHandler(HttpContext);
                    break;
            }

            if (user.IsAdmin)
            {
                switch (Request.Query["action"])//管理员用
                {
                    //case "uploadvideo":
                    //    action = new UploadHandler(context, new UploadConfig()
                    //    {
                    //        AllowExtensions = UeditorConfig.GetStringList("videoAllowFiles"),
                    //        PathFormat = UeditorConfig.GetString("videoPathFormat"),
                    //        SizeLimit = UeditorConfig.GetInt("videoMaxSize"),
                    //        UploadFieldName = UeditorConfig.GetString("videoFieldName")
                    //    });
                    //    break;
                    case "uploadfile":
                        action = new UploadHandler(HttpContext, new UploadConfig()
                        {
                            AllowExtensions = UeditorConfig.GetStringList("fileAllowFiles"),
                            PathFormat = UeditorConfig.GetString("filePathFormat"),
                            SizeLimit = UeditorConfig.GetInt("fileMaxSize"),
                            UploadFieldName = UeditorConfig.GetString("fileFieldName")
                        });
                        break;
                        //case "listimage":
                        //    action = new ListFileManager(context, UeditorConfig.GetString("imageManagerListPath"), UeditorConfig.GetStringList("imageManagerAllowFiles"));
                        //    break;
                        //case "listfile":
                        //    action = new ListFileManager(context, UeditorConfig.GetString("fileManagerListPath"), UeditorConfig.GetStringList("fileManagerAllowFiles"));
                        //    break;
                }
            }

            string result = action.Process();
            return Content(result, ContentType.Json);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("upload"), ApiExplorerSettings(IgnoreApi = false)]
        public async Task<ActionResult> UploadFile(IFormFile file)
        {
            string path;
            string filename = SnowFlake.GetInstance().GetUniqueId() + Path.GetExtension(file.FileName);
            switch (file.ContentType)
            {
                case var _ when file.ContentType.StartsWith("image"):
                    var (url, success) = await _imagebedClient.UploadImage(file.OpenReadStream(), file.FileName);
                    if (success)
                    {
                        return ResultData(url);
                    }

                    path = Path.Combine(HostEnvironment.WebRootPath, "upload", "images", filename);
                    var dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        file.CopyTo(fs);
                    }

                    break;
                case var _ when file.ContentType.StartsWith("audio") || file.ContentType.StartsWith("video"):
                    path = Path.Combine(HostEnvironment.WebRootPath, "upload", "media", filename);
                    break;
                case var _ when file.ContentType.StartsWith("text") || (ContentType.Doc + "," + ContentType.Xls + "," + ContentType.Ppt + "," + ContentType.Pdf).Contains(file.ContentType):
                    path = Path.Combine(HostEnvironment.WebRootPath, "upload", "docs", filename);
                    break;
                default:
                    path = Path.Combine(HostEnvironment.WebRootPath, "upload", "files", filename);
                    break;
            }
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    file.CopyTo(fs);
                }

                return ResultData(path.Substring(HostEnvironment.WebRootPath.Length).Replace("\\", "/"));
            }
            catch (Exception e)
            {
                LogManager.Error(e);
                return ResultData(null, false, "文件上传失败！");
            }
        }
    }
}