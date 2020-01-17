using HtmlAgilityPack;
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

        public ImagebedClient ImagebedClient { get; set; }

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
        public async Task<ActionResult> UploadWord()
        {
            var files = Request.Form.Files;
            if (files.Count <= 0)
            {
                return ResultData(null, false, "请先选择您需要上传的文件!");
            }

            var file = files[0];
            string fileName = file.FileName;
            if (!Regex.IsMatch(Path.GetExtension(fileName), "doc|docx"))
            {
                return ResultData(null, false, "文件格式不支持，只能上传doc或者docx的文档!");
            }

            await using var ms = file.OpenReadStream().SaveAsMemoryStream();
            var html = await SaveAsHtml(ms, file);
            if (html.Length < 10)
            {
                return ResultData(null, false, "读取文件内容失败，请检查文件的完整性，建议另存后重新上传！");
            }

            if (html.Length > 1000000)
            {
                return ResultData(null, false, "文档内容超长，服务器拒绝接收，请优化文档内容后再尝试重新上传！");
            }

            return ResultData(new
            {
                Title = Path.GetFileNameWithoutExtension(fileName),
                Content = html
            });
        }

        private async Task<string> SaveAsHtml(MemoryStream ms, IFormFile file)
        {
            var html = DocxToHtml.Docx.ConvertToHtml(ms);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var body = doc.DocumentNode.SelectSingleNode("//body");
            var style = doc.DocumentNode.SelectSingleNode("//style");
            var nodes = body.SelectNodes("//img");
            if (nodes != null)
            {
                foreach (var img in nodes)
                {
                    var attr = img.Attributes["src"].Value;
                    var strs = attr.Split(",");
                    var base64 = strs[1];
                    var bytes = Convert.FromBase64String(base64);
                    var ext = strs[0].Split(";")[0].Split("/")[1];
                    await using var image = new MemoryStream(bytes);
                    var imgFile = $"{SnowFlake.NewId}.{ext}";
                    var (url, success) = await ImagebedClient.UploadImage(image, imgFile);
                    if (success)
                    {
                        img.Attributes["src"].Value = url;
                    }
                    else
                    {
                        var path = Path.Combine(HostEnvironment.WebRootPath, "upload", "images", imgFile);
                        await SaveFile(file, path);
                        img.Attributes["src"].Value = path.Substring(HostEnvironment.WebRootPath.Length).Replace("\\", "/");
                    }
                }
            }

            return style.OuterHtml + body.InnerHtml.HtmlSantinizerStandard().HtmlSantinizerCustom(attributes: new[] { "dir", "lang" });
        }

        private static async Task SaveFile(IFormFile file, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            file.CopyTo(fs);
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
            var action = Request.Query["action"].ToString() switch //通用
            {
                "config" => (Handler)new ConfigHandler(HttpContext),
                "uploadimage" => new UploadHandler(HttpContext, new UploadConfig()
                {
                    AllowExtensions = UeditorConfig.GetStringList("imageAllowFiles"),
                    PathFormat = UeditorConfig.GetString("imagePathFormat"),
                    SizeLimit = UeditorConfig.GetInt("imageMaxSize"),
                    UploadFieldName = UeditorConfig.GetString("imageFieldName")
                }),
                "uploadscrawl" => new UploadHandler(HttpContext, new UploadConfig()
                {
                    AllowExtensions = new[]
                    {
                        ".png"
                    },
                    PathFormat = UeditorConfig.GetString("scrawlPathFormat"),
                    SizeLimit = UeditorConfig.GetInt("scrawlMaxSize"),
                    UploadFieldName = UeditorConfig.GetString("scrawlFieldName"),
                    Base64 = true,
                    Base64Filename = "scrawl.png"
                }),
                "catchimage" => new CrawlerHandler(HttpContext),
                _ => new NotSupportedHandler(HttpContext)
            };

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
                    var (url, success) = await ImagebedClient.UploadImage(file.OpenReadStream(), file.FileName);
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

                    await using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
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
                await SaveFile(file, path);
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