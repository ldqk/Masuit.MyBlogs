using CacheManager.Core;
using Collections.Pooled;
using Hangfire;
using JiebaNet.Segmenter;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Configs;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.Firewall;
using Masuit.MyBlogs.Core.Extensions.Hangfire;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Infrastructure.Repository;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Command;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.MyBlogs.Core.Views.Post;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.AspNetCore.ResumeFileResults.Extensions;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Core.Validator;
using Masuit.Tools.Database;
using Masuit.Tools.Excel;
using Masuit.Tools.Html;
using Masuit.Tools.Linq;
using Masuit.Tools.Logging;
using Masuit.Tools.Models;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Z.EntityFramework.Plus;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 文章管理
    /// </summary>
    public class PostController : BaseController
    {
        public IPostService PostService { get; set; }

        public ICategoryService CategoryService { get; set; }

        public ISeminarService SeminarService { get; set; }

        public IPostHistoryVersionService PostHistoryVersionService { get; set; }

        public IWebHostEnvironment HostEnvironment { get; set; }

        public ISearchEngine<DataContext> SearchEngine { get; set; }

        public ImagebedClient ImagebedClient { get; set; }

        public IPostVisitRecordService PostVisitRecordService { get; set; }

        public ICommentService CommentService { get; set; }

        public IPostTagService PostTagService { get; set; }

        /// <summary>
        /// 文章详情页
        /// </summary>
        /// <returns></returns>
        [Route("{id:int}"), Route("{id:int}/comments/{cid:int}"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Details(int id, string kw, int cid, string t)
        {
            var notRobot = !Request.IsRobot();
            if (string.IsNullOrEmpty(t) && notRobot)
            {
                return RedirectToAction("Details", cid > 0 ? new { id, kw, cid, t = SnowFlake.NewId } : new { id, kw, t = SnowFlake.NewId });
            }

            var post = await PostService.GetQuery(p => p.Id == id && (p.Status == Status.Published || CurrentUser.IsAdmin)).Include(p => p.Seminar).AsNoTracking().FirstOrDefaultAsync() ?? throw new NotFoundException("文章未找到");
            CheckPermission(post);
            if (!string.IsNullOrEmpty(post.Redirect))
            {
                if (notRobot && string.IsNullOrEmpty(HttpContext.Session.Get<string>("post" + id)))
                {
                    BackgroundJob.Enqueue<IHangfireBackJob>(job => job.RecordPostVisit(id, ClientIP, Request.Headers[HeaderNames.Referer].ToString(), Request.GetDisplayUrl()));
                    HttpContext.Session.Set("post" + id, id.ToString());
                }

                return Redirect(post.Redirect);
            }

            post.Category = CategoryService[post.CategoryId];
            ViewBag.CommentsCount = CommentService.Count(c => c.PostId == id && c.ParentId == null && c.Status == Status.Published);
            ViewBag.HistoryCount = PostHistoryVersionService.Count(c => c.PostId == id);
            ViewBag.Keyword = post.Keyword + "," + post.Label;
            ViewBag.Desc = "若页面无法访问，可通过搜索引擎网页快照进行浏览。" + await post.Content.GetSummary(200);
            var modifyDate = post.ModifyDate;
            ViewBag.Next = await PostService.GetFromCacheAsync<DateTime, PostModelBase>(p => p.ModifyDate > modifyDate && (p.LimitMode ?? 0) == RegionLimitMode.All && (p.Status == Status.Published || CurrentUser.IsAdmin), p => p.ModifyDate);
            ViewBag.Prev = await PostService.GetFromCacheAsync<DateTime, PostModelBase>(p => p.ModifyDate < modifyDate && (p.LimitMode ?? 0) == RegionLimitMode.All && (p.Status == Status.Published || CurrentUser.IsAdmin), p => p.ModifyDate, false);
            ViewData[nameof(post.Author)] = post.Author;
            ViewData[nameof(post.PostDate)] = post.PostDate;
            ViewData[nameof(post.ModifyDate)] = post.ModifyDate;
            ViewData["cover"] = post.Content.MatchFirstImgSrc();
            if (!string.IsNullOrEmpty(kw))
            {
                await PostService.Highlight(post, kw);
            }

            var regex = SearchEngine.LuceneIndexSearcher.CutKeywords(string.IsNullOrWhiteSpace(post.Keyword + post.Label) ? post.Title : post.Keyword + post.Label).Select(Regex.Escape).Join("|");
            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, Request.Location(), post.CategoryId, regex);
            var related = PostService.GetQuery(PostBaseWhere().And(p => p.Id != id && Regex.IsMatch(p.Title + (p.Keyword ?? "") + (p.Label ?? ""), regex, RegexOptions.IgnoreCase)), p => p.AverageViewCount, false).Take(10).Select(p => new { p.Id, p.Title }).FromCache().ToDictionary(p => p.Id, p => p.Title);
            ViewBag.Related = related;
            post.ModifyDate = post.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            post.PostDate = post.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            post.Content = ReplaceVariables(post.Content);
            post.ProtectContent = ReplaceVariables(post.ProtectContent);

            if (CurrentUser.IsAdmin)
            {
                return View("Details_Admin", post);
            }

            if (notRobot && string.IsNullOrEmpty(HttpContext.Session.Get<string>("post" + id)))
            {
                BackgroundJob.Enqueue<IHangfireBackJob>(job => job.RecordPostVisit(id, ClientIP, Request.Headers[HeaderNames.Referer].ToString(), Request.GetDisplayUrl()));
                HttpContext.Session.Set("post" + id, id.ToString());
            }

            if (post.LimitMode == RegionLimitMode.OnlyForSearchEngine)
            {
                BackgroundJob.Enqueue<IHangfireBackJob>(job => job.RecordPostVisit(id, ClientIP, Request.Headers[HeaderNames.Referer].ToString(), Request.GetDisplayUrl()));
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
        [Route("{id:int}/history"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "page", "size" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> History(int id, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 20)
        {
            var post = await PostService.GetAsync(p => p.Id == id && (p.Status == Status.Published || CurrentUser.IsAdmin)) ?? throw new NotFoundException("文章未找到");
            CheckPermission(post);
            ViewBag.Primary = post;
            var list = await PostHistoryVersionService.GetPagesAsync(page, size, v => v.PostId == id, v => v.ModifyDate, false);
            foreach (var item in list.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, Request.Location(), post.CategoryId, post.Keyword + "," + post.Label);
            return View(list);
        }

        /// <summary>
        /// 文章历史版本
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hid"></param>
        /// <returns></returns>
        [Route("{id:int}/history/{hid:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "hid" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> HistoryVersion(int id, int hid)
        {
            var history = await PostHistoryVersionService.GetAsync(v => v.Id == hid && (v.Post.Status == Status.Published || CurrentUser.IsAdmin)) ?? throw new NotFoundException("文章未找到");
            CheckPermission(history.Post);
            history.Content = ReplaceVariables(history.Content);
            history.ProtectContent = ReplaceVariables(history.ProtectContent);
            history.ModifyDate = history.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            var next = await PostHistoryVersionService.GetAsync(p => p.PostId == id && p.ModifyDate > history.ModifyDate, p => p.ModifyDate);
            var prev = await PostHistoryVersionService.GetAsync(p => p.PostId == id && p.ModifyDate < history.ModifyDate, p => p.ModifyDate, false);
            ViewBag.Next = next;
            ViewBag.Prev = prev;
            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, Request.Location(), history.CategoryId, history.Label);
            ViewData[nameof(history.Post.Author)] = history.Post.Author;
            ViewData[nameof(history.Post.PostDate)] = history.Post.PostDate;
            ViewData[nameof(history.ModifyDate)] = history.ModifyDate;
            ViewData["cover"] = history.Content.MatchFirstImgSrc();
            return CurrentUser.IsAdmin ? View("HistoryVersion_Admin", history) : View(history);
        }

        /// <summary>
        /// 版本对比
        /// </summary>
        /// <param name="id"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        [Route("{id:int}/history/{v1:int}-{v2:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "v1", "v2" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> CompareVersion(int id, int v1, int v2)
        {
            var post = await PostService.GetAsync(p => p.Id == id && (p.Status == Status.Published || CurrentUser.IsAdmin));
            var main = post.Mapper<PostHistoryVersion>() ?? throw new NotFoundException("文章未找到");
            CheckPermission(post);
            var left = v1 <= 0 ? main : await PostHistoryVersionService.GetAsync(v => v.Id == v1) ?? throw new NotFoundException("文章未找到");
            var right = v2 <= 0 ? main : await PostHistoryVersionService.GetAsync(v => v.Id == v2) ?? throw new NotFoundException("文章未找到");
            main.Id = id;
            var diff = new HtmlDiff.HtmlDiff(right.Content, left.Content);
            var diffOutput = diff.Build();
            right.Content = ReplaceVariables(Regex.Replace(Regex.Replace(diffOutput, "<ins.+?</ins>", string.Empty), @"<\w+></\w+>", string.Empty));
            right.ModifyDate = right.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            left.Content = ReplaceVariables(Regex.Replace(Regex.Replace(diffOutput, "<del.+?</del>", string.Empty), @"<\w+></\w+>", string.Empty));
            left.ModifyDate = left.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            ViewBag.Ads = AdsService.GetsByWeightedPrice(2, AdvertiseType.InPage, Request.Location(), main.CategoryId, main.Label);
            ViewBag.DisableCopy = post.DisableCopy;
            return View(new[] { main, left, right });
        }

        /// <summary>
        /// 反对
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> VoteDown(int id)
        {
            if (HttpContext.Session.Get("post-vote" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            var b = await PostService.GetQuery(p => p.Id == id).UpdateFromQueryAsync(p => new Post()
            {
                VoteDownCount = p.VoteDownCount + 1
            }) > 0;
            if (b)
            {
                HttpContext.Session.Set("post-vote" + id, id.GetBytes());
            }

            return ResultData(null, b, b ? "投票成功！" : "投票失败！");
        }

        /// <summary>
        /// 支持
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> VoteUp(int id)
        {
            if (HttpContext.Session.Get("post-vote" + id) != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            var b = await PostService.GetQuery(p => p.Id == id).UpdateFromQueryAsync(p => new Post()
            {
                VoteUpCount = p.VoteUpCount + 1
            }) > 0;
            if (b)
            {
                HttpContext.Session.Set("post-vote" + id, id.GetBytes());
            }

            return ResultData(null, b, b ? "投票成功！" : "投票失败！");
        }

        /// <summary>
        /// 投稿页
        /// </summary>
        /// <returns></returns>
        public ActionResult Publish()
        {
            return View();
        }

        /// <summary>
        /// 发布投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="code"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Publish(PostCommand post, [Required(ErrorMessage = "验证码不能为空")] string code, CancellationToken cancellationToken)
        {
            if (RedisHelper.Get("code:" + post.Email) != code)
            {
                return ResultData(null, false, "验证码错误！");
            }

            if (PostService.Any(p => p.Status == Status.Forbidden && p.Email == post.Email))
            {
                return ResultData(null, false, "由于您曾经恶意投稿，该邮箱已经被标记为黑名单，无法进行投稿，如有疑问，请联系网站管理员进行处理。");
            }

            var match = Regex.Match(post.Title + post.Author + post.Content, CommonHelper.BanRegex);
            if (match.Success)
            {
                LogManager.Info($"提交内容：{post.Title}/{post.Author}/{post.Content}，敏感词：{match.Value}");
                return ResultData(null, false, "您提交的内容包含敏感词，被禁止发表，请检查您的内容后尝试重新提交！");
            }

            if (!CategoryService.Any(c => c.Id == post.CategoryId))
            {
                return ResultData(null, message: "请选择一个分类");
            }

            post.Label = string.IsNullOrEmpty(post.Label?.Trim()) ? null : post.Label.Replace("，", ",");
            post.Status = Status.Pending;
            post.Content = await ImagebedClient.ReplaceImgSrc(await post.Content.HtmlSantinizerStandard().ClearImgAttributes(), cancellationToken);
            Post p = post.Mapper<Post>();
            p.IP = ClientIP;
            p.Modifier = p.Author;
            p.ModifierEmail = p.Email;
            p.DisableCopy = true;
            p.Rss = true;
            PostTagService.AddOrUpdate(t => t.Name, p.Label.AsNotNull().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => new PostTag()
            {
                Name = s,
                Count = PostService.Count(t => t.Label.Contains(s))
            }));
            p = PostService.AddEntitySaved(p);
            if (p == null)
            {
                return ResultData(null, false, "文章发表失败！");
            }

            RedisHelper.Expire("code:" + p.Email, 1);
            var content = new Template(await new FileInfo(HostEnvironment.WebRootPath + "/template/publish.html").ShareReadWrite().ReadAllTextAsync(Encoding.UTF8))
                .Set("link", Url.Action("Details", "Post", new { id = p.Id }, Request.Scheme))
                .Set("time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Set("title", p.Title).Render();
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "有访客投稿：", content, CommonHelper.SystemSettings["ReceiveEmail"], ClientIP));
            return ResultData(p.Mapper<PostDto>(), message: "文章发表成功，待站长审核通过以后将显示到列表中！");
        }

        /// <summary>
        /// 获取标签
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public ActionResult GetTag()
        {
            return ResultData(PostService.GetTags().Select(x => x.Key).OrderBy(s => s));
        }

        /// <summary>
        /// 标签云
        /// </summary>
        /// <returns></returns>
        [Route("all"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public async Task<ActionResult> All()
        {
            ViewBag.tags = new Dictionary<string, int>(PostService.GetTags().Where(x => x.Value > 1).OrderBy(x => x.Key));
            ViewBag.cats = await CategoryService.GetQuery(c => c.Post.Count > 0, c => c.Post.Count, false).Include(c => c.Parent).ThenInclude(c => c.Parent).AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Path()); //category
            ViewBag.seminars = await SeminarService.GetAll(c => c.Post.Count, false).AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Title); //seminars
            return View();
        }

        /// <summary>
        /// 检查访问密码
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall]
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
                    Expires = DateTime.Now.AddYears(1),
                    SameSite = SameSiteMode.Lax
                });
                Response.Cookies.Append("PostAccessToken", email.MDString3(AppConfig.BaiduAK), new CookieOptions
                {
                    Expires = DateTime.Now.AddYears(1),
                    SameSite = SameSiteMode.Lax
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
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall]
        public ActionResult GetViewToken(string email)
        {
            var validator = new IsEmailAttribute();
            if (!validator.IsValid(email))
            {
                return ResultData(null, false, validator.ErrorMessage);
            }

            if (RedisHelper.Exists("get:" + email))
            {
                RedisHelper.Expire("get:" + email, 120);
                return ResultData(null, false, "发送频率限制，请在2分钟后重新尝试发送邮件！请检查你的邮件，若未收到，请检查你的邮箱地址或邮件垃圾箱！");
            }

            if (!UserInfoService.Any(b => b.Email.Equals(email)))
            {
                return ResultData(null, false, "您目前没有权限访问这个链接，请联系站长开通访问权限！");
            }

            var token = SnowFlake.GetInstance().GetUniqueShortId(6);
            RedisHelper.Set("token:" + email, token, 86400);
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(Request.Host + "博客访问验证码", $"{Request.Host}本次验证码是：<span style='color:red'>{token}</span>，有效期为24h，请按时使用！", email, ClientIP));
            RedisHelper.Set("get:" + email, token, 120);
            return ResultData(null);
        }

        /// <summary>
        /// 文章合并
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/merge")]
        public async Task<ActionResult> PushMerge(int id)
        {
            var post = await PostService.GetAsync(p => p.Id == id && p.Status == Status.Published && !p.Locked) ?? throw new NotFoundException("文章未找到");
            CheckPermission(post);
            return View(post);
        }

        /// <summary>
        /// 文章合并
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mid"></param>
        /// <returns></returns>
        [HttpGet("{id}/merge/{mid}")]
        public async Task<ActionResult> RepushMerge(int id, int mid)
        {
            var post = await PostService.GetAsync(p => p.Id == id && p.Status == Status.Published && !p.Locked) ?? throw new NotFoundException("文章未找到");
            CheckPermission(post);
            var merge = post.PostMergeRequests.FirstOrDefault(p => p.Id == mid && p.MergeState != MergeStatus.Merged) ?? throw new NotFoundException("待合并文章未找到");
            return View(merge);
        }

        /// <summary>
        /// 文章合并
        /// </summary>
        /// <param name="messageService"></param>
        /// <param name="postMergeRequestService"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("{id}/pushmerge")]
        public async Task<ActionResult> PushMerge([FromServices] IInternalMessageService messageService, [FromServices] IPostMergeRequestService postMergeRequestService, PostMergeRequestCommand dto)
        {
            if (RedisHelper.Get("code:" + dto.ModifierEmail) != dto.Code)
            {
                return ResultData(null, false, "验证码错误！");
            }

            var post = await PostService.GetAsync(p => p.Id == dto.PostId && p.Status == Status.Published && !p.Locked) ?? throw new NotFoundException("文章未找到");
            if (post.Title.Equals(dto.Title) && post.Content.HammingDistance(dto.Content) <= 1)
            {
                return ResultData(null, false, "内容未被修改或修改的内容过少(无意义修改)！");
            }

            #region 合并验证

            if (postMergeRequestService.Any(p => p.ModifierEmail == dto.ModifierEmail && p.MergeState == MergeStatus.Block))
            {
                return ResultData(null, false, "由于您曾经多次恶意修改文章，已经被标记为黑名单，无法修改任何文章，如有疑问，请联系网站管理员进行处理。");
            }

            if (post.PostMergeRequests.Any(p => p.ModifierEmail == dto.ModifierEmail && p.MergeState == MergeStatus.Pending))
            {
                return ResultData(null, false, "您已经提交过一次修改请求正在待处理，暂不能继续提交修改请求！");
            }

            #endregion 合并验证

            #region 直接合并

            if (post.Email.Equals(dto.ModifierEmail))
            {
                var history = post.Mapper<PostHistoryVersion>();
                Mapper.Map(dto, post);
                post.PostHistoryVersion.Add(history);
                post.ModifyDate = DateTime.Now;
                return await PostService.SaveChangesAsync() > 0 ? ResultData(null, true, "你是文章原作者，无需审核，文章已自动更新并在首页展示！") : ResultData(null, false, "操作失败！");
            }

            #endregion 直接合并

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
                merge.SubmitTime = DateTime.Now;
                post.PostMergeRequests.Add(merge);
            }
            merge.IP = ClientIP;
            var b = await PostService.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "操作失败！");
            }

            RedisHelper.Expire("code:" + dto.ModifierEmail, 1);
            await messageService.AddEntitySavedAsync(new InternalMessage()
            {
                Title = $"来自【{dto.Modifier}】对文章《{post.Title}》的修改请求",
                Content = dto.Title,
                Link = "#/merge/compare?id=" + merge.Id
            });

            var htmlDiff = new HtmlDiff.HtmlDiff(post.Content.RemoveHtmlTag(), dto.Content.RemoveHtmlTag());
            var diff = htmlDiff.Build();
            var content = new Template(await new FileInfo(HostEnvironment.WebRootPath + "/template/merge-request.html").ShareReadWrite().ReadAllTextAsync(Encoding.UTF8))
                .Set("title", post.Title)
                .Set("link", Url.Action("Index", "Dashboard", new { }, Request.Scheme) + "#/merge/compare?id=" + merge.Id)
                .Set("diff", diff)
                .Set("host", "//" + Request.Host)
                .Set("id", merge.Id.ToString())
                .Render();
            BackgroundJob.Enqueue(() => CommonHelper.SendMail("博客文章修改请求：", content, CommonHelper.SystemSettings["ReceiveEmail"], ClientIP));
            return ResultData(null, true, "您的修改请求已提交，已进入审核状态，感谢您的参与！");
        }

        #region 后端管理

        /// <summary>
        /// 固顶
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Fixtop(int id)
        {
            Post post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.IsFixedTop = !post.IsFixedTop;
            bool b = await PostService.SaveChangesAsync() > 0;
            return b ? ResultData(null, true, post.IsFixedTop ? "置顶成功！" : "取消置顶成功！") : ResultData(null, false, "操作失败！");
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Pass(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.Status = Status.Published;
            post.ModifyDate = DateTime.Now;
            post.PostDate = DateTime.Now;
            var b = await PostService.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "审核失败！");
            }

            var js = new JiebaSegmenter();
            (post.Keyword + "," + post.Label).Split(',', StringSplitOptions.RemoveEmptyEntries).ForEach(s => js.AddWord(s));
            SearchEngine.LuceneIndexer.Add(post);
            return ResultData(null, true, "审核通过！");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Delete(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.Status = Status.Deleted;
            bool b = await PostService.SaveChangesAsync(true) > 0;
            SearchEngine.LuceneIndexer.Delete(post);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 还原版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Restore(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.Status = Status.Published;
            bool b = await PostService.SaveChangesAsync() > 0;
            SearchEngine.LuceneIndexer.Add(post);
            return ResultData(null, b, b ? "恢复成功！" : "恢复失败！");
        }

        /// <summary>
        /// 彻底删除文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult Truncate(int id)
        {
            bool b = PostService - id;
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult Get(int id)
        {
            Post post = PostService[id] ?? throw new NotFoundException("文章未找到");
            PostDto model = post.Mapper<PostDto>();
            model.Seminars = post.Seminar.Select(s => s.Id).Join(",");
            return ResultData(model);
        }

        /// <summary>
        /// 获取文章分页
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> GetPageData([FromServices] ICacheManager<HashSet<string>> cacheManager, int page = 1, [Range(1, 200, ErrorMessage = "页大小必须介于{1}-{2}")] int size = 10, OrderBy orderby = OrderBy.ModifyDate, string kw = "", int? cid = null)
        {
            Expression<Func<Post, bool>> where = p => true;
            if (cid.HasValue)
            {
                where = where.And(p => p.CategoryId == cid.Value || p.Category.ParentId == cid.Value || p.Category.Parent.ParentId == cid.Value);
            }

            if (!string.IsNullOrEmpty(kw))
            {
                kw = Regex.Escape(kw);
                where = where.And(p => Regex.IsMatch(p.Title + p.Author + p.Email + p.Content, kw, RegexOptions.IgnoreCase));
            }

            var list = orderby switch
            {
                OrderBy.Trending => await PostService.GetQuery(where).OrderByDescending(p => p.Status).ThenByDescending(p => p.IsFixedTop).ThenByDescending(p => p.PostVisitRecordStats.Sum(t => t.Count) / p.PostVisitRecordStats.Count).ToPagedListAsync<Post, PostDataModel>(page, size, MapperConfig),
                _ => await PostService.GetQuery(where).OrderBy($"{nameof(Post.Status)} desc,{nameof(Post.IsFixedTop)} desc,{orderby.GetDisplay()} desc").ToPagedListAsync<Post, PostDataModel>(page, size, MapperConfig)
            };
            foreach (var item in list.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                item.PostDate = item.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                item.Online = cacheManager.Get(nameof(PostOnline) + ":" + item.Id)?.Count ?? 0;
            }

            return Ok(list);
        }

        /// <summary>
        /// 获取未审核文章
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> GetPending([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15, string search = "")
        {
            Expression<Func<Post, bool>> where = p => p.Status == Status.Pending;
            if (!string.IsNullOrEmpty(search))
            {
                where = where.And(p => p.Title.Contains(search) || p.Author.Contains(search) || p.Email.Contains(search) || p.Label.Contains(search));
            }

            var pages = await PostService.GetQuery(where).OrderByDescending(p => p.IsFixedTop).ThenByDescending(p => p.ModifyDate).ToPagedListAsync<Post, PostDataModel>(page, size, MapperConfig);
            foreach (var item in pages.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                item.PostDate = item.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return Ok(pages);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        /// <param name="post"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost, MyAuthorize]
        public async Task<ActionResult> Edit([FromBodyOrDefault] PostCommand post, CancellationToken cancellationToken = default)
        {
            post.Content = await ImagebedClient.ReplaceImgSrc(await post.Content.Trim().ClearImgAttributes(), cancellationToken);
            if (!ValidatePost(post, out var resultData))
            {
                return resultData;
            }

            Post p = await PostService.GetByIdAsync(post.Id);
            if (post.Reserve && p.Status == Status.Published)
            {
                if (p.Content.HammingDistance(post.Content) > 0)
                {
                    var history = p.Mapper<PostHistoryVersion>();
                    p.PostHistoryVersion.Add(history);
                }

                p.ModifyDate = DateTime.Now;
                var user = HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
                post.Modifier = string.IsNullOrEmpty(post.Modifier) ? user.NickName : post.Modifier;
                post.ModifierEmail = string.IsNullOrEmpty(post.ModifierEmail) ? user.Email : post.ModifierEmail;
            }

            Mapper.Map(post, p);
            p.IP = ClientIP;
            p.Seminar.Clear();
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct().Select(int.Parse).ToArray();
                var seminars = SeminarService.GetQuery(s => tmp.Contains(s.Id)).ToList();
                p.Seminar.AddRange(seminars);
            }

            var js = new JiebaSegmenter();
            (p.Keyword + "," + p.Label).Split(',', StringSplitOptions.RemoveEmptyEntries).ForEach(s => js.AddWord(s));
            PostTagService.AddOrUpdate(t => t.Name, p.Label.AsNotNull().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => new PostTag()
            {
                Name = s,
                Count = PostService.Count(t => t.Label.Contains(s))
            }));
            bool b = await SearchEngine.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "文章修改失败！");
            }

            if (p.LimitMode == RegionLimitMode.OnlyForSearchEngine)
            {
                SearchEngine.LuceneIndexer.Delete(p);
            }
            return ResultData(p.Mapper<PostDto>(), message: "文章修改成功！");
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="post"></param>
        /// <param name="timespan"></param>
        /// <param name="schedule"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MyAuthorize, HttpPost]
        public async Task<ActionResult> Write([FromBodyOrDefault] PostCommand post, [FromBodyOrDefault] DateTime? timespan, [FromBodyOrDefault] bool schedule = false, CancellationToken cancellationToken = default)
        {
            post.Content = await ImagebedClient.ReplaceImgSrc(await post.Content.Trim().ClearImgAttributes(), cancellationToken);
            if (!ValidatePost(post, out var resultData))
            {
                return resultData;
            }

            post.Status = Status.Published;
            Post p = post.Mapper<Post>();
            p.Modifier = p.Author;
            p.ModifierEmail = p.Email;
            p.IP = ClientIP;
            p.Rss = p.LimitMode is null or RegionLimitMode.All;
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct().Select(int.Parse).ToArray();
                p.Seminar.AddRange(SeminarService[s => tmp.Contains(s.Id)]);
            }

            if (schedule)
            {
                if (!timespan.HasValue || timespan.Value <= DateTime.Now)
                {
                    return ResultData(null, false, "如果要定时发布，请选择正确的一个将来时间点！");
                }

                p.Status = Status.Schedule;
                p.PostDate = timespan.Value.ToUniversalTime();
                p.ModifyDate = timespan.Value.ToUniversalTime();
                BackgroundJob.Enqueue<IHangfireBackJob>(job => job.PublishPost(p));
                return ResultData(p.Mapper<PostDto>(), message: $"文章于{timespan.Value:yyyy-MM-dd HH:mm:ss}将会自动发表！");
            }

            PostService.AddEntity(p);
            var js = new JiebaSegmenter();
            (p.Keyword + "," + p.Label).Split(',', StringSplitOptions.RemoveEmptyEntries).ForEach(s => js.AddWord(s));
            PostTagService.AddOrUpdate(t => t.Name, p.Label.AsNotNull().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => new PostTag()
            {
                Name = s,
                Count = PostService.Count(t => t.Label.Contains(s))
            }));
            bool b = await SearchEngine.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "文章发表失败！");
            }

            if (p.LimitMode == RegionLimitMode.OnlyForSearchEngine)
            {
                SearchEngine.LuceneIndexer.Delete(p);
            }

            return ResultData(null, true, "文章发表成功！");
        }

        private bool ValidatePost(PostCommand post, out ActionResult resultData)
        {
            if (!CategoryService.Any(c => c.Id == post.CategoryId && c.Status == Status.Available))
            {
                resultData = ResultData(null, false, "请选择一个分类");
                return false;
            }

            switch (post.LimitMode)
            {
                case RegionLimitMode.AllowRegion:
                case RegionLimitMode.ForbidRegion:
                    if (string.IsNullOrEmpty(post.Regions))
                    {
                        resultData = ResultData(null, false, "请输入限制的地区");
                        return false;
                    }

                    post.Regions = post.Regions.Replace(",", "|").Replace("，", "|");
                    break;

                case RegionLimitMode.AllowRegionExceptForbidRegion:
                case RegionLimitMode.ForbidRegionExceptAllowRegion:
                    if (string.IsNullOrEmpty(post.ExceptRegions))
                    {
                        resultData = ResultData(null, false, "请输入排除的地区");
                        return false;
                    }

                    post.ExceptRegions = post.ExceptRegions.Replace(",", "|").Replace("，", "|");
                    goto case RegionLimitMode.AllowRegion;
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

            if (string.IsNullOrEmpty(post.ProtectContent?.RemoveHtmlTag()) || post.ProtectContent.Equals("null"))
            {
                post.ProtectContent = null;
            }

            resultData = null;
            return true;
        }

        /// <summary>
        /// 添加专题
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> AddSeminar(int id, int sid)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            Seminar seminar = await SeminarService.GetByIdAsync(sid) ?? throw new NotFoundException("专题未找到");
            post.Seminar.Add(seminar);
            bool b = await PostService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? $"已将文章【{post.Title}】添加到专题【{seminar.Title}】" : "添加失败");
        }

        /// <summary>
        /// 移除专题
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sid"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> RemoveSeminar(int id, int sid)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            Seminar seminar = await SeminarService.GetByIdAsync(sid) ?? throw new NotFoundException("专题未找到");
            post.Seminar.Remove(seminar);
            bool b = await PostService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? $"已将文章【{post.Title}】从【{seminar.Title}】专题移除" : "添加失败");
        }

        /// <summary>
        /// 删除历史版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> DeleteHistory(int id)
        {
            bool b = await PostHistoryVersionService.DeleteByIdAsync(id) > 0;
            return ResultData(null, b, b ? "历史版本文章删除成功！" : "历史版本文章删除失败！");
        }

        /// <summary>
        /// 还原版本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Revert(int id)
        {
            var history = await PostHistoryVersionService.GetByIdAsync(id) ?? throw new NotFoundException("版本不存在");
            history.Post.Category = history.Category;
            history.Post.CategoryId = history.CategoryId;
            history.Post.Content = history.Content;
            history.Post.Title = history.Title;
            history.Post.Label = history.Label;
            history.Post.ModifyDate = history.ModifyDate;
            history.Post.Seminar.Clear();
            foreach (var s in history.Seminar)
            {
                history.Post.Seminar.Add(s);
            }
            bool b = await SearchEngine.SaveChangesAsync() > 0;
            await PostHistoryVersionService.DeleteByIdAsync(id);
            return ResultData(null, b, b ? "回滚成功" : "回滚失败");
        }

        /// <summary>
        /// 禁用或开启文章评论
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        [MyAuthorize]
        [HttpPost("post/{id}/DisableComment")]
        public async Task<ActionResult> DisableComment(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.DisableComment = !post.DisableComment;
            return ResultData(null, await PostService.SaveChangesAsync() > 0, post.DisableComment ? $"已禁用【{post.Title}】这篇文章的评论功能！" : $"已启用【{post.Title}】这篇文章的评论功能！");
        }

        /// <summary>
        /// 禁用或开启文章评论
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        [MyAuthorize]
        [HttpPost("post/{id}/DisableCopy")]
        public async Task<ActionResult> DisableCopy(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.DisableCopy = !post.DisableCopy;
            return ResultData(null, await PostService.SaveChangesAsync() > 0, post.DisableCopy ? $"已开启【{post.Title}】这篇文章的防复制功能！" : $"已关闭【{post.Title}】这篇文章的防复制功能！");
        }

        /// <summary>
        /// 修改分类
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cid"></param>
        /// <returns></returns>
        [HttpPost("post/{id}/ChangeCategory/{cid}")]
        public async Task<ActionResult> ChangeCategory(int id, int cid)
        {
            await PostService.GetQuery(p => p.Id == id).UpdateFromQueryAsync(p => new Post()
            {
                CategoryId = cid
            });
            return Ok();
        }

        /// <summary>
        /// 修改专题
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost("post/{id}/ChangeSeminar/{sids}")]
        public async Task<ActionResult> ChangeSeminar(int id, string sids)
        {
            var post = PostService[id] ?? throw new NotFoundException("文章不存在");
            var ids = sids.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
            post.Seminar.Clear();
            post.Seminar.AddRange(SeminarService[s => ids.Contains(s.Id)]);
            await PostService.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// 刷新文章
        /// </summary>
        /// <param name="id">文章id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Refresh(int id, CancellationToken cancellationToken = default)
        {
            await PostService.GetQuery(p => p.Id == id).UpdateFromQueryAsync(p => new Post()
            {
                ModifyDate = DateTime.Now
            }, cancellationToken);
            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// 标记为恶意修改
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MyAuthorize]
        [HttpPost("post/block/{id}")]
        public async Task<ActionResult> Block(int id, CancellationToken cancellationToken = default)
        {
            var b = await PostService.GetQuery(p => p.Id == id).UpdateFromQueryAsync(p => new Post()
            {
                Status = Status.Forbidden
            }, cancellationToken) > 0;
            return b ? ResultData(null, true, "操作成功！") : ResultData(null, false, "操作失败！");
        }

        /// <summary>
        /// 切换允许rss订阅
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MyAuthorize]
        [HttpPost("post/{id}/rss-switch")]
        public async Task<ActionResult> RssSwitch(int id, CancellationToken cancellationToken = default)
        {
            await PostService.GetQuery(p => p.Id == id).UpdateFromQueryAsync(p => new Post()
            {
                Rss = !p.Rss
            }, cancellationToken);
            return ResultData(null, message: "操作成功");
        }

        /// <summary>
        /// 切换锁定编辑
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [MyAuthorize]
        [HttpPost("post/{id}/locked-switch")]
        public async Task<ActionResult> LockedSwitch(int id, CancellationToken cancellationToken = default)
        {
            await PostService.GetQuery(p => p.Id == id).UpdateFromQueryAsync(p => new Post()
            {
                Locked = !p.Locked
            }, cancellationToken);
            return ResultData(null, message: "操作成功");
        }

        /// <summary>
        /// 文章统计
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<IActionResult> Statistic(CancellationToken cancellationToken = default)
        {
            var keys = RedisHelper.Keys(nameof(PostOnline) + ":*");
            var sets = keys.Select(s => (Id: s.Split(':')[1].ToInt32(), Clients: RedisHelper.HGet<HashSet<string>>(s, "value")));
            var ids = sets.Where(t => t.Clients?.Count > 0).OrderByDescending(t => t.Clients.Count).Take(10).Select(t => t.Id).ToArray();
            var mostHots = await PostService.GetQuery<PostModelBase>(p => ids.Contains(p.Id)).ToListAsync().ContinueWith(t =>
            {
                foreach (var item in t.Result)
                {
                    item.ViewCount = sets.FirstOrDefault(x => x.Id == item.Id).Clients.Count;
                }

                return t.Result.OrderByDescending(p => p.ViewCount);
            });
            var postsQuery = PostService.GetQuery(p => p.Status == Status.Published);
            var mostView = await postsQuery.OrderByDescending(p => p.TotalViewCount).Take(10).Select(p => new PostModelBase()
            {
                Id = p.Id,
                Title = p.Title,
                ViewCount = p.TotalViewCount
            }).ToListAsync(cancellationToken);
            var mostAverage = await postsQuery.OrderByDescending(p => p.AverageViewCount).Take(10).Select(p => new PostModelBase()
            {
                Id = p.Id,
                Title = p.Title,
                ViewCount = (int)p.AverageViewCount
            }).ToListAsync(cancellationToken);
            var yesterday = DateTime.Now.AddDays(-1);
            var trending = await postsQuery.Select(p => new PostModelBase()
            {
                Id = p.Id,
                Title = p.Title,
                ViewCount = p.PostVisitRecords.Count(t => t.Time >= yesterday)
            }).OrderByDescending(p => p.ViewCount).Take(10).ToListAsync(cancellationToken);
            var readCount = PostVisitRecordService.Count(e => e.Time >= yesterday);
            return ResultData(new
            {
                mostHots,
                mostView,
                mostAverage,
                trending,
                readCount
            });
        }

        /// <summary>
        /// 文章访问记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet("/{id}/records"), MyAuthorize]
        [ProducesResponseType(typeof(PagedList<PostVisitRecordViewModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostVisitRecords(int id, int page = 1, int size = 15, string kw = "")
        {
            Expression<Func<PostVisitRecord, bool>> where = e => e.PostId == id;
            if (!string.IsNullOrEmpty(kw))
            {
                kw = Regex.Escape(kw);
                where = where.And(e => Regex.IsMatch(e.IP + e.Location + e.Referer + e.RequestUrl, kw, RegexOptions.IgnoreCase));
            }

            var pages = await PostVisitRecordService.GetPagesAsync<DateTime, PostVisitRecordViewModel>(page, size, where, e => e.Time, false);
            return Ok(pages);
        }

        /// <summary>
        /// 导出文章访问记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/{id}/records-export"), MyAuthorize]
        [ProducesResponseType(typeof(PagedList<PostVisitRecordViewModel>), (int)HttpStatusCode.OK)]
        public IActionResult ExportPostVisitRecords(int id)
        {
            using var list = PostVisitRecordService.GetQuery<DateTime, PostVisitRecordViewModel>(e => e.PostId == id, e => e.Time, false).ToPooledList();
            using var ms = list.ToDataTable().ToExcel();
            var post = PostService[id];
            return this.ResumeFile(ms.ToArray(), ContentType.Xlsx, post.Title + "访问记录.xlsx");
        }

        /// <summary>
        /// 文章访问记录图表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("/{id}/records-chart"), MyAuthorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostVisitRecordChart(int id, CancellationToken cancellationToken)
        {
            var list = await PostVisitRecordService.GetQuery(e => e.PostId == id).Select(e => e.Time).GroupBy(t => t.Date).Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            }).OrderBy(a => a.Date).ToListAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// 文章访问记录图表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("/post/records-chart"), MyAuthorize]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostVisitRecordChart(CancellationToken cancellationToken)
        {
            var list = await PostVisitRecordService.GetAll().Select(e => new { e.Time.Date, e.IP }).GroupBy(t => t.Date).Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                UV = g.Select(e => e.IP).Distinct().Count()
            }).OrderBy(a => a.Date).ToListAsync(cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// 文章访问记录分析
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/{id}/insight"), MyAuthorize]
        [ProducesResponseType(typeof(PagedList<PostVisitRecordViewModel>), (int)HttpStatusCode.OK)]
        public IActionResult PostVisitRecordInsight(int id)
        {
            return View(PostService[id]);
        }

        /// <summary>
        /// 获取地区集
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [MyAuthorize]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetRegions(string name)
        {
            return ResultData(await PostService.GetAll().Select(name).Distinct().ToDynamicListAsync());
        }

        #endregion 后端管理
    }
}
