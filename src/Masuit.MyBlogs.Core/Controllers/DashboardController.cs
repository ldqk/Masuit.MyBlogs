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
        /// PostService
        /// </summary>
        public IPostService PostService { get; set; }
        /// <summary>
        /// LeaveMessageService
        /// </summary>
        public ILeaveMessageService LeaveMessageService { get; set; }
        /// <summary>
        /// CommentService
        /// </summary>
        public ICommentService CommentService { get; set; }

        /// <summary>
        /// 控制面板
        /// </summary>
        /// <param name="userInfoService"></param>
        /// <param name="postService"></param>
        /// <param name="commentService"></param>
        /// <param name="leaveMessageService"></param>
        public DashboardController(IUserInfoService userInfoService, IPostService postService, ICommentService commentService, ILeaveMessageService leaveMessageService)
        {
            UserInfoService = userInfoService;
            CommentService = commentService;
            LeaveMessageService = leaveMessageService;
            PostService = postService;
        }

        /// <summary>
        /// 控制面板
        /// </summary>
        /// <returns></returns>
        [Route("dashboard")]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取站内消息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMessages()
        {
            var post = PostService.LoadEntitiesFromL2CacheNoTracking(p => p.Status == Status.Pending).Select(p => new
            {
                p.Id,
                p.Title,
                p.PostDate,
                p.Author
            });
            var msgs = LeaveMessageService.LoadEntitiesFromL2CacheNoTracking(m => m.Status == Status.Pending).Select(p => new
            {
                p.Id,
                p.PostDate,
                p.NickName
            });
            var comments = CommentService.LoadEntitiesFromL2CacheNoTracking(c => c.Status == Status.Pending).Select(p => new
            {
                p.Id,
                p.CommentDate,
                p.PostId,
                p.NickName
            });
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
        [Route("filemanager")]
        public ActionResult FileManager()
        {
            return View();
        }
    }
}