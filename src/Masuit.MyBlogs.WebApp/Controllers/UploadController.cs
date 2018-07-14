using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Common;
using Hangfire;
using Masuit.Tools;
using Masuit.Tools.Html;
using Masuit.Tools.Logging;
using Masuit.Tools.Media;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class UploadController : Controller
    {
        public ActionResult ResultData(object data, bool isTrue = true, string message = "")
        {
            return Content(JsonConvert.SerializeObject(new { Success = isTrue, Message = message, Data = data }, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore }), "application/json", Encoding.UTF8);
        }

        #region Word上传转码

        [HttpPost]
        public ActionResult UploadWord()
        {
            var files = Request.Files;
            if (files.Count > 0 && files[0] != null)
            {
                HttpPostedFileBase file = files[0];
                string fileName = file.FileName;
                if (fileName != null && !Regex.IsMatch(Path.GetExtension(fileName), @"doc|docx"))
                {
                    return ResultData(null, false, "文件格式不支持，只能上传doc或者docx的文档!");
                }
                if (fileName != null)
                {
                    string upload = Request.MapPath("/upload");
                    if (!Directory.Exists(upload))
                    {
                        Directory.CreateDirectory(upload);
                    }
                    string resourceName = string.Empty.CreateShortToken(9);
                    string ext = Path.GetExtension(fileName);
                    string docPath = Path.Combine(upload, resourceName + ext);
                    file.SaveAs(docPath);
                    string htmlDir = docPath.Replace(".docx", "").Replace(".doc", "");
                    DocumentConvert.Doc2Html(docPath, htmlDir);
                    string htmlfile = Path.Combine(htmlDir, "index.html");
                    string html = System.IO.File.ReadAllText(htmlfile).ReplaceHtmlImgSource("/upload/" + resourceName).ClearHtml().HtmlSantinizerStandard();
                    MatchCollection matches = Regex.Matches(html, "<img.+?src=\"(.+?)\".+?>");
                    foreach (Match m in matches)
                    {
                        string src = m.Groups[1].Value;
                        var (url, success) = CommonHelper.UploadImage(Server.MapPath(src));
                        if (success)
                        {
                            html = html.Replace(src, url);
                            BackgroundJob.Enqueue(() => System.IO.File.Delete(Server.MapPath(src)));
                        }
                    }
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
            }
            return ResultData(null, false, "请先选择您需要上传的文件!");
        }

        public ActionResult DecodeDataUri(string data)
        {
            var dir = Environment.GetEnvironmentVariable("temp");
            var filename = string.Empty.CreateShortToken(9) + ".jpg";
            string path = Path.Combine(dir, filename);
            try
            {
                data.SaveDataUriAsImageFile().Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                var (url, success) = CommonHelper.UploadImage(path);
                BackgroundJob.Enqueue(() => System.IO.File.Delete(path));
                if (success)
                {
                    return ResultData(url);
                }
                return ResultData(null, false, "图片上传失败！");
            }
            catch (Exception e)
            {
                LogManager.Error(e);
                return ResultData(null, false, "转码失败！");
            }
        }
        #endregion

        [Route("download")]
        [Route("download/{path}")]
        public ActionResult Download(string path)
        {
            if (string.IsNullOrEmpty(path)) return Content("null");
            var file = Path.Combine(Server.MapPath("/upload"), path.Trim('/', '\\'));
            if (System.IO.File.Exists(file))
            {
                return File(System.IO.File.OpenRead(file), "application/octet-stream", Path.GetFileName(file));
            }
            return Content("null");
        }
    }
}