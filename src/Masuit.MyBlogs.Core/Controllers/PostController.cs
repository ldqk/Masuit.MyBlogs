using AutoMapper.QueryableExtensions;
using EFSecondLevelCache.Core;
using Hangfire;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.LuceneEFCore.SearchEngine.Linq;
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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Net.Http.Headers;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 文章管理
    /// </summary>
    public class PostController : BaseController
    {
        public IPostService PostService { get; set; }
        public ICategoryService CategoryService { get; set; }
        public IBroadcastService BroadcastService { get; set; }
        public ISeminarService SeminarService { get; set; }
        public IPostHistoryVersionService PostHistoryVersionService { get; set; }

        public IInternalMessageService MessageService { get; set; }

        public IHostingEnvironment HostingEnvironment { get; set; }
        public ISearchEngine<DataContext> SearchEngine { get; set; }
        public ImagebedClient ImagebedClient { get; set; }

        /// <summary>
        /// 文章详情页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="kw"></param>
        /// <returns></returns>
        [Route("{id:int}/{kw}"), Route("{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Details(int id, string kw)
        {
            var post = PostService.Get(p => p.Id == id && (p.Status == Status.Pended || CurrentUser.IsAdmin)) ?? throw new NotFoundException("文章未找到");
            ViewBag.Keyword = post.Keyword + "," + post.Label;
            var modifyDate = post.ModifyDate;
            ViewBag.Next = PostService.GetFromCache<DateTime, PostModelBase>(p => p.ModifyDate > modifyDate && (p.Status == Status.Pended || CurrentUser.IsAdmin), p => p.ModifyDate);
            ViewBag.Prev = PostService.GetFromCache<DateTime, PostModelBase>(p => p.ModifyDate < modifyDate && (p.Status == Status.Pended || CurrentUser.IsAdmin), p => p.ModifyDate, false);
            if (!string.IsNullOrEmpty(kw))
            {
                ViewData["keywords"] = post.Content.Contains(kw) ? $"['{kw}']" : SearchEngine.LuceneIndexSearcher.CutKeywords(kw).ToJsonString();
            }

            if (CurrentUser.IsAdmin)
            {
                return View("Details_Admin", post);
            }

            if (!HttpContext.Request.IsRobot() && string.IsNullOrEmpty(HttpContext.Session.Get<string>("post" + id)))
            {
                HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.RecordPostVisit), args: id);
                HttpContext.Session.Set("post" + id, id.ToString());
            }

            return View(post);
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
            var post = PostService.Get(p => p.Id == id && (p.Status == Status.Pended || CurrentUser.IsAdmin)).Mapper<PostOutputDto>() ?? throw new NotFoundException("文章未找到");
            ViewBag.Primary = post;
            var list = PostHistoryVersionService.GetPages(page, size, out int total, v => v.PostId == id, v => v.ModifyDate, false).ToList();
            ViewBag.Total = total;
            ViewBag.PageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return View(list);
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
            var post = PostHistoryVersionService.Get(v => v.Id == hid) ?? throw new NotFoundException("文章未找到");
            ViewBag.Next = PostHistoryVersionService.Get(p => p.PostId == id && p.ModifyDate > post.ModifyDate, p => p.ModifyDate);
            ViewBag.Prev = PostHistoryVersionService.Get(p => p.PostId == id && p.ModifyDate < post.ModifyDate, p => p.ModifyDate, false);
            return CurrentUser.IsAdmin ? View("HistoryVersion_Admin", post) : View(post);
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
            var main = PostService.Get(p => p.Id == id && (p.Status == Status.Pended || CurrentUser.IsAdmin)).Mapper<PostHistoryVersion>() ?? throw new NotFoundException("文章未找到");
            var left = v1 <= 0 ? main : PostHistoryVersionService.Get(v => v.Id == v1) ?? throw new NotFoundException("文章未找到");
            var right = v2 <= 0 ? main : PostHistoryVersionService.Get(v => v.Id == v2) ?? throw new NotFoundException("文章未找到");
            main.Id = id;
            var diff = new HtmlDiff.HtmlDiff(right.Content, left.Content);
            var diffOutput = diff.Build();
            right.Content = Regex.Replace(Regex.Replace(diffOutput, "<ins.+?</ins>", string.Empty), @"<\w+></\w+>", string.Empty);
            left.Content = Regex.Replace(Regex.Replace(diffOutput, "<del.+?</del>", string.Empty), @"<\w+></\w+>", string.Empty);
            return View(new[] { main, left, right });
        }

        /// <summary>
        /// 反对
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult VoteDown(int id)
        {
            if (HttpContext.Session.Get("post-vote" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            Post post = PostService.GetById(id);
            if (post == null)
            {
                return ResultData(null, false, "非法操作");
            }

            HttpContext.Session.Set("post-vote" + id, id.GetBytes());
            post.VoteDownCount += 1;
            var b = PostService.SaveChanges() > 0;
            return ResultData(null, b, b ? "投票成功！" : "投票失败！");

        }

        /// <summary>
        /// 支持
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult VoteUp(int id)
        {
            if (HttpContext.Session.Get("post-vote" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            Post post = PostService.GetById(id);
            if (post == null)
            {
                return ResultData(null, false, "非法操作");
            }

            HttpContext.Session.Set("post-vote" + id, id.GetBytes());
            post.VoteUpCount += 1;
            var b = PostService.SaveChanges() > 0;
            return ResultData(null, b, b ? "投票成功！" : "投票失败！");

        }

        /// <summary>
        /// 投稿页
        /// </summary>
        /// <returns></returns>
        public ActionResult Publish()
        {
            var list = PostService.GetQuery(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).Distinct().SelectMany(s => s.Split(',', '，')).OrderBy(s => s).Cacheable().ToHashSet();
            ViewBag.Category = CategoryService.GetQueryFromCache(c => c.Status == Status.Available).ToList();
            return View(list);
        }

        /// <summary>
        /// 发布投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Publish(PostInputDto post, string code)
        {
            if (RedisHelper.Get("code:" + post.Email) != code)
            {
                return ResultData(null, false, "验证码错误！");
            }

            if (Regex.Match(post.Content, CommonHelper.BanRegex).Length > 0)
            {
                return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请注意改善您的言辞！");
            }

            if (!CategoryService.Any(c => c.Id == post.CategoryId))
            {
                return ResultData(null, message: "请选择一个分类");
            }

            post.Label = string.IsNullOrEmpty(post.Label?.Trim()) ? null : post.Label.Replace("，", ",");
            post.Status = Status.Pending;
            post.PostDate = DateTime.Now;
            post.ModifyDate = DateTime.Now;
            post.Content = await ImagebedClient.ReplaceImgSrc(post.Content.HtmlSantinizerStandard().ClearImgAttributes());
            ViewBag.CategoryId = new SelectList(CategoryService.GetQueryNoTracking(c => c.Status == Status.Available), "Id", "Name", post.CategoryId);
            Post p = post.Mapper<Post>();
            p.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            p.Modifier = p.Author;
            p.ModifierEmail = p.Email;
            p = PostService.AddEntitySaved(p);
            if (p == null)
            {
                return ResultData(null, false, "文章发表失败！");
            }

            RedisHelper.Expire("code:" + p.Email, 1);
            var content = System.IO.File.ReadAllText(HostingEnvironment.WebRootPath + "/template/publish.html")
                .Replace("{{link}}", Url.Action("Details", "Post", new { id = p.Id }, Request.Scheme))
                .Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{{title}}", p.Title);
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "有访客投稿：", content, CommonHelper.SystemSettings["ReceiveEmail"]));
            return ResultData(p.Mapper<PostOutputDto>(), message: "文章发表成功，待站长审核通过以后将显示到列表中！");
        }

        /// <summary>
        /// 获取标签
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult GetTag()
        {
            var list = PostService.GetQuery(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).Distinct().SelectMany(s => s.Split(',', '，')).OrderBy(s => s).Cacheable().ToHashSet();
            return ResultData(list);
        }

        /// <summary>
        /// 标签云
        /// </summary>
        /// <returns></returns>
        [Route("all"), ResponseCache(Duration = 600, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult All()
        {
            var tags = PostService.GetQuery(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).SelectMany(s => s.Split(',', '，')).OrderBy(s => s).Cacheable().ToList(); //tag
            ViewBag.tags = tags.GroupBy(t => t).OrderByDescending(g => g.Count()).ThenBy(g => g.Key);
            ViewBag.cats = CategoryService.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Count = c.Post.Count(p => p.Status == Status.Pended || CurrentUser.IsAdmin)
            }).Cacheable().ToList(); //category
            ViewBag.seminars = SeminarService.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel
            {
                Id = c.Id,
                Name = c.Title,
                Count = c.Post.Count(p => p.Post.Status == Status.Pended || CurrentUser.IsAdmin)
            }).Cacheable().ToList(); //seminars
            return View();
        }

        /// <summary>
        /// 检查访问密码
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall, ResponseCache(Duration = 115, VaryByQueryKeys = new[] { "email", "token" })]
        public ActionResult CheckViewToken(string email, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return ResultData(null, false, "请输入访问密码！");
            }

            var s = RedisHelper.Get("token:" + email);
            if (token.Equals(s))
            {
                HttpContext.Session.Set("AccessViewToken", token);
                Response.Cookies.Append("Email", email, new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1)
                });
                Response.Cookies.Append("PostAccessToken", email.MDString3(AppConfig.BaiduAK), new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1)
                });
                return ResultData(null);
            }

            return ResultData(null, false, "访问密码不正确！");
        }

        /// <summary>
        /// 检查授权邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall, ResponseCache(Duration = 115, VaryByQueryKeys = new[] { "email" })]
        public ActionResult GetViewToken(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.MatchEmail())
            {
                return ResultData(null, false, "请输入正确的邮箱！");
            }

            if (RedisHelper.Exists("get:" + email))
            {
                RedisHelper.Expire("get:" + email, 120);
                return ResultData(null, false, "发送频率限制，请在2分钟后重新尝试发送邮件！请检查你的邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！");
            }

            if (!BroadcastService.Any(b => b.Email.Equals(email) && b.SubscribeType == SubscribeType.ArticleToken))
            {
                return ResultData(null, false, "您目前没有权限访问这个链接，请联系站长开通访问权限！");
            }

            var token = SnowFlake.GetInstance().GetUniqueShortId(6);
            RedisHelper.Set("token:" + email, token, 86400);
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Domain"] + "博客访问验证码", $"{CommonHelper.SystemSettings["Domain"]}本次验证码是：<span style='color:red'>{token}</span>，有效期为24h，请按时使用！", email));
            RedisHelper.Set("get:" + email, token, 120);
            return ResultData(null);

        }

        /// <summary>
        /// 文章合并
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/merge")]
        public ActionResult PushMerge(int id)
        {
            var post = PostService.GetById(id) ?? throw new NotFoundException("文章未找到");
            return View(post);
        }

        /// <summary>
        /// 文章合并
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mid"></param>
        /// <returns></returns>
        [HttpGet("{id}/merge/{mid}")]
        public ActionResult RepushMerge(int id, int mid)
        {
            var post = PostService.GetById(id) ?? throw new NotFoundException("文章未找到");
            var merge = post.PostMergeRequests.FirstOrDefault(p => p.Id == mid && p.MergeState != MergeStatus.Merged) ?? throw new NotFoundException("待合并文章未找到");
            return View(merge);
        }

        /// <summary>
        /// 文章合并
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("{id}/pushmerge")]
        public ActionResult PushMerge(PostMergeRequestInputDto dto)
        {
            if (RedisHelper.Get("code:" + dto.ModifierEmail) != dto.Code)
            {
                return ResultData(null, false, "验证码错误！");
            }

            var post = PostService.GetById(dto.PostId) ?? throw new NotFoundException("文章未找到");
            if (post.Title.Equals(dto.Title) && post.Content.Equals(dto.Content))
            {
                return ResultData(null, false, "内容未被修改！");
            }

            #region 直接合并

            if (post.Email.Equals(dto.ModifierEmail))
            {
                var history = post.Mapper<PostHistoryVersion>();
                Mapper.Map(dto, post);
                post.PostHistoryVersion.Add(history);
                post.ModifyDate = DateTime.Now;
                return PostService.SaveChanges() > 0 ? ResultData(null, true, "你是文章原作者，无需审核，文章已自动更新并在首页展示！") : ResultData(null, false, "操作失败！");
            }

            #endregion

            var merge = post.PostMergeRequests.FirstOrDefault(r => r.Id == dto.Id && r.MergeState != MergeStatus.Merged);
            if (merge != null)
            {
                Mapper.Map(dto, merge);
                merge.SubmitTime = DateTime.Now;
                merge.MergeState = MergeStatus.Pending;
            }
            else
            {
                merge = Mapper.Map<PostMergeRequest>(dto);
                post.PostMergeRequests.Add(merge);
            }

            var b = PostService.SaveChanges() > 0;
            if (!b)
            {
                return ResultData(null, b, b ? "您的修改请求已提交，已进入审核状态，感谢您的参与！" : "操作失败！");
            }

            RedisHelper.Expire("code:" + dto.ModifierEmail, 1);
            MessageService.AddEntitySaved(new InternalMessage()
            {
                Title = $"来自【{dto.Modifier}】的文章修改合并请求",
                Content = dto.Title,
                Link = "#/merge/compare?id=" + merge.Id
            });
            var content = System.IO.File.ReadAllText(HostingEnvironment.WebRootPath + "/template/merge-request.html").Replace("{{title}}", post.Title).Replace("{{link}}", Url.Action("Index", "Dashboard", new { }, Request.Scheme) + "#/merge/compare?id=" + merge.Id);
            BackgroundJob.Enqueue(() => CommonHelper.SendMail("博客文章修改请求：", content, CommonHelper.SystemSettings["ReceiveEmail"]));

            return ResultData(null, b, b ? "您的修改请求已提交，已进入审核状态，感谢您的参与！" : "操作失败！");
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
            bool b = PostService.SaveChanges() > 0;
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
            bool b = PostService.SaveChanges() > 0;
            if (!b)
            {
                return ResultData(null, false, "审核失败！");
            }

            if ("true" == CommonHelper.SystemSettings["DisabledEmailBroadcast"])
            {
                return ResultData(null, true, "审核通过！");
            }

            var cast = BroadcastService.GetQuery(c => c.Status == Status.Subscribed).ToList();
            var link = Request.Scheme + "://" + Request.Host + "/" + id;
            cast.ForEach(c =>
            {
                var ts = DateTime.Now.GetTotalMilliseconds();
                var content = System.IO.File.ReadAllText(HostingEnvironment.WebRootPath + "/template/broadcast.html")
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
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客有新文章发布了", content, c.Email));
            });

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
            bool b = SearchEngine.SaveChanges() > 0;
            SearchEngine.LuceneIndexer.Delete(post);
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
            bool b = PostService.SaveChanges() > 0;
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
                    System.IO.File.Delete(Path.Combine(HostingEnvironment.WebRootPath + "/upload", post.ResourceName));
                    Directory.Delete(Path.Combine(HostingEnvironment.WebRootPath + "/upload", Path.GetFileNameWithoutExtension(post.ResourceName)), true);
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
                        System.IO.File.Delete(HostingEnvironment.WebRootPath + path);
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
            model.Seminars = post.Seminar.Select(s => s.Seminar.Title).Join(",");
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
        /// 获取文章分页
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页数必须大于0")]int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")]int size = 10, OrderBy orderby = OrderBy.ModifyDate, string kw = "")
        {
            IOrderedQueryable<Post> temp;
            var query = string.IsNullOrEmpty(kw) ? PostService.GetAll() : PostService.GetQuery(p => p.Title.Contains(kw) || p.Author.Contains(kw) || p.Email.Contains(kw) || p.Label.Contains(kw) || p.Content.Contains(kw));
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

            var list = temp.Skip((page - 1) * size).Take(size).ProjectTo<PostDataModel>(MapperConfig).ToList();
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
            Expression<Func<Post, bool>> where = p => p.Status == Status.Pending;
            if (!string.IsNullOrEmpty(search))
            {
                where = where.And(p => p.Title.Contains(search) || p.Author.Contains(search) || p.Email.Contains(search) || p.Label.Contains(search));
            }

            var temp = PostService.GetPagesNoTracking(page, size, out var total, where, p => p.Id);
            var list = temp.OrderByDescending(p => p.IsFixedTop).ThenByDescending(p => p.ModifyDate).ProjectTo<PostDataModel>(MapperConfig).ToList();
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
        public async Task<ActionResult> Edit(PostInputDto post, bool notify = true, bool reserve = true)
        {
            post.Content = await ImagebedClient.ReplaceImgSrc(post.Content.Trim().ClearImgAttributes());
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
                var history = p.Mapper<PostHistoryVersion>();
                p.PostHistoryVersion.Add(history);
                post.ModifyDate = DateTime.Now;
                var user = HttpContext.Session.Get<UserInfoOutputDto>(SessionKey.UserInfo);
                p.Modifier = user.NickName;
                p.ModifierEmail = user.Email;
            }

            p.IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            Mapper.Map(post, p);
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct();
                p.Seminar.Clear();
                tmp.ForEach(s =>
                {
                    var seminar = SeminarService.Get(e => e.Title.Equals(s));
                    if (seminar != null)
                    {
                        p.Seminar.Add(new SeminarPost()
                        {
                            Post = p,
                            Seminar = seminar,
                            PostId = p.Id,
                            SeminarId = seminar.Id
                        });
                    }
                });
            }

            bool b = PostService.SaveChanges() > 0;
            if (!b)
            {
                return ResultData(null, false, "文章修改失败！");
            }

