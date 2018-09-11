using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Common;
using Hangfire;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.Tools.Net;
using Masuit.Tools.Systems;
using Models.DTO;
using Models.Entity;
using Models.Enum;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class BugController : BaseController
    {
        public IIssueBll IssueBll { get; set; }
        public IInternalMessageBll MessageBll { get; set; }

        public BugController(IIssueBll issueBll, IInternalMessageBll messageBll)
        {
            IssueBll = issueBll;
            MessageBll = messageBll;
        }

        [Route("bug")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PageData(int page = 1, int size = 10, string kw = "")
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            List<Issue> list = string.IsNullOrEmpty(kw) ? IssueBll.LoadPageEntitiesNoTracking(page, size, out int total, i => i.Level != BugLevel.Fatal || user.IsAdmin, i => i.SubmitTime, false).ToList() : IssueBll.LoadPageEntitiesNoTracking(page, size, out total, i => (i.Level != BugLevel.Fatal || user.IsAdmin) && (i.Description.Contains(kw) || i.Title.Contains(kw) || i.Name.Contains(kw) || i.Email.Contains(kw) || i.Link.Contains(kw)), i => i.SubmitTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list.Select(i => new
            {
                i.Id,
                i.Name,
                i.Email,
                i.Title,
                i.Link,
                i.Description,
                i.SubmitTime,
                i.HandleTime,
                Status = i.Status.GetDisplay(),
                Level = i.Level.GetDisplay()
            }), pageCount, total);
        }

        [Route("bug/{id:int}")]
        public ActionResult Datails(int id)
        {
            Issue issue = IssueBll.GetById(id);
            if (issue is null)
            {
                return RedirectToAction("Index", "Error");
            }
            return View(issue);
        }

        [Authority]
        public ActionResult Handle(int id, string text)
        {
            Issue issue = IssueBll.GetById(id);
            issue.Status = Status.Handled;
            issue.HandleTime = DateTime.Now;
            issue.Msg = text;
            bool b = IssueBll.UpdateEntitySaved(issue);
            string content = System.IO.File.ReadAllText(Request.MapPath("/template/bugfeed.html")).Replace("{{title}}", issue.Title).Replace("{{link}}", issue.Link).Replace("{{text}}", text).Replace("{{date}}", issue.HandleTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            BackgroundJob.Enqueue(() => CommonHelper.SendMail("bug提交反馈通知", content, issue.Email));
            return ResultData(null, b, b ? "问题处理成功！" : "处理失败！");
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Submit(Issue issue)
        {
            issue.Description = Regex.Replace(issue.Description, @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>");
            issue.IPAddress = Request.UserHostAddress;
            Issue bug = IssueBll.AddEntitySaved(issue);
            if (bug != null)
            {
                MessageBll.AddEntitySaved(new InternalMessage()
                {
                    Title = $"来自【{issue.Name}({issue.Email})】的bug问题反馈",
                    Content = bug.Description,
                    Link = Url.Action("Index")
                });
                string content = System.IO.File.ReadAllText(Request.MapPath("/template/bugreport.html")).Replace("{{name}}", bug.Name).Replace("{{email}}", bug.Email).Replace("{{title}}", bug.Title).Replace("{{desc}}", bug.Description).Replace("{{link}}", bug.Link).Replace("{{date}}", bug.SubmitTime.ToString("yyyy-MM-dd HH:mm:ss"));
                BackgroundJob.Enqueue(() => CommonHelper.SendMail("bug提交通知", content, "admin@masuit.com"));
                return ResultData(issue, true, "问题提交成功，感谢您的反馈！");
            }
            return ResultData(null, false, "提交失败！");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            bool b = IssueBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        public ActionResult GetBugLevels()
        {
            List<object> list = new List<object>();
            foreach (Enum value in Enum.GetValues(typeof(BugLevel)))
            {
                list.Add(new
                {
                    name = value.GetDisplay(),
                    value
                });
            }
            return ResultData(list);
        }
    }
}