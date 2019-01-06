using Common;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Files;
using Masuit.Tools.Logging;
using Masuit.Tools.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class FileController : AdminController
    {
        public List<string> FileList { get; set; } = new List<string>();

        public ActionResult GetFiles(string path)
        {
            var files = Directory.GetFiles(Request.MapPath(path)).OrderByDescending(s => s).Select(s => new
            {
                filename = Path.GetFileName(s),
                path = s
            }).ToList();
            return ResultData(files);
        }

        public ActionResult Read(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                string text = System.IO.File.ReadAllText(filename);
                return ResultData(text);
            }
            return ResultData(null, false, "文件不存在！");
        }

        [ValidateInput(false)]
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

        [HttpPost]
        public ActionResult Handle(FileRequest req)
        {
            List<object> list = new List<object>();
            var prefix = CommonHelper.GetSettings("PathRoot").Trim('\\', '/');
            switch (req.Action)
            {
                case "list":
                    string path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Path) : prefix + req.Path; //Server.MapPath(req.Path);
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
                        s = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(s) : prefix + s;
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
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Item) : prefix + req.Item;
                    var newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.NewItemPath) : prefix + req.NewItemPath;
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
                        newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.NewPath) : prefix + req.NewPath;
                        req.Items.ForEach(s =>
                        {
                            try
                            {
                                System.IO.File.Move(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(s) : prefix + s, Path.Combine(newpath, Path.GetFileName(s)));
                            }
                            catch
                            {
                                Directory.Move(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(s) : prefix + s, Path.Combine(newpath, Path.GetFileName(s)));
                            }
                        });
                    }
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "copy":
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Item) : prefix + req.Item;
                    newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.NewItemPath) : prefix + req.NewItemPath;
                    //newpath = Server.MapPath(req.NewItemPath);
                    if (!string.IsNullOrEmpty(req.Item))
                    {
                        System.IO.File.Copy(path, newpath);
                    }
                    else
                    {
                        newpath = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.NewPath) : prefix + req.NewPath;
                        //Server.MapPath(req.NewPath);
                        req.Items.ForEach(s => System.IO.File.Copy(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(s) : prefix + s, !string.IsNullOrEmpty(req.SingleFilename) ? Path.Combine(newpath, req.SingleFilename) : Path.Combine(newpath, Path.GetFileName(s))));
                    }
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "edit":
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Item) : prefix + req.Item;
                    //path = Server.MapPath(req.Item);
                    string content = req.Content;
                    System.IO.File.WriteAllText(path, content, Encoding.UTF8);
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "getContent":
                    path = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Item) : prefix + req.Item;
                    //path = Server.MapPath(req.Item);
                    content = System.IO.File.ReadAllText(path, Encoding.UTF8);
                    return Json(new
                    {
                        result = content
                    }, JsonRequestBehavior.AllowGet);
                case "createFolder":
                    string dir = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.NewPath) : prefix + req.NewPath;
                    //string dir = Server.MapPath(req.NewPath);
                    var directoryInfo = Directory.CreateDirectory(dir);
                    list.Add(new
                    {
                        success = directoryInfo.Exists.ToString()
                    });
                    break;
                case "changePermissions":
                    break;
                case "compress":
                    string filename = Path.Combine(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Destination) : prefix + req.Destination, Path.GetFileNameWithoutExtension(req.CompressedFilename) + ".zip");
                    SevenZipCompressor.Zip(req.Items.Select(s => string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(s) : prefix + s).ToList(), filename);

                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                case "extract":
                    string folder = Path.Combine(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Destination) : prefix + req.Destination, req.FolderName.Trim('/', '\\'));
                    string zip = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Item) : prefix + req.Item;
                    SevenZipCompressor.Extract(zip, folder);
                    list.Add(new
                    {
                        success = "true"
                    });
                    break;
                default:
                    var httpfiles = Request.Files;
                    if (httpfiles.Count > 0)
                    {
                        for (var i = 0; i < httpfiles.Count; i++)
                        {
                            path = Path.Combine(string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(req.Destination) : prefix + req.Destination, httpfiles[i].FileName);
                            httpfiles[i].SaveAs(path);
                        }
                    }
                    break;
            }
            return Json(new
            {
                result = list
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Handle(string path, string[] items, string toFilename)
        {
            var prefix = CommonHelper.GetSettings("PathRoot").Trim('\\', '/');
            switch (Request["action"])
            {
                case "download":
                    string file = string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(path) : prefix + path;
                    //Server.MapPath(path);
                    if (System.IO.File.Exists(file))
                    {
                        return this.ResumePhysicalFile(file, "application/octet-stream", Path.GetFileName(file));
                    }
                    break;
                case "downloadMultiple":
                    byte[] buffer = SevenZipCompressor.ZipStream(items.Select(s => string.IsNullOrEmpty(prefix) && !Directory.Exists(prefix) ? Server.MapPath(s) : prefix + s).ToList()).ToArray();
                    return File(buffer, "application/octet-stream", Path.GetFileName(toFilename));
            }
            return Content("null");
        }
    }
}