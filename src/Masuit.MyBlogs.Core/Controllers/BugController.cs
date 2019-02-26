using Common;
using Hangfire;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Application;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.RequestModels;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// bug反馈
    /// </summary>
    public class BugController : BaseController
    {
        /// <summary>
        /// IssueService
        /// </summary>
        private IIssueService IssueService { get; set; }

        /// <summary>
        /// MessageService
        /// </summary>
        private IInternalMessageService MessageService { get; set; }

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISearchEngine<DataContext> _searchEngine;

        /// <summary>
        /// bug反馈
        /// </summary>
        /// <param name="issueService"></param>
        /// <param name="messageService"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="searchEngine"></param>
        public BugController(IIssueService issueService, IInternalMessageService messageService, IHostingEnvironment hostingEnvironment, ISearchEngine<DataContext> searchEngine)
        {
            IssueService = issueService;
            MessageService = messageService;
            _hostingEnvironment = hostingEnvironment;
            _searchEngine = searchEngine;
        }

        /// <summary>
        /// bug首页
        /// </summary>
        /// <returns></returns>
        [Route("bug"), ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PageData([FromBody]PageFilter filter)
        {
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            List<Issue> list;
            int total;
            if (string.IsNullOrEmpty(filter.Kw))
            {
                list = IssueService.LoadPageEntitiesFromL2CacheNoTracking(filter.Page, filter.Size, out total, i => i.Status != Status.Handled && i.Level != BugLevel.Fatal || user.IsAdmin, i => i.SubmitTime, false).ToList();
            }
            else
            {
                var searchResult = IssueService.SearchPage(filter.Page, filter.Size, filter.Kw);
                total = searchResult.Total;
                list = searchResult.Results;
            }

            var pageCount = Math.Ceiling(total * 1.0 / filter.Size).ToInt32();
            return PageResult(list.Select(i => new
            {
                i.Id,
                i.Name,
                //i.Email,
                i.Title,
                i.Link,
                i.Description,
                i.SubmitTime,
                i.HandleTime,
                Status = i.Status.GetDisplay(),
                Level = i.Level.GetDisplay()
            }), pageCount, total);
        }

        /// <summary>
        /// 问题详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("bug/{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Datails(int id)
        {
            Issue issue = IssueService.GetById(id);
            if (issue is null)
            {
                return RedirectToAction("Index", "Error");
            }
            return View(issue);
        }

        /// <summary>
        /// 处理
        /// </summary>
        /// <returns></returns>
        [Authority,]
        public ActionResult Handle([FromBody]IssueHandleRequest req)
        {
            Issue issue = IssueService.GetById(req.Id);
            issue.Status = Status.Handled;
            issue.HandleTime = DateTime.Now;
            issue.Msg = req.Text;
            IssueService.UpdateEntity(issue);
            bool b = _searchEngine.SaveChanges() > 0;
            string content = System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/bugfeed.html").Replace("{{title}}", issue.Title).Replace("{{link}}", issue.Link).Replace("{{text}}", req.Text).Replace("{{date}}", issue.HandleTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            BackgroundJob.Enqueue(() => CommonHelper.SendMail("bug提交反馈通知", content, issue.Email));
            return ResultData(null, b, b ? "问题处理成功！" : "处理失败！");
        }

        /// <summary>
        /// 提交问题
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Submit([FromBody]Issue issue)
        {
            issue.Description = CommonHelper.ReplaceImgSrc(Regex.Replace(issue.Description, @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            issue.IPAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            IssueService.AddEntity(issue);
            bool b = _searchEngine.SaveChanges(issue.Level < BugLevel.Fatal) > 0;
            if (b)
            {
                MessageService.AddEntitySaved(new InternalMessage()
                {
                    Title = $"来自【{issue.Name}({issue.Email})】的bug问题反馈",
                    Content = issue.Description,
                    Link = Url.Action("Index")
                });
                string content = System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/bugreport.html").Replace("{{name}}", issue.Name).Replace("{{email}}", issue.Email).Replace("{{title}}", issue.Title).Replace("{{desc}}", issue.Description).Replace("{{link}}", issue.Link).Replace("{{date}}", issue.SubmitTime.ToString("yyyy-MM-dd HH:mm:ss"));
                BackgroundJob.Enqueue(() => CommonHelper.SendMail("bug提交通知", content, "admin@masuit.com"));
                return ResultData(issue, true, "问题提交成功，感谢您的反馈！");
            }
            return ResultData(null, false, "提交失败！");
        }

        /// <summary>
        /// 删除问题
        /// </summary>
        /// <param name="req">问题id</param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete([FromBody]RequestModelBase req)
        {
            bool b = IssueService.DeleteByIdSaved(req.Id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取问题级别
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600)]
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