#if !DEBUG
            if (notify && "false" == CommonHelper.SystemSettings["DisabledEmailBroadcast"])
            {
                var cast = BroadcastService.GetQuery(c => c.Status == Status.Subscribed).ToList();
                string link = Request.Scheme + "://" + Request.Host + "/" + p.Id;
                cast.ForEach(c =>
                {
                    var ts = DateTime.Now.GetTotalMilliseconds();
                    string content = System.IO.File.ReadAllText(Path.Combine(HostingEnvironment.WebRootPath, "template", "broadcast.html"))
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

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="post"></param>
        /// <param name="timespan"></param>
        /// <param name="schedule"></param>
        /// <returns></returns>
        [Authority, HttpPost]
        public async Task<ActionResult> Write(PostInputDto post, DateTime? timespan, bool schedule = false)
        {
            post.Content = await ImagebedClient.ReplaceImgSrc(post.Content.Trim().ClearImgAttributes());
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
            p.Modifier = p.Author;
            p.ModifierEmail = p.Email;
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

            if (schedule)
            {
                if (timespan.HasValue && timespan.Value > DateTime.Now)
                {
                    p.Status = Status.Schedule;
                    p.PostDate = timespan.Value;
                    p.ModifyDate = timespan.Value;
                    HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.PublishPost), args: p);
                    return ResultData(p.Mapper<PostOutputDto>(), message: $"文章于{timespan.Value:yyyy-MM-dd HH:mm:ss}将会自动发表！");
                }

                return ResultData(null, false, "如果要定时发布，请选择正确的一个将来时间点！");
            }

            bool b = PostService.AddEntitySaved(p) != null;
            if (!b)
            {
                return ResultData(null, false, "文章发表失败！");
            }

            if ("true" == CommonHelper.SystemSettings["DisabledEmailBroadcast"])
            {
                return ResultData(null, true, "文章发表成功！");
            }
            var cast = BroadcastService.GetQuery(c => c.Status == Status.Subscribed).ToList();
            string link = Request.Scheme + "://" + Request.Host + "/" + p.Id;
            cast.ForEach(c =>
            {
                var ts = DateTime.Now.GetTotalMilliseconds();
                string content = System.IO.File.ReadAllText(HostingEnvironment.WebRootPath + "/template/broadcast.html")
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
            return ResultData(null, true, "文章发表成功！");
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
            bool b = PostService.SaveChanges() > 0;
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
            bool b = PostService.SaveChanges() > 0;
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
        /// 还原版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Revert(int id)
        {
            var history = PostHistoryVersionService.GetById(id);
            if (history == null)
            {
                return ResultData(null, false, "版本不存在");
            }

            history.Post.Category = history.Category;
            history.Post.CategoryId = history.CategoryId;
            history.Post.Content = history.Content;
            history.Post.Title = history.Title;
            history.Post.Label = history.Label;
            history.Post.ModifyDate = history.ModifyDate;
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
            bool b = PostHistoryVersionService.SaveChanges() > 0;
            PostHistoryVersionService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "回滚成功" : "回滚失败");

        }

        /// <summary>
        /// 禁用或开启文章评论
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        [Authority]
        public ActionResult DisableComment(int id)
        {
            var post = PostService.GetById(id);
            if (post != null)
            {
                post.DisableComment = !post.DisableComment;
                return ResultData(null, PostService.SaveChanges() > 0, post.DisableComment ? $"已禁用【{post.Title}】这篇文章的评论功能！" : $"已启用【{post.Title}】这篇文章的评论功能！");
            }

            return ResultData(null, false, "文章不存在");
        }

        #endregion
    }
}