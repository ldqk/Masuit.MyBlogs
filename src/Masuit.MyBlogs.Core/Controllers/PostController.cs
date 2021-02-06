using Hangfire;
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
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Html;
using Masuit.Tools.Linq;
using Masuit.Tools.Logging;
using Masuit.Tools.Security;
using Masuit.Tools.Strings;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        /// <summary>
        /// 文章详情页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="kw"></param>
        /// <returns></returns>
        [Route("{id:int}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Details(int id, string kw)
        {
            var post = await PostService.GetAsync(p => p.Id == id && (p.Status == Status.Published || CurrentUser.IsAdmin)) ?? throw new NotFoundException("文章未找到");
            CheckPermission(post);
            ViewBag.Keyword = post.Keyword + "," + post.Label;
            ViewBag.Desc = post.Content.GetSummary(200);
            var modifyDate = post.ModifyDate;
            ViewBag.Next = PostService.GetFromCache<DateTime, PostModelBase>(p => p.ModifyDate > modifyDate && (p.Status == Status.Published || CurrentUser.IsAdmin), p => p.ModifyDate);
            ViewBag.Prev = PostService.GetFromCache<DateTime, PostModelBase>(p => p.ModifyDate < modifyDate && (p.Status == Status.Published || CurrentUser.IsAdmin), p => p.ModifyDate, false);
            if (!string.IsNullOrEmpty(kw))
            {
                ViewData["keywords"] = post.Content.Contains(kw) ? $"['{kw}']" : SearchEngine.LuceneIndexSearcher.CutKeywords(kw).ToJsonString();
            }

            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, post.CategoryId);
            var related = PostService.ScoreSearch(1, 11, string.IsNullOrWhiteSpace(post.Keyword + post.Label) ? post.Title : post.Keyword + post.Label);
            related.RemoveAll(p => p.Id == id);
            if (related.Count <= 1)
            {
                related = (await PostService.GetPagesFromCacheAsync(1, 10, p => p.Id != id && p.CategoryId == post.CategoryId, p => p.TotalViewCount, false)).Data;
            }

            ViewBag.Related = related;
            post.ModifyDate = post.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            post.PostDate = post.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            post.Content = ReplaceVariables(post.Content);
            post.ProtectContent = ReplaceVariables(post.ProtectContent);
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

        private void CheckPermission(Post post)
        {
            var location = Request.Location() + "|" + Request.Headers[HeaderNames.UserAgent];
            switch (post.LimitMode)
            {
                case PostLimitMode.AllowRegion:
                    if (!location.Contains(post.Regions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid && !Request.IsRobot())
                    {
                        Disallow(post);
                    }

                    break;
                case PostLimitMode.ForbidRegion:
                    if (location.Contains(post.Regions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid && !Request.IsRobot())
                    {
                        Disallow(post);
                    }

                    break;
                case PostLimitMode.AllowRegionExceptForbidRegion:
                    if (location.Contains(post.ExceptRegions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid)
                    {
                        Disallow(post);
                    }

                    goto case PostLimitMode.AllowRegion;
                case PostLimitMode.ForbidRegionExceptAllowRegion:
                    if (location.Contains(post.ExceptRegions.Split(',', StringSplitOptions.RemoveEmptyEntries)) && !CurrentUser.IsAdmin && !VisitorTokenValid)
                    {
                        break;
                    }

                    goto case PostLimitMode.ForbidRegion;
            }
        }

        private void Disallow(Post post)
        {
            BackgroundJob.Enqueue(() => HangfireBackJob.InterceptLog(new IpIntercepter()
            {
                IP = ClientIP,
                RequestUrl = $"//{Request.Host}/{post.Id}",
                Time = DateTime.Now,
                UserAgent = Request.Headers[HeaderNames.UserAgent],
                Remark = "无权限查看该文章"
            }));
            throw new NotFoundException("文章未找到");
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
            var list = PostHistoryVersionService.GetPages(page, size, v => v.PostId == id, v => v.ModifyDate, false);
            foreach (var item in list.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, post.CategoryId);
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
            var post = await PostHistoryVersionService.GetAsync(v => v.Id == hid && (v.Post.Status == Status.Published || CurrentUser.IsAdmin)) ?? throw new NotFoundException("文章未找到");
            CheckPermission(post.Post);
            post.Content = ReplaceVariables(post.Content);
            post.ProtectContent = ReplaceVariables(post.ProtectContent);
            var next = await PostHistoryVersionService.GetAsync(p => p.PostId == id && p.ModifyDate > post.ModifyDate, p => p.ModifyDate);
            var prev = await PostHistoryVersionService.GetAsync(p => p.PostId == id && p.ModifyDate < post.ModifyDate, p => p.ModifyDate, false);
            ViewBag.Next = next;
            ViewBag.Prev = prev;
            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.InPage, post.CategoryId);
            return CurrentUser.IsAdmin ? View("HistoryVersion_Admin", post) : View(post);
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
            left.Content = ReplaceVariables(Regex.Replace(Regex.Replace(diffOutput, "<del.+?</del>", string.Empty), @"<\w+></\w+>", string.Empty));
            ViewBag.Ads = AdsService.GetsByWeightedPrice(2, AdvertiseType.InPage, main.CategoryId);
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

            Post post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.VoteDownCount = post.VoteDownCount + 1;
            var b = await PostService.SaveChangesAsync() > 0;
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

            Post post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.VoteUpCount += 1;
            var b = await PostService.SaveChangesAsync() > 0;
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
        public async Task<ActionResult> Publish()
        {
            var list = await CategoryService.GetQueryFromCacheAsync(c => c.Status == Status.Available).ConfigureAwait(false);
            return View(list);
        }

        /// <summary>
        /// 发布投稿
        /// </summary>
        /// <param name="post"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Publish(PostCommand post, [Required(ErrorMessage = "验证码不能为空")] string code)
        {
            if (await RedisHelper.GetAsync("code:" + post.Email) != code)
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
            post.Content = await ImagebedClient.ReplaceImgSrc(post.Content.HtmlSantinizerStandard().ClearImgAttributes()).ConfigureAwait(false);
            Post p = post.Mapper<Post>();
            p.IP = ClientIP;
            p.Modifier = p.Author;
            p.ModifierEmail = p.Email;
            p.DisableCopy = true;
            p = PostService.AddEntitySaved(p);
            if (p == null)
            {
                return ResultData(null, false, "文章发表失败！");
            }

            await RedisHelper.ExpireAsync("code:" + p.Email, 1);
            var content = new Template(await System.IO.File.ReadAllTextAsync(HostEnvironment.WebRootPath + "/template/publish.html"))
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
            var list = PostService.GetQuery(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).Distinct().ToList().SelectMany(s => s.Split(',', '，')).GroupBy(s => s).Where(g => g.Count() > 1).OrderBy(s => s.Key).Select(g => g.Key).ToHashSet();
            return ResultData(list);
        }

        /// <summary>
        /// 标签云
        /// </summary>
        /// <returns></returns>
        [Route("all"), ResponseCache(Duration = 600, VaryByHeader = "Cookie")]
        public ActionResult All()
        {
            var tags = PostService.GetQuery(p => !string.IsNullOrEmpty(p.Label)).Select(p => p.Label).ToList().SelectMany(s => s.Split(',', '，')).GroupBy(t => t).Where(g => g.Count() > 1).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).ToList(); //tag
            ViewBag.tags = tags;
            ViewBag.cats = CategoryService.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Count = c.Post.Count(p => p.Status == Status.Published || CurrentUser.IsAdmin)
            }).ToList(); //category
            ViewBag.seminars = SeminarService.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel
            {
                Id = c.Id,
                Name = c.Title,
                Count = c.Post.Count(p => p.Status == Status.Published || CurrentUser.IsAdmin)
            }).ToList(); //seminars
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
        [HttpPost, ValidateAntiForgeryToken, AllowAccessFirewall, ResponseCache(Duration = 115, VaryByQueryKeys = new[] { "email" })]
        public ActionResult GetViewToken(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.MatchEmail().isMatch)
            {
                return ResultData(null, false, "请输入正确的邮箱！");
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
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
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
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
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
            if (await RedisHelper.GetAsync("code:" + dto.ModifierEmail) != dto.Code)
            {
                return ResultData(null, false, "验证码错误！");
            }

            var post = await PostService.GetByIdAsync(dto.PostId) ?? throw new NotFoundException("文章未找到");
            var htmlDiff = new HtmlDiff.HtmlDiff(post.Content.RemoveHtmlTag(), dto.Content.RemoveHtmlTag());
            var diff = htmlDiff.Build();
            if (post.Title.Equals(dto.Title) && !diff.Contains(new[] { "diffmod", "diffdel", "diffins" }))
            {
                return ResultData(null, false, "内容未被修改！");
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

            #endregion

            #region 直接合并

            if (post.Email.Equals(dto.ModifierEmail))
            {
                var history = post.Mapper<PostHistoryVersion>();
                Mapper.Map(dto, post);
                post.PostHistoryVersion.Add(history);
                post.ModifyDate = DateTime.Now;
                return await PostService.SaveChangesAsync() > 0 ? ResultData(null, true, "你是文章原作者，无需审核，文章已自动更新并在首页展示！") : ResultData(null, false, "操作失败！");
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
                merge.SubmitTime = DateTime.Now;
                post.PostMergeRequests.Add(merge);
            }

            var b = await PostService.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "操作失败！");
            }

            await RedisHelper.ExpireAsync("code:" + dto.ModifierEmail, 1);
            await messageService.AddEntitySavedAsync(new InternalMessage()
            {
                Title = $"来自【{dto.Modifier}】对文章《{post.Title}》的修改请求",
                Content = dto.Title,
                Link = "#/merge/compare?id=" + merge.Id
            });
            var content = new Template(await System.IO.File.ReadAllTextAsync(HostEnvironment.WebRootPath + "/template/merge-request.html"))
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
            Post post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.Status = Status.Published;
            post.ModifyDate = DateTime.Now;
            post.PostDate = DateTime.Now;
            bool b = await PostService.SaveChangesAsync() > 0;
            if (!b)
            {
                SearchEngine.LuceneIndexer.Add(post);
                return ResultData(null, false, "审核失败！");
            }

            if ("true" == CommonHelper.SystemSettings["DisabledEmailBroadcast"])
            {
                return ResultData(null, true, "审核通过！");
            }

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
        public async Task<ActionResult> Truncate(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            var srcs = post.Content.MatchImgSrcs();
            foreach (var path in srcs)
            {
                if (path.StartsWith("/"))
                {
                    try
                    {
                        System.IO.File.Delete(HostEnvironment.WebRootPath + path);
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            bool b = PostService - id;
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 获取文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Get(int id)
        {
            Post post = PostService[id] ?? throw new NotFoundException("文章未找到");
            PostDto model = post.Mapper<PostDto>();
            model.Seminars = post.Seminar.Select(s => s.Title).Join(",");
            return ResultData(model);
        }

        /// <summary>
        /// 获取文章分页
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页数必须大于0")] int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")] int size = 10, OrderBy orderby = OrderBy.ModifyDate, string kw = "", int? cid = null)
        {
            Expression<Func<Post, bool>> where = p => true;
            if (cid.HasValue)
            {
                where = where.And(p => p.CategoryId == cid.Value);
            }

            if (!string.IsNullOrEmpty(kw))
            {
                where = where.And(p => p.Title.Contains(kw) || p.Author.Contains(kw) || p.Email.Contains(kw) || p.Label.Contains(kw) || p.Content.Contains(kw));
            }

            var list = PostService.GetQuery(where).OrderBy($"{nameof(Post.Status)} desc,{nameof(Post.IsFixedTop)} desc,{orderby.GetDisplay()} desc").ToPagedList<Post, PostDataModel>(page, size, MapperConfig);
            foreach (var item in list.Data)
            {
                item.ModifyDate = item.ModifyDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
                item.PostDate = item.PostDate.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
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

            var pages = await PostService.GetQuery(where).OrderByDescending(p => p.IsFixedTop).ThenByDescending(p => p.ModifyDate).ToCachedPagedListAsync<Post, PostDataModel>(page, size, MapperConfig);
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
        /// <param name="reserve">是否保留历史版本</param>
        /// <returns></returns>
        [HttpPost, MyAuthorize]
        public async Task<ActionResult> Edit(PostCommand post, bool reserve = true)
        {
            post.Content = await ImagebedClient.ReplaceImgSrc(post.Content.Trim().ClearImgAttributes());
            if (!ValidatePost(post, out var resultData))
            {
                return resultData;
            }

            Post p = await PostService.GetByIdAsync(post.Id);
            if (reserve && p.Status == Status.Published)
            {
                var history = p.Mapper<PostHistoryVersion>();
                p.PostHistoryVersion.Add(history);
                p.ModifyDate = DateTime.Now;
                var user = HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo);
                p.Modifier = user.NickName;
                p.ModifierEmail = user.Email;
            }

            p.IP = ClientIP;
            Mapper.Map(post, p);
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct();
                p.Seminar.Clear();
                foreach (var s in tmp)
                {
                    var seminar = await SeminarService.GetAsync(e => e.Title.Equals(s));
                    if (seminar != null)
                    {
                        p.Seminar.Add(seminar);
                    }
                }
            }

            bool b = await SearchEngine.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "文章修改失败！");
            }

            return ResultData(p.Mapper<PostDto>(), message: "文章修改成功！");
        }

        /// <summary>
        /// 发布
        /// </summary>
        /// <param name="post"></param>
        /// <param name="timespan"></param>
        /// <param name="schedule"></param>
        /// <returns></returns>
        [MyAuthorize, HttpPost]
        public async Task<ActionResult> Write(PostCommand post, DateTime? timespan, bool schedule = false)
        {
            post.Content = await ImagebedClient.ReplaceImgSrc(post.Content.Trim().ClearImgAttributes());
            if (!ValidatePost(post, out var resultData))
            {
                return resultData;
            }

            post.Status = Status.Published;
            Post p = post.Mapper<Post>();
            p.Modifier = p.Author;
            p.ModifierEmail = p.Email;
            p.IP = ClientIP;
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct();
                foreach (var s in tmp)
                {
                    var id = s.ToInt32();
                    Seminar seminar = await SeminarService.GetByIdAsync(id);
                    p.Seminar.Add(seminar);
                }
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
                HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.PublishPost), args: p);
                return ResultData(p.Mapper<PostDto>(), message: $"文章于{timespan.Value:yyyy-MM-dd HH:mm:ss}将会自动发表！");
            }

            PostService.AddEntity(p);
            bool b = await SearchEngine.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "文章发表失败！");
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
                case PostLimitMode.AllowRegion:
                case PostLimitMode.ForbidRegion:
                    if (string.IsNullOrEmpty(post.Regions))
                    {
                        resultData = ResultData(null, false, "请输入限制的地区");
                        return false;
                    }

                    break;
                case PostLimitMode.AllowRegionExceptForbidRegion:
                case PostLimitMode.ForbidRegionExceptAllowRegion:
                    if (string.IsNullOrEmpty(post.ExceptRegions))
                    {
                        resultData = ResultData(null, false, "请输入排除的地区");
                        return false;
                    }

                    goto case PostLimitMode.AllowRegion;
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
            bool b = await PostHistoryVersionService.DeleteByIdSavedAsync(id) > 0;
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
            await PostHistoryVersionService.DeleteByIdSavedAsync(id);
            return ResultData(null, b, b ? "回滚成功" : "回滚失败");
        }

        /// <summary>
        /// 禁用或开启文章评论
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        [MyAuthorize]
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
        public async Task<ActionResult> DisableCopy(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.DisableCopy = !post.DisableCopy;
            return ResultData(null, await PostService.SaveChangesAsync() > 0, post.DisableCopy ? $"已开启【{post.Title}】这篇文章的防复制功能！" : $"已关闭【{post.Title}】这篇文章的防复制功能！");
        }

        /// <summary>
        /// 刷新文章
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Refresh(int id)
        {
            var post = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            post.ModifyDate = DateTime.Now;
            await PostService.SaveChangesAsync();
            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// 标记为恶意修改
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        [HttpPost("post/block/{id}")]
        public async Task<ActionResult> Block(int id)
        {
            var merge = await PostService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            merge.Status = Status.Forbidden;
            var b = await PostService.SaveChangesAsync() > 0;
            return b ? ResultData(null, true, "操作成功！") : ResultData(null, false, "操作失败！");
        }
        #endregion
    }
}