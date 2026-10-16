using FreeRedis;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.Files;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Authorization;
using Polly;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 资源管理器
/// </summary>
[Route("[controller]/[action]")]
public sealed class FileController : AdminController
{
    private sealed record DownloadManifest(string[] Paths, bool IsArchive, string FileName);

    public IWebHostEnvironment HostEnvironment { get; set; }

    /// <summary>
    ///
    /// </summary>
    public ISevenZipCompressor SevenZipCompressor { get; set; }

    public IRedisClient RedisClient { get; set; }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public ActionResult GetFiles([FromBodyOrDefault] string path)
    {
        var files = Directory.GetFiles(HostEnvironment.WebRootPath + path).OrderByDescending(s => s).Select(s => new
        {
            filename = Path.GetFileName(s),
            path = s
        });
        return ResultData(files);
    }

    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public ActionResult Read([FromBodyOrDefault] string filename)
    {
        if (System.IO.File.Exists(filename))
        {
            string text = new FileInfo(filename).ShareReadWrite().ReadAllText(Encoding.UTF8);
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
    public ActionResult Save([FromBodyOrDefault] string filename, [FromBodyOrDefault] string content)
    {
        try
        {
            new FileInfo(filename).ShareReadWrite().WriteAllText(content, Encoding.UTF8);
            return ResultData(null, message: "保存成功");
        }
        catch (IOException e)
        {
            LogManager.Error(GetType(), e.Demystify());
            return ResultData(null, false, "保存失败");
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> Upload([FromBodyOrDefault] string destination)
    {
        var form = await Request.ReadFormAsync();
        foreach (var t in form.Files)
        {
            string path = Path.Combine(HostEnvironment.ContentRootPath, CommonHelper.SystemSettings["PathRoot"].TrimStart('\\', '/'), destination.TrimStart('\\', '/'), t.FileName);
            await using var fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            await t.CopyToAsync(fs);
        }
        return Json(new
        {
            result = new List<object>()
        });
    }

    /// <summary>
    /// 操作文件
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult Handle([FromBody] FileRequest req)
    {
        var list = new List<object>();
        var root = Path.Combine(HostEnvironment.ContentRootPath, CommonHelper.SystemSettings["PathRoot"].TrimStart('\\', '/'));
        switch (req.Action)
        {
            case "list":
                var path = Path.Combine(root, req.Path.TrimStart('\\', '/'));
                var dirs = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);
                list.AddRange(dirs.Select(s => new DirectoryInfo(s)).Select(dirinfo => new FileList
                {
                    date = dirinfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    name = dirinfo.Name,
                    size = 0,
                    type = "dir"
                }).Union(files.Select(s => new FileInfo(s)).Select(info => new FileList
                {
                    name = info.Name,
                    size = info.Length,
                    date = info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    type = "file"
                })));
                break;

            case "remove":
                req.Items.ForEach(s =>
                {
                    s = Path.Combine(root, s.TrimStart('\\', '/'));
                    try
                    {
                        Policy.Handle<IOException>().WaitAndRetry(5, i => TimeSpan.FromSeconds(1)).Execute(() => System.IO.File.Delete(s));
                    }
                    catch
                    {
                        Policy.Handle<IOException>().WaitAndRetry(5, i => TimeSpan.FromSeconds(1)).Execute(() => Directory.Delete(s, true));
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
                if (!string.IsNullOrEmpty(req.Item))
                {
                    System.IO.File.Copy(Path.Combine(root, req.Item.TrimStart('\\', '/')), Path.Combine(root, req.NewItemPath.TrimStart('\\', '/')), true);
                }
                else
                {
                    newpath = Path.Combine(root, req.NewPath.TrimStart('\\', '/'));
                    req.Items.ForEach(s => System.IO.File.Copy(Path.Combine(root, s.TrimStart('\\', '/')), !string.IsNullOrEmpty(req.SingleFilename) ? Path.Combine(newpath, req.SingleFilename) : Path.Combine(newpath, Path.GetFileName(s)), true));
                }
                list.Add(new
                {
                    success = "true"
                });
                break;

            case "edit":
                new FileInfo(Path.Combine(root, req.Item.TrimStart('\\', '/'))).ShareReadWrite().WriteAllText(req.Content, Encoding.UTF8);
                list.Add(new
                {
                    success = "true"
                });
                break;

            case "getContent":
                return Json(new
                {
                    result = new FileInfo(Path.Combine(root, req.Item.TrimStart('\\', '/'))).ShareReadWrite().ReadAllText(Encoding.UTF8)
                });

            case "createFolder":
                list.Add(new
                {
                    success = Directory.CreateDirectory(Path.Combine(root, req.NewPath.TrimStart('\\', '/'))).Exists.ToString()
                });
                break;

            case "compress":
                var filename = Path.Combine(Path.Combine(root, req.Destination.TrimStart('\\', '/')), Path.GetFileNameWithoutExtension(req.CompressedFilename) + ".zip");
                SevenZipCompressor.Zip(req.Items.Select(s => Path.Combine(root, s.TrimStart('\\', '/'))), filename);
                list.Add(new
                {
                    success = "true"
                });
                break;

            case "extract":
                var folder = Path.Combine(Path.Combine(root, req.Destination.TrimStart('\\', '/')), req.FolderName.Trim('/', '\\'));
                var zip = Path.Combine(root, req.Item.TrimStart('\\', '/'));
                SevenZipCompressor.Decompress(zip, folder);
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
        var root = GetDownloadRoot();
        var isArchive = items is { Length: > 0 };
        var requestedPaths = isArchive ? items : [path];
        if (requestedPaths.Any(string.IsNullOrWhiteSpace))
        {
            return BadRequest();
        }

        var fullPaths = new List<string>(requestedPaths.Length);
        foreach (var requestedPath in requestedPaths)
        {
            if (!TryResolvePathWithinRoot(root, requestedPath, out var fullPath) || !System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            fullPaths.Add(Path.GetRelativePath(root, fullPath));
        }

        var manifest = new DownloadManifest(
            fullPaths.ToArray(),
            isArchive,
            isArchive ? Path.GetFileName(toFilename) : string.Empty);
        var token = Guid.NewGuid().ToString().MDString(Guid.NewGuid().ToString()).FromBaseBig(16).ToBase(62);
        RedisClient.Set("FileManager:Token:" + token, JsonSerializer.Serialize(manifest));
        RedisClient.Expire("FileManager:Token:" + token, TimeSpan.FromDays(1));
        return RedirectToAction("Download", "File", new { token });
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="items"></param>
    /// <param name="toFilename"></param>
    /// <param name="token">访问token</param>
    /// <returns></returns>
    [HttpGet("{**path}"), AllowAnonymous]
    public ActionResult Download(string path, string[] items, string toFilename, string token)
    {
        var tokenValue = RedisClient.Get("FileManager:Token:" + token);
        if (!string.IsNullOrWhiteSpace(tokenValue))
        {
            DownloadManifest manifest;
            try
            {
                manifest = JsonSerializer.Deserialize<DownloadManifest>(tokenValue) ?? throw new NotFoundException("文件未找到");
            }
            catch (JsonException)
            {
                throw new NotFoundException("文件未找到");
            }

            var root = GetDownloadRoot();
            var fullPaths = new List<string>(manifest.Paths.Length);
            foreach (var relativePath in manifest.Paths)
            {
                if (!TryResolvePathWithinRoot(root, relativePath, out var fullPath) || !System.IO.File.Exists(fullPath))
                {
                    throw new NotFoundException("文件未找到");
                }

                fullPaths.Add(fullPath);
            }

            if (manifest.IsArchive && fullPaths.Count > 0)
            {
                using var ms = SevenZipCompressor.ZipStream(fullPaths);
                var buffer = ms.ToArray();
                return this.ResumeFile(buffer, manifest.FileName);
            }

            if (!manifest.IsArchive && fullPaths.Count == 1)
            {
                return this.ResumePhysicalFile(fullPaths[0], Path.GetFileName(fullPaths[0]));
            }
        }

        throw new NotFoundException("文件未找到");
    }

    private string GetDownloadRoot()
    {
        var configuredRoot = CommonHelper.SystemSettings["PathRoot"].TrimStart('\\', '/');
        return Path.GetFullPath(Path.Combine(HostEnvironment.ContentRootPath, configuredRoot));
    }

    private static bool TryResolvePathWithinRoot(string root, string requestedPath, out string fullPath)
    {
        fullPath = Path.GetFullPath(Path.Combine(root, requestedPath.TrimStart('\\', '/')));
        var relativePath = Path.GetRelativePath(root, fullPath);
        return !Path.IsPathRooted(relativePath)
            && relativePath != ".."
            && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }
}