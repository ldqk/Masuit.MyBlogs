using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 控制面板
    /// </summary>
    public class DashboardController : AdminController
    {
        /// <summary>
        /// 控制面板
        /// </summary>
        /// <returns></returns>
        [Route("dashboard"), ResponseCache(Duration = 60, VaryByHeader = "Cookie")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("counter")]
        public ActionResult Counter()
        {
            return View();
        }

        /// <summary>
        /// 获取站内消息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMessages([FromServices] IPostService postService, [FromServices] ILeaveMessageService leaveMessageService, [FromServices] ICommentService commentService)
        {
            var post = postService.GetQuery(p => p.Status == Status.Pending).Select(p => new
            {
                p.Id,
                p.Title,
                p.PostDate,
                p.Author
            }).ToList();
            var msgs = leaveMessageService.GetQuery(m => m.Status == Status.Pending).Select(p => new
            {
                p.Id,
                p.PostDate,
                p.NickName
            }).ToList();
            var comments = commentService.GetQuery(c => c.Status == Status.Pending).Select(p => new
            {
                p.Id,
                p.CommentDate,
                p.PostId,
                p.NickName
            }).ToList();
            return ResultData(new
            {
                post,
                msgs,
                comments
            });
        }

        /// <summary>
        /// 获取日志文件列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLogfiles()
        {
            List<string> files = Directory.GetFiles(LogManager.LogDirectory).OrderByDescending(s => s).Select(Path.GetFileName).ToList();
            return ResultData(files);
        }

        /// <summary>
        /// 查看日志
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult Catlog(string filename)
        {
            if (System.IO.File.Exists(Path.Combine(LogManager.LogDirectory, filename)))
            {
                string text = System.IO.File.ReadAllText(Path.Combine(LogManager.LogDirectory, filename));
                return ResultData(text);
            }
            return ResultData(null, false, "文件不存在！");
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ActionResult DeleteFile(string filename)
        {
            try
            {
                System.IO.File.Delete(Path.Combine(LogManager.LogDirectory, filename));
                return ResultData(null, message: "文件删除成功!");
            }
            catch (IOException)
            {
                return ResultData(null, false, "文件删除失败！");
            }
        }

        /// <summary>
        /// 资源管理器
        /// </summary>
        /// <returns></returns>
        [Route("filemanager"), ResponseCache(Duration = 60, VaryByHeader = "Cookie")]
        public ActionResult FileManager()
        {
            return View();
        }
    }
}