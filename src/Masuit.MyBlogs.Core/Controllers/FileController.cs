using Common;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.Files;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    [Route("[controller]/[action]")]
    public class FileController : AdminController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISevenZipCompressor _sevenZipCompressor;

        /// <summary>
        /// 资源管理器
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        /// <param name="sevenZipCompressor"></param>
        public FileController(IHostingEnvironment hostingEnvironment, ISevenZipCompressor sevenZipCompressor)
        {
            _hostingEnvironment = hostingEnvironment;
            _sevenZipCompressor = sevenZipCompressor;
        }

        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult GetFiles(string path)
        {
            var files = Directory.GetFiles(_hostingEnvironment.WebRootPath + path).OrderByDescending(s => s).Select(s => new
            {
                filename = Path.GetFileName(s),
                path = s
            }).ToList();
            return ResultData(files);
        }

        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult Read(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                string text = System.IO.File.ReadAllText(filename);
                return ResultData(text);
            }
            return ResultData(null, false, "文件不存在！");
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public ActionResult Save(string filename, string content)
        {
            try
            {
                System.IO.File.WriteAllText(filename, content);
                return ResultData(null, message: "保存成功");
            }
            catch (IOException e)
            {
                LogManager.Error(GetType(), e);
                return ResultData(null, false, "保存失败");
            }
        }

        /// <summary>
        /// 操作文件
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Handle([FromBody]FileRequest req)
        {
            List<object> list = new List<object>();
            var prefix = CommonHelper.SystemSettings["PathRoot"].Trim('\\', '/');
            switch (req.Action)
            {
                case "list":
                    string path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + req.Path : prefix + req.Path; //_hostingEnvironment.WebRootPath + (req.Path);
                    string[] dirs = Directory.GetDirectories(path);
                    string[] files = Directory.GetFiles(path);
                    dirs.ForEach(s =>
                    {
                        DirectoryInfo dirinfo = new DirectoryInfo(s);
                        list.Add(new FileList()
                        {
                            date = dirinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            name = dirinfo.Name,
                            rights = "drwxrwxrwx",
                            size = 0,
                            type = "dir"
                        });
                    });
                    files.ForEach(s =>
                    {
                        FileInfo info = new FileInfo(s);
                        list.Add(new FileList()
                        {
                            name = info.Name,
                            rights = "-rw-rw-rw-",
                            size = info.Length,
                            date = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            type = "file"
                        });
                    });
                    break;
                case "remove":
                    req.Items.ForEach(s =>
                    {
                        s = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + s : prefix + s;
                        try
                        {
                            System.IO.File.Delete(s);
                        }
                        catch
                        {
                            Directory.Delete(s, true);
                        }
                    });
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "rename":
                case "move":
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + req.Item : prefix + req.Item;
                    var newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + req.NewItemPath : prefix + req.NewItemPath;
                    if (!string.IsNullOrEmpty(req.Item))
                    {
                        try
                        {
                            System.IO.File.Move(path, newpath);
                        }
                        catch
                        {
                            Directory.Move(path, newpath);
                        }
                    }
                    else
                    {
                        newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + req.NewPath : prefix + req.NewPath;
                        req.Items.ForEach(s =>
                        {
                            try
                            {
                                System.IO.File.Move(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (s) : prefix + s, Path.Combine(newpath, Path.GetFileName(s)));
                            }
                            catch
                            {
                                Directory.Move(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (s) : prefix + s, Path.Combine(newpath, Path.GetFileName(s)));
                            }
                        });
                    }
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "copy":
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.Item) : prefix + req.Item;
                    newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.NewItemPath) : prefix + req.NewItemPath;
                    //newpath = _hostingEnvironment.WebRootPath + (req.NewItemPath);
                    if (!string.IsNullOrEmpty(req.Item))
                    {
                        System.IO.File.Copy(path, newpath);
                    }
                    else
                    {
                        newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.NewPath) : prefix + req.NewPath;
                        //_hostingEnvironment.WebRootPath + (req.NewPath);
                        req.Items.ForEach(s => System.IO.File.Copy(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (s) : prefix + s, !string.IsNullOrEmpty(req.SingleFilename) ? Path.Combine(newpath, req.SingleFilename) : Path.Combine(newpath, Path.GetFileName(s))));
                    }
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "edit":
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.Item) : prefix + req.Item;
                    //path = _hostingEnvironment.WebRootPath + (req.Item);
                    string content = req.Content;
                    System.IO.File.WriteAllText(path, content, Encoding.UTF8);
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "getContent":
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.Item) : prefix + req.Item;
                    //path = _hostingEnvironment.WebRootPath + (req.Item);
                    content = System.IO.File.ReadAllText(path, Encoding.UTF8);
                    return Json(new
                    {
                        result = content
                    });
                case "createFolder":
                    string dir = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.NewPath) : prefix + req.NewPath;
                    //string dir = _hostingEnvironment.WebRootPath + (req.NewPath);
                    var directoryInfo = Directory.CreateDirectory(dir);
                    list.Add(new
                    {
                        success = directoryInfo.Exists.ToString()
                    });
                    break;
                case "changePermissions":
                    break;
                case "compress":
                    string filename = Path.Combine(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.Destination) : prefix + req.Destination, Path.GetFileNameWithoutExtension(req.CompressedFilename) + ".zip");
                    _sevenZipCompressor.Zip(req.Items.Select(s => string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (s) : prefix + s).ToList(), filename);

                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "extract":
                    string folder = Path.Combine(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.Destination) : prefix + req.Destination, req.FolderName.Trim('/', '\\'));
                    string zip = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.Item) : prefix + req.Item;
                    _sevenZipCompressor.Extract(zip, folder);
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                default:
                    var httpfiles = Request.Form.Files;
                    if (httpfiles.Count > 0)
                    {
                        foreach (var t in httpfiles)
                        {
                            path = Path.Combine(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (req.Destination) : prefix + req.Destination, t.FileName);
                            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                            {
                                t.CopyTo(fs);
                            }
                        }
                    }
                    break;
            }
            return Json(new
            {
                result = list
            });
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="items"></param>
        /// <param name="toFilename"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Handle(string path, string[] items, string toFilename)
        {
            var prefix = CommonHelper.SystemSettings["PathRoot"].Trim('\\', '/');
            switch (Request.Query["action"])
            {
                case "download":
                    string file = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (path) : prefix + path;
                    //_hostingEnvironment.WebRootPath + (path);
                    if (System.IO.File.Exists(file))
                    {
                        return this.ResumePhysicalFile(file, Path.GetFileName(file));
                    }
                    break;
                case "downloadMultiple":
                    byte[] buffer = _sevenZipCompressor.ZipStream(items.Select(s => string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? _hostingEnvironment.WebRootPath + (s) : prefix + s).ToList()).ToArray();
                    return this.ResumeFile(buffer, Path.GetFileName(toFilename));
            }
            return Content("null");
        }
    }
}