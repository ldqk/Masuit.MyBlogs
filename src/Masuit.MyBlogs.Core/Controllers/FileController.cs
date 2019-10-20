using Masuit.MyBlogs.Core.Common;
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
        public IWebHostEnvironment HostEnvironment { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ISevenZipCompressor SevenZipCompressor { get; set; }

        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult GetFiles(string path)
        {
            var files = Directory.GetFiles(HostEnvironment.WebRootPath + path).OrderByDescending(s => s).Select(s => new
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
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Upload(string destination)
        {
            List<object> list = new List<object>();
            if (Request.Form.Files.Count > 0)
            {
                foreach (var t in Request.Form.Files)
                {
                    string path = Path.Combine(HostEnvironment.ContentRootPath, CommonHelper.SystemSettings["PathRoot"].TrimStart('\\', '/'), destination.TrimStart('\\', '/'), t.FileName);
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        t.CopyTo(fs);
                    }
                }
            }
            return Json(new
            {
                result = list
            });
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
            var root = Path.Combine(HostEnvironment.ContentRootPath, CommonHelper.SystemSettings["PathRoot"].TrimStart('\\', '/'));
            switch (req.Action)
            {
                case "list":
                    string path = Path.Combine(root, req.Path.TrimStart('\\', '/'));
                    string[] dirs = Directory.GetDirectories(path);
                    string[] files = Directory.GetFiles(path);
                    dirs.ForEach(s =>
                    {
                        DirectoryInfo dirinfo = new DirectoryInfo(s);
                        list.Add(new FileList()
                        {
                            date = dirinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            name = dirinfo.Name,
                            //rights = "drwxrwxrwx",
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
                            //rights = "-rw-rw-rw-",
                            size = info.Length,
                            date = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            type = "file"
                        });
                    });
                    break;
                case "remove":
                    req.Items.ForEach(s =>
                    {
                        s = Path.Combine(root, s.TrimStart('\\', '/'));
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
                    string newpath;
                    if (!string.IsNullOrEmpty(req.Item))
                    {
                        newpath = Path.Combine(root, req.NewItemPath?.TrimStart('\\', '/'));
                        path = Path.Combine(root, req.Item.TrimStart('\\', '/'));
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
                        newpath = Path.Combine(root, req.NewPath.TrimStart('\\', '/'));
                        req.Items.ForEach(s =>
                        {
                            try
                            {
                                System.IO.File.Move(Path.Combine(root, s.TrimStart('\\', '/')), Path.Combine(newpath, Path.GetFileName(s)));
                            }
                            catch
                            {
                                Directory.Move(Path.Combine(root, s.TrimStart('\\', '/')), Path.Combine(newpath, Path.GetFileName(s)));
                            }
                        });
                    }
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "copy":
                    path = Path.Combine(root, req.Item.TrimStart('\\', '/'));
                    newpath = Path.Combine(root, req.NewItemPath.TrimStart('\\', '/'));
                    if (!string.IsNullOrEmpty(req.Item))
                    {
                        System.IO.File.Copy(path, newpath);
                    }
                    else
                    {
                        newpath = Path.Combine(root, req.NewPath.TrimStart('\\', '/'));
                        req.Items.ForEach(s => System.IO.File.Copy(Path.Combine(root, s.TrimStart('\\', '/')), !string.IsNullOrEmpty(req.SingleFilename) ? Path.Combine(newpath, req.SingleFilename) : Path.Combine(newpath, Path.GetFileName(s))));
                    }
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "edit":
                    path = Path.Combine(root, req.Item.TrimStart('\\', '/'));
                    string content = req.Content;
                    System.IO.File.WriteAllText(path, content, Encoding.UTF8);
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "getContent":
                    path = Path.Combine(root, req.Item.TrimStart('\\', '/'));
                    content = System.IO.File.ReadAllText(path, Encoding.UTF8);
                    return Json(new
                    {
                        result = content
                    });
                case "createFolder":
                    string dir = Path.Combine(root, req.NewPath.TrimStart('\\', '/'));
                    var directoryInfo = Directory.CreateDirectory(dir);
                    list.Add(new
                    {
                        success = directoryInfo.Exists.ToString()
                    });
                    break;
                case "changePermissions":
                    break;
                case "compress":
                    string filename = Path.Combine(Path.Combine(root, req.Destination.TrimStart('\\', '/')), Path.GetFileNameWithoutExtension(req.CompressedFilename) + ".zip");
                    SevenZipCompressor.Zip(req.Items.Select(s => Path.Combine(root, s.TrimStart('\\', '/'))).ToList(), filename);

                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "extract":
                    string folder = Path.Combine(Path.Combine(root, req.Destination.TrimStart('\\', '/')), req.FolderName.Trim('/', '\\'));
                    string zip = Path.Combine(root, req.Item.TrimStart('\\', '/'));
                    SevenZipCompressor.Extract(zip, folder);
                    list.Add(new
                    {
                        success = "true"
                    });
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
            path = path?.TrimStart('\\', '/') ?? "";
            var root = CommonHelper.SystemSettings["PathRoot"].TrimStart('\\', '/');
            var file = Path.Combine(HostEnvironment.ContentRootPath, root, path);
            switch (Request.Query["action"])
            {
                case "download":
                    if (System.IO.File.Exists(file))
                    {
                        return this.ResumePhysicalFile(file, Path.GetFileName(file));
                    }
                    break;
                case "downloadMultiple":
                    byte[] buffer = SevenZipCompressor.ZipStream(items.Select(s => Path.Combine(HostEnvironment.ContentRootPath, root, s.TrimStart('\\', '/'))).ToList()).ToArray();
                    return this.ResumeFile(buffer, Path.GetFileName(toFilename));
            }
            return Content("null");
        }
    }
}