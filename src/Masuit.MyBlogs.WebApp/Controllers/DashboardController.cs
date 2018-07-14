using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Logging;
using Models.Enum;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    [Authority]
    public class DashboardController : AdminController
    {
        public IPostBll PostBll { get; set; }
        public ILeaveMessageBll LeaveMessageBll { get; set; }
        public ICommentBll CommentBll { get; set; }

        public DashboardController(IUserInfoBll userInfoBll, IPostBll postBll, ICommentBll commentBll, ILeaveMessageBll leaveMessageBll)
        {
            UserInfoBll = userInfoBll;
            CommentBll = commentBll;
            LeaveMessageBll = leaveMessageBll;
            PostBll = postBll;
        }

        [Route("dashboard")]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetMessages()
        {
            var post = (await PostBll.LoadEntitiesFromCacheNoTrackingAsync(p => p.Status == Status.Pending).ConfigureAwait(false)).Select(p => new
            {
                p.Id,
                p.Title,
                p.PostDate,
                p.Author
            });
            var msgs = (await LeaveMessageBll.LoadEntitiesFromCacheNoTrackingAsync(m => m.Status == Status.Pending).ConfigureAwait(false)).Select(p => new
            {
                p.Id,
                p.PostDate,
                p.NickName
            });
            var comments = (await CommentBll.LoadEntitiesFromCacheNoTrackingAsync(c => c.Status == Status.Pending).ConfigureAwait(false)).Select(p => new
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

        public ActionResult GetLogfiles()
        {
            List<string> files = Directory.GetFiles(LogManager.LogDirectory).OrderByDescending(s => s).Select(Path.GetFileName).ToList();
            return ResultData(files);
        }

        public ActionResult Catlog(string filename)
        {
            if (System.IO.File.Exists(Path.Combine(LogManager.LogDirectory, filename)))
            {
                string text = System.IO.File.ReadAllText(Path.Combine(LogManager.LogDirectory, filename));
                return ResultData(text);
            }
            return ResultData(null, false, "文件不存在！");
        }

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

        [Route("filemanager")]
        public ActionResult FileManager()
        {
            return View();
        }
    }
}