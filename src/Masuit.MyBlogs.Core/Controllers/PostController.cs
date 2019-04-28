using AutoMapper;
using Common;
using EFSecondLevelCache.Core;
using Hangfire;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Html;
using Masuit.Tools.Security;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 文章管理
    /// </summary>
    public class PostController : BaseController
    {
        private IPostService PostService { get; set; }
        private ICategoryService CategoryService { get; set; }
        private IBroadcastService BroadcastService { get; set; }
        private ISeminarService SeminarService { get; set; }
        private IPostHistoryVersionService PostHistoryVersionService { get; set; }

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISearchEngine<DataContext> _searchEngine;

        /// <summary>
        /// 文章管理
        /// </summary>
        /// <param name="postService"></param>
        /// <param name="categoryService"></param>
        /// <param name="broadcastService"></param>
        /// <param name="seminarService"></param>
        /// <param name="postHistoryVersionService"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="searchEngine"></param>
        public PostController(IPostService postService, ICategoryService categoryService, IBroadcastService broadcastService, ISeminarService seminarService, IPostHistoryVersionService postHistoryVersionService, IHostingEnvironment hostingEnvironment, ISearchEngine<DataContext> searchEngine)
        {
            PostService = postService;
            CategoryService = categoryService;
            BroadcastService = broadcastService;
            SeminarService = seminarService;
            PostHistoryVersionService = postHistoryVersionService;
            _hostingEnvironment = hostingEnvironment;
            _searchEngine = searchEngine;
        }

        /// <summary>
        /// 文章详情页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="kw"></param>
        /// <returns></returns>
        [Route("{id:int}/{kw}"), Route("{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "kw" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Details(int id, string kw)
        {
            Post post = PostService.GetById(id);
            if (post != null)
            {
                ViewBag.Keyword = post.Keyword + "," + post.Label;
                UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
                DateTime modifyDate = post.ModifyDate;
                ViewBag.Next = PostService.GetFirstEntityNoTracking(p => p.ModifyDate > modifyDate && (p.Status == Status.Pended || user.IsAdmin), p => p.ModifyDate);
                ViewBag.Prev = PostService.GetFirstEntityNoTracking(p => p.ModifyDate < modifyDate && (p.Status == Status.Pended || user.IsAdmin), p => p.ModifyDate, false);
                if (!string.IsNullOrEmpty(kw))
                {
                    ViewData["keywords"] = post.Content.Contains(kw) ? $"['{kw}']" : _searchEngine.LuceneIndexSearcher.CutKeywords(kw).ToJsonString();
                }
                if (user.IsAdmin)
                {
                    return View("Details_Admin", post);
                }

                if (post.Status != Status.Pended)
                {
                    return RedirectToAction("Post", "Home");
                }

                if (!HttpContext.Request.IsRobot() && string.IsNullOrEmpty(HttpContext.Session.Get<string>("post" + id)))
                {
                    HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.RecordPostVisit), args: id);
                    HttpContext.Session.Set("post" + id, id.ToString());
                }

                return View(post);
            }

            return RedirectToAction("Index", "Error");
        }

        /// <summary>
        /// 文章历史版本
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Route("{id:int}/history"), Route("{id:int}/history/{page:int}/{size:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "page", "size" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult History(int id, int page = 1, int size = 20)
        {
            var p = PostService.GetById(id).Mapper<PostOutputDto>();
            if (p != null)
            {
                ViewBag.Primary = p;
                var list = PostHistoryVersionService.LoadPageEntitiesNoTracking(page, size, out int total, v => v.PostId == id, v => v.ModifyDate, false).Select(v => new PostHistoryVersion()
                {
                    PostId = id,
                    Category = v.Category,
                    ModifyDate = v.ModifyDate,
                    Title = v.Title,
                    Id = v.Id,
                    CategoryId = v.CategoryId
                }).Cacheable().ToList();
                ViewBag.Total = total;
                ViewBag.PageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
                return View(list);
            }

            return RedirectToAction("Details", "Post", new { id });
        }

        /// <summary>
        /// 文章历史版本
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hid"></param>
        /// <returns></returns>
        [Route("{id:int}/history/{hid:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "hid" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult HistoryVersion(int id, int hid)
        {
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var post = PostHistoryVersionService.GetById(hid);
            if (post is null)
            {
                return RedirectToAction("History", new
                {
                    id
                });
            }

            ViewBag.Next = PostHistoryVersionService.GetFirstEntityNoTracking(p => p.PostId == id && p.ModifyDate > post.ModifyDate, p => p.ModifyDate);
            ViewBag.Prev = PostHistoryVersionService.GetFirstEntityNoTracking(p => p.PostId == id && p.ModifyDate < post.ModifyDate, p => p.ModifyDate, false);
            if (user.IsAdmin)
            {
                return View("HistoryVersion_Admin", post);
            }

            return View(post);
        }

        /// <summary>
        /// 版本对比
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [Route("{id:int}/history/{v1:int}-{v2:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "v1", "v2" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult CompareVersion(int id, int v1, int v2)
        {
            var main = PostService.GetById(id).Mapper<PostHistoryVersion>();
            var left = v1 <= 0 ? main : PostHistoryVersionService.GetById(v1);
            var right = v2 <= 0 ? main : PostHistoryVersionService.GetById(v2);
            if (left is null || right is null)
            {
                return RedirectToAction("History", "Post", new { id });
            }
            HtmlDiff.HtmlDiff diffHelper = new HtmlDiff.HtmlDiff(right.Content, left.Content);
            string diffOutput = diffHelper.Build();
            right.Content = Regex.Replace(Regex.Replace(diffOutput, "<ins.+?</ins>", string.Empty), @"<\w+></\w+>", string.Empty);
            left.Content = Regex.Replace(Regex.Replace(diffOutput, "<del.+?</del>", string.Empty), @"<\w+></\w+>", string.Empty);
            return View(new[]
            {
                main,
                left,
                right
            });
        }

        /// <summary>
        /// 反对
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult VoteDown(int id)
        {
            Post post = PostService.GetById(id);
            if (HttpContext.Session.Get("post-vote" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            if (post != null)
            {
                HttpContext.Session.Set("post-vote" + id, id.GetBytes());
                ++post.VoteDownCount;
                PostService.UpdateEntity(post);
                var b = PostService.SaveChanges() > 0;
                return ResultData(null, b, b ? "投票成功！" : "投票失败！");
            }

            return ResultData(null, false, "非法操作");
        }

        /// <summary>
        /// 支持
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult VoteUp(int id)
        {
            Post post = PostService.GetById(id);
            if (HttpContext.Session.Get("post-vote" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            if (post != null)
            {
                HttpContext.Session.Set("post-vote" + id, id.GetBytes());
                ++post.VoteUpCount;
                PostService.UpdateEntity(post);
                var b = PostService.SaveChanges() > 0;
                return ResultData(null, b, b ? "投票成功！" : "投票失败！");
            }

            return ResultData(null, false, "非法操作");
        }

        /// <summary>
        /// 投稿页
        /// </summary>
        /// <returns></returns>
        public ActionResult Publish()
        {
            List<string> list = PostService.GetAll().Select(p => p.Label).ToList();
            List<string> result = new List<string>();
            list.ForEach(s =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    result.AddRange(s.Split(',', '，'));
                }
            });
            ViewBag.Category = CategoryService.LoadEntitiesNoTracking(c => c.Status == Status.Available).ToList();
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
            if (user != null)
            {
                return View("Publish_Admin", result.Distinct().OrderBy(s => s));
            }

            return View(result.Distinct().OrderBy(s => s));
        }

        /// <summary>
        /// 发布投稿
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Publish(PostInputDto post)
        {
            if (Regex.Match(post.Content, CommonHelper.BanRegex).Length > 0)
            {
                return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请注意改善您的言辞！");
            }

            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
            if (!CategoryService.Any(c => c.Id == post.CategoryId && c.Status == Status.Available))
            {
                return ResultData(null, message: "请选择一个分类");
            }

            if (string.IsNullOrEmpty(post.Label?.Trim()))
            {
                post.Label = null;
            }
            else if (post.Label.Trim().Length > 50)
            {
                post.Label = post.Label.Replace("，", ",").Trim().Substring(0, 50);
            }
            else
            {
                post.Label = post.Label.Replace("，", ",");
            }

            post.Status = Status.Pending;
            post.PostDate = DateTime.Now;
            post.ModifyDate = DateTime.Now;
            if (user != null && user.IsAdmin)
            {
                post.Status = Status.Pended;
            }
            else
            {
                post.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(post.Content.HtmlSantinizerStandard(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            }

            ViewBag.CategoryId = new SelectList(CategoryService.LoadEntitiesNoTracking(c => c.Status == Status.Available), "Id", "Name", post.CategoryId);
            Post p = post.Mapper<Post>();
            p.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            p.PostAccessRecord.Add(new PostAccessRecord()
            {
                AccessTime = DateTime.Today,
                ClickCount = 0
            });
            p = PostService.AddEntitySaved(p);
            if (p != null)
            {
                if (p.Status == Status.Pending)
                {
                    var email = CommonHelper.SystemSettings["ReceiveEmail"];
                    string link = Url.Action("Details", "Post", new
                    {
                        id = p.Id
                    }, Request.Scheme);
                    string content = System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/publish.html").Replace("{{link}}", link).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{title}}", p.Title);
                    BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "有访客投稿：", content, email));
                    return ResultData(p.Mapper<PostOutputDto>(), message: "文章发表成功，待站长审核通过以后将显示到列表中！");
                }

                return ResultData(p.Mapper<PostOutputDto>(), message: "文章发表成功！");
            }

            return ResultData(null, false, "文章发表失败！");
        }

        /// <summary>
        /// 获取标签
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult GetTag()
        {
            List<string> list = PostService.GetAll().Select(p => p.Label).ToList();
            List<string> result = new List<string>();
            list.ForEach(s =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    result.AddRange(s.Split(',', '，'));
                }
            });
            return ResultData(result.Distinct().OrderBy(s => s));
        }

        /// <summary>
        /// 标签云
        /// </summary>
        /// <returns></returns>
        [Route("all"), ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult All()
        {
            UserInfoOutputDto user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            List<string> tags = PostService.GetAll().Select(p => p.Label).ToList(); //tag
            List<string> result = new List<string>();
            tags.ForEach(s =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    result.AddRange(s.Split(',', '，'));
                }
            });
            ViewBag.tags = result.GroupBy(t => t).OrderByDescending(g => g.Count()).ThenBy(g => g.Key);
            ViewBag.cats = CategoryService.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel()
            {
                Id = c.Id,
                Name = c.Name,
                Count = c.Post.Count(p => p.Status == Status.Pended || user.IsAdmin)
            }).ToList(); //category
            ViewBag.seminars = SeminarService.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel
            {
                Id = c.Id,
                Name = c.Title,
                Count = c.Post.Count(p => p.Post.Status == Status.Pended || user.IsAdmin)
            }).ToList(); //seminars
            return View();
        }

        /// <summary>
        /// 检查访问密码
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CheckViewToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return ResultData(null, false, "请输入文章访问密码！");
            }

            var s = RedisHelper.Get("ArticleViewToken");
            if (token.Equals(s))
            {
                HttpContext.Session.Set("ArticleViewToken", token);
                return ResultData(null);
            }

            return ResultData(null, false, "文章访问密码不正确！");
        }

        /// <summary>
        /// 检查授权邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "email" })]
        public ActionResult GetViewToken(string email)
        {
            if (string.IsNullOrEmpty(email) && !email.MatchEmail())
            {
                return ResultData(null, false, "请输入正确的邮箱！");
            }

            if (RedisHelper.Exists("code:" + email))
            {
                RedisHelper.Expire("code:" + email, 120);
                return ResultData(null, false, "发送频率限制，请在2分钟后重新尝试发送邮件！请检查你的邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！");
            }

            if (BroadcastService.Any(b => b.Email.Equals(email) && b.SubscribeType == SubscribeType.ArticleToken))
            {
                var s = RedisHelper.Get("ArticleViewToken");
                CommonHelper.SendMail(CommonHelper.SystemSettings["Domain"] + "博客文章验证码", $"{CommonHelper.SystemSettings["Domain"]}博客文章验证码是：<span style='color:red'>{s}</span>，有效期为24h，请按时使用！", email);
                RedisHelper.Set("code:" + email, s, 120);
                return ResultData(null);
            }

            return ResultData(null, false, "您目前没有权限访问这篇文章的加密部分，请联系站长开通这篇文章的访问权限！");
        }

        #region 后端管理

        /// <summary>
        /// 固顶
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Fixtop(int id)
        {
            Post post = PostService.GetById(id);
            post.IsFixedTop = !post.IsFixedTop;
            bool b = PostService.UpdateEntitySaved(post);
            if (b)
            {
                return ResultData(null, true, post.IsFixedTop ? "置顶成功！" : "取消置顶成功！");
            }

            return ResultData(null, false, "操作失败！");
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Pass(int id)
        {
            Post post = PostService.GetById(id);
            post.Status = Status.Pended;
            post.ModifyDate = DateTime.Now;
            post.PostDate = DateTime.Now;
            bool b = PostService.UpdateEntitySaved(post);
            if (!b)
            {
                return ResultData(null, false, "审核失败！");
            }
            if ("false" == CommonHelper.SystemSettings["DisabledEmailBroadcast"])
            {
                var cast = BroadcastService.LoadEntities(c => c.Status == Status.Subscribed).ToList();
                string link = Request.Scheme + "://" + Request.Host + "/" + id;
                cast.ForEach(c =>
                {
                    var ts = DateTime.Now.GetTotalMilliseconds();
                    string content = System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/broadcast.html").Replace("{{link}}", link + "?email=" + c.Email).Replace("{{time}}", post.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{title}}", post.Title).Replace("{{author}}", post.Author).Replace("{{content}}", post.Content.RemoveHtmlTag(150)).Replace("{{cancel}}", Url.Action("Subscribe", "Subscribe", new
                    {
                        c.Email,
                        act = "cancel",
                        validate = c.ValidateCode,
                        timespan = ts,
                        hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(AppConfig.BaiduAK)
                    }, Request.Scheme));
                    BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客有新文章发布了", content, c.Email));
                });
            }

            return ResultData(null, true, "审核通过！");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete(int id)
        {
            var post = PostService.GetById(id);
            post.Status = Status.Deleted;
            bool b = PostService.UpdateEntitySaved(post);
            _searchEngine.LuceneIndexer.Delete(post);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 还原版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Restore(int id)
        {
            var post = PostService.GetById(id);
            post.Status = Status.Pended;
            bool b = PostService.UpdateEntitySaved(post);
            return ResultData(null, b, b ? "恢复成功！" : "恢复失败！");
        }

        /// <summary>
        /// 彻底删除文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Truncate(int id)
        {
            var post = PostService.GetById(id);
            if (post is null)
            {
                return ResultData(null, false, "文章已经被删除！");
            }

            if (post.IsWordDocument)
            {
                try
                {
                    System.IO.File.Delete(Path.Combine(_hostingEnvironment.WebRootPath + "/upload", post.ResourceName));
                    Directory.Delete(Path.Combine(_hostingEnvironment.WebRootPath + "/upload", Path.GetFileNameWithoutExtension(post.ResourceName)), true);
                }
                catch (IOException)
                {
                }
            }

            var srcs = post.Content.MatchImgSrcs();
            foreach (var path in srcs)
            {
                if (path.StartsWith("/"))
                {
                    try
                    {
                        System.IO.File.Delete(_hostingEnvironment.WebRootPath + path);
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            bool b = PostService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Get(int id)
        {
            Post post = PostService.GetById(id);
            PostOutputDto model = post.Mapper<PostOutputDto>();
            model.Seminars = string.Join(",", post.Seminar.Select(s => s.Seminar.Title));
            return ResultData(model);
        }

        /// <summary>
        /// 文章详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Read(int id) => ResultData(PostService.GetById(id).Mapper<PostOutputDto>());

        /// <summary>
        /// 获取所有文章
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllData()
        {
            var plist = PostService.LoadEntitiesNoTracking(p => p.Status != Status.Deleted).OrderBy(p => p.Status).ThenByDescending(p => p.IsFixedTop).ThenByDescending(p => p.ModifyDate).Select(p => new
            {
                p.Id,
                p.Author,
                CategoryName = p.Category.Name,
                p.Email,
                p.IsFixedTop,
                p.Label,
                md = p.ModifyDate,
                pd = p.PostDate,
                p.Title,
                ViewCount = p.TotalViewCount,
                p.VoteDownCount,
                p.VoteUpCount,
                stat = p.Status
            }).ToList();
            var list = new List<PostDataModel>();
            plist.ForEach(item =>
            {
                PostDataModel model = item.MapTo<PostDataModel>();
                model.PostDate = item.pd.ToString("yyyy-MM-dd HH:mm");
                model.ModifyDate = item.md.ToString("yyyy-MM-dd HH:mm");
                model.Status = item.stat.GetDisplay();
                list.Add(model);
            });
            return ResultData(list);
        }

        /// <summary>
        /// 获取文章分页
        /// </summary>
        /// <returns></returns>
        public ActionResult GetPageData(int page = 1, int size = 10, OrderBy orderby = OrderBy.ModifyDate, string kw = "")
        {
            if (page < 1)
            {
                page = 1;
            }

            if (size < 1)
            {
                page = 10;
            }

            var list = new List<PostDataModel>();
            IOrderedQueryable<Post> temp;
            var query = string.IsNullOrEmpty(kw) ? PostService.GetAllNoTracking() : PostService.LoadEntitiesNoTracking(p => p.Title.Contains(kw) || p.Author.Contains(kw) || p.Email.Contains(kw) || p.Label.Contains(kw) || p.Content.Contains(kw));
            var total = query.Count();
            var order = query.OrderByDescending(p => p.Status).ThenByDescending(p => p.IsFixedTop);
            switch (orderby)
            {
                case OrderBy.CommentCount:
                    temp = order.ThenByDescending(p => p.Comment.Count);
                    break;
                case OrderBy.PostDate:
                    temp = order.ThenByDescending(p => p.PostDate);
                    break;
                case OrderBy.ViewCount:
                    temp = order.ThenByDescending(p => p.TotalViewCount);
                    break;
                case OrderBy.VoteCount:
                    temp = order.ThenByDescending(p => p.VoteUpCount);
                    break;
                case OrderBy.AverageViewCount:
                    temp = order.ThenByDescending(p => p.AverageViewCount);
                    break;
                default:
                    temp = order.ThenByDescending(p => p.ModifyDate);
                    break;
            }

            var plist = temp.Skip((page - 1) * size).Take(size).Select(p => new
            {
                p.Id,
                p.Author,
                CategoryName = p.Category.Name,
                p.Email,
                p.IsFixedTop,
                p.Label,
                md = p.ModifyDate,
                pd = p.PostDate,
                p.Title,
                ViewCount = p.TotalViewCount,
                p.VoteDownCount,
                p.VoteUpCount,
                stat = p.Status,
                ModifyCount = p.PostHistoryVersion.Count
            }).ToList();
            plist.ForEach(item =>
            {
                PostDataModel model = item.MapTo<PostDataModel>();
                model.PostDate = item.pd.ToString("yyyy-MM-dd HH:mm");
                model.ModifyDate = item.md.ToString("yyyy-MM-dd HH:mm");
                model.Status = item.stat.GetDisplay();
                model.ModifyCount = item.ModifyCount;
                list.Add(model);
            });
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 获取未审核文章
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPending(int page = 1, int size = 10, string search = "")
        {
            int total;
            IQueryable<Post> temp;
            if (string.IsNullOrEmpty(search))
            {
                temp = PostService.LoadPageEntitiesNoTracking(page, size, out total, p => p.Status == Status.Pending, p => p.Id);
            }
            else
            {
                temp = PostService.LoadPageEntitiesNoTracking(page, size, out total, p => p.Status == Status.Pending && (p.Title.Contains(search) || p.Author.Contains(search) || p.Email.Contains(search) || p.Label.Contains(search)), p => p.Id);
            }

            var plist = temp.OrderByDescending(p => p.IsFixedTop).ThenByDescending(p => p.ModifyDate).Select(p => new
            {
                p.Id,
                p.Author,
                CategoryName = p.Category.Name,
                p.Email,
                p.IsFixedTop,
                p.Label,
                md = p.ModifyDate,
                pd = p.PostDate,
                p.Title,
                ViewCount = p.TotalViewCount,
                p.VoteDownCount,
                p.VoteUpCount,
                stat = p.Status
            }).ToList();
            var list = new List<PostDataModel>();
            plist.ForEach(item =>
            {
                PostDataModel model = item.MapTo<PostDataModel>();
                model.PostDate = item.pd.ToString("yyyy-MM-dd HH:mm");
                model.ModifyDate = item.md.ToString("yyyy-MM-dd HH:mm");
                model.Status = item.stat.GetDisplay();
                list.Add(model);
            });
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="post"></param>
        /// <param name="notify"></param>
        /// <param name="reserve"></param>
        /// <returns></returns>
        [HttpPost, Authority]
        public ActionResult Edit(PostInputDto post, bool notify = true, bool reserve = true)
        {
            post.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(post.Content.Trim(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>"));
            if (!CategoryService.Any(c => c.Id == post.CategoryId && c.Status == Status.Available))
            {
                return ResultData(null, message: "请选择一个分类");
            }

            if (string.IsNullOrEmpty(post.Label?.Trim()) || post.Label.Equals("null"))
            {
                post.Label = null;
            }
            else if (post.Label.Trim().Length > 50)
            {
                post.Label = post.Label.Replace("，", ",");
                post.Label = post.Label.Trim().Substring(0, 50);
            }
            else
            {
                post.Label = post.Label.Replace("，", ",");
            }

            if (!post.IsWordDocument)
            {
                post.ResourceName = null;
            }

            if (string.IsNullOrEmpty(post.ProtectContent) || post.ProtectContent.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                post.ProtectContent = null;
            }

            Post p = PostService.GetById(post.Id);
            if (reserve)
            {
                post.ModifyDate = DateTime.Now;
                var history = p.Mapper<PostHistoryVersion>();
                history.Id = 0;
                p.PostHistoryVersion.Add(history);
            }

            p.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            Mapper.Map(post, p);
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct();
                p.Seminar.Clear();
                tmp.ForEach(s =>
                {
                    Seminar seminar = SeminarService.GetFirstEntity(e => e.Title.Equals(s));
                    p.Seminar.Add(new SeminarPost()
                    {
                        Post = p,
                        Seminar = seminar,
                        PostId = p.Id,
                        SeminarId = seminar.Id
                    });
                });
            }

            bool b = PostService.UpdateEntitySaved(p);
            if (b)
            {
#if !DEBUG
                if (notify && "false" == CommonHelper.SystemSettings["DisabledEmailBroadcast"])
                {
                    var cast = BroadcastService.LoadEntities(c => c.Status == Status.Subscribed).ToList();
                    string link = Request.Scheme + "://" + Request.Host + "/" + p.Id;
                    cast.ForEach(c =>
                    {
                        var ts = DateTime.Now.GetTotalMilliseconds();
                        string content = System.IO.File.ReadAllText(Path.Combine(_hostingEnvironment.WebRootPath, "template", "broadcast.html"))
                            .Replace("{{link}}", link + "?email=" + c.Email)
                            .Replace("{{time}}", post.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))
                            .Replace("{{title}}", post.Title)
                            .Replace("{{author}}", post.Author)
                            .Replace("{{content}}", post.Content.RemoveHtmlTag(150))
                            .Replace("{{cancel}}", Url.Action("Subscribe", "Subscribe", new
                            {
                                c.Email,
                                act = "cancel",
                                validate = c.ValidateCode,
                                timespan = ts,
                                hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(AppConfig.BaiduAK)
                            }, Request.Scheme));
                        BackgroundJob.Schedule(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客有新文章发布了", content, c.Email), (p.ModifyDate - DateTime.Now));
                    });
                }
#endif
                return ResultData(p.Mapper<PostOutputDto>(), message: "文章修改成功！");
            }

            return ResultData(null, false, "文章修改失败！");
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="post"></param>
        /// <param name="timespan"></param>
        /// <param name="schedule"></param>
        /// <returns></returns>
        [Authority, HttpPost]
        public ActionResult Write(PostInputDto post, DateTime? timespan, bool schedule = false)
        {
            post.Content = CommonHelper.ReplaceImgSrc(Regex.Replace(post.Content.Trim(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/"); //提取img标签，提取src属性并重新创建个只包含src属性的img标签
            if (!CategoryService.Any(c => c.Id == post.CategoryId && c.Status == Status.Available))
            {
                return ResultData(null, message: "请选择一个分类");
            }

            if (string.IsNullOrEmpty(post.Label?.Trim()) || post.Label.Equals("null"))
            {
                post.Label = null;
            }
            else if (post.Label.Trim().Length > 50)
            {
                post.Label = post.Label.Replace("，", ",");
                post.Label = post.Label.Trim().Substring(0, 50);
            }
            else
            {
                post.Label = post.Label.Replace("，", ",");
            }

            if (!post.IsWordDocument)
            {
                post.ResourceName = null;
            }

            if (string.IsNullOrEmpty(post.ProtectContent) || post.ProtectContent.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                post.ProtectContent = null;
            }

            post.Status = Status.Pended;
            post.PostDate = DateTime.Now;
            post.ModifyDate = DateTime.Now;
            Post p = post.Mapper<Post>();
            p.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct();
                tmp.ForEach(s =>
                {
                    var id = s.ToInt32();
                    Seminar seminar = SeminarService.GetById(id);
                    p.Seminar.Add(new SeminarPost()
                    {
                        Post = p,
                        PostId = p.Id,
                        Seminar = seminar,
                        SeminarId = seminar.Id
                    });
                });
            }

            p.PostAccessRecord.Add(new PostAccessRecord()
            {
                AccessTime = DateTime.Today,
                ClickCount = 0
            });
            if (schedule)
            {
                if (timespan.HasValue && timespan.Value > DateTime.Now)
                {
                    p.Status = Status.Schedule;
                    p.PostDate = timespan.Value;
                    p.ModifyDate = timespan.Value;
                    HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.PublishPost), args: p);
                    return ResultData(p.Mapper<PostOutputDto>(), message: schedule ? $"文章于{timespan.Value:yyyy-MM-dd HH:mm:ss}将会自动发表！" : "文章发表成功！");
                }

                return ResultData(null, false, "如果要定时发布，请选择正确的一个将来时间点！");
            }

            bool b = PostService.AddEntitySaved(p) != null;
            if (b)
            {
                if ("false" == CommonHelper.SystemSettings["DisabledEmailBroadcast"])
                {
                    var cast = BroadcastService.LoadEntities(c => c.Status == Status.Subscribed).ToList();
                    string link = Request.Scheme + "://" + Request.Host + "/" + p.Id;
                    cast.ForEach(c =>
                    {
                        var ts = DateTime.Now.GetTotalMilliseconds();
                        string content = System.IO.File.ReadAllText(_hostingEnvironment.WebRootPath + "/template/broadcast.html")
                            .Replace("{{link}}", link + "?email=" + c.Email)
                            .Replace("{{time}}", post.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))
                            .Replace("{{title}}", post.Title).Replace("{{author}}", post.Author)
                            .Replace("{{content}}", post.Content.RemoveHtmlTag(150))
                            .Replace("{{cancel}}", Url.Action("Subscribe", "Subscribe", new
                            {
                                c.Email,
                                act = "cancel",
                                validate = c.ValidateCode,
                                timespan = ts,
                                hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(AppConfig.BaiduAK)
                            }, Request.Scheme));
                        BackgroundJob.Schedule(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客有新文章发布了", content, c.Email), (p.ModifyDate - DateTime.Now));
                    });
                }

                return ResultData(null, true, "文章发表成功！");
            }

            return ResultData(null, false, "文章发表失败！");
        }

        /// <summary>
        /// 添加专题
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult AddSeminar(int id, int sid)
        {
            var post = PostService.GetById(id);
            Seminar seminar = SeminarService.GetById(sid);
            post.Seminar.Add(new SeminarPost()
            {
                Post = post,
                Seminar = seminar,
                SeminarId = seminar.Id,
                PostId = post.Id
            });
            bool b = PostService.UpdateEntitySaved(post);
            return ResultData(null, b, b ? $"已将文章【{post.Title}】添加到专题【{seminar.Title}】" : "添加失败");
        }

        /// <summary>
        /// 移除专题
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult RemoveSeminar(int id, int sid)
        {
            var post = PostService.GetById(id);
            Seminar seminar = SeminarService.GetById(sid);
            post.Seminar.Remove(new SeminarPost()
            {
                Post = post,
                Seminar = seminar,
                SeminarId = seminar.Id,
                PostId = post.Id
            });
            bool b = PostService.UpdateEntitySaved(post);
            return ResultData(null, b, b ? $"已将文章【{post.Title}】从【{seminar.Title}】专题移除" : "添加失败");
        }

        /// <summary>
        /// 删除历史版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult DeleteHistory(int id)
        {
            bool b = PostHistoryVersionService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "历史版本文章删除成功！" : "历史版本文章删除失败！");
        }

        /// <summary>
        /// 获取文章访问密码
        /// </summary>
        /// <returns></returns>
        [Authority, HttpPost]
        public ActionResult ViewToken()
        {
            if (!RedisHelper.Exists("ArticleViewToken"))
            {
                RedisHelper.Set("ArticleViewToken", SnowFlake.GetInstance().GetUniqueShortId(6));
            }

            var token = RedisHelper.Get("ArticleViewToken");
            return ResultData(token);
        }

        /// <summary>
        /// 还原版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Revert(int id)
        {
            var history = PostHistoryVersionService.GetById(id);
            if (history != null)
            {
                //var version = history.Post.Mapper<PostHistoryVersion>();
                //version.Id = 0;
                //PostHistoryVersionService.AddEntity(version);
                history.Post.Category = history.Category;
                history.Post.CategoryId = history.CategoryId;
                history.Post.Content = history.Content;
                history.Post.Title = history.Title;
                history.Post.Label = history.Label;
                history.Post.Seminar.Clear();
                foreach (var s in history.Seminar)
                {
                    history.Post.Seminar.Add(new SeminarPost()
                    {
                        Post = history.Post,
                        PostId = history.PostId,
                        Seminar = s.Seminar,
                        SeminarId = s.SeminarId
                    });
                }
                history.Post.ModifyDate = history.ModifyDate;
                bool b = PostHistoryVersionService.UpdateEntitySaved(history);
                PostHistoryVersionService.DeleteByIdSaved(id);
                return ResultData(null, b, b ? "回滚成功" : "回滚失败");
            }

            return ResultData(null, false, "版本不存在");
        }

        /// <summary>
        /// 文章分析
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Analyse(int id)
        {
            var list = PostService.GetById(id).PostAccessRecord.OrderBy(r => r.AccessTime).GroupBy(r => r.AccessTime.Date).Select(r => new[]
            {
                r.Key.GetTotalMilliseconds(),
                r.Sum(p => p.ClickCount)
            }).ToList();
            var high = list.OrderByDescending(n => n[1]).FirstOrDefault();
            var average = list.Average(d => d[1]);
            return ResultData(new
            {
                list,
                aver = average,
                high = high[1],
                highDate = DateTime.Parse("1970-01-01").AddMilliseconds(high[0])
            });
        }

        #endregion
    }
}