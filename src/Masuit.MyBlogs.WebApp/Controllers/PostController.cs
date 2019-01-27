using AutoMapper;
using Common;
using Hangfire;
using IBLL;
using Masuit.MyBlogs.WebApp.Models;
using Masuit.MyBlogs.WebApp.Models.Hangfire;
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Html;
using Masuit.Tools.Net;
using Masuit.Tools.Security;
using Masuit.Tools.Systems;
using Models.DTO;
using Models.Entity;
using Models.Enum;
using Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using static Common.CommonHelper;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class PostController : BaseController
    {
        private IPostBll PostBll { get; set; }
        private ICategoryBll CategoryBll { get; set; }
        private IBroadcastBll BroadcastBll { get; set; }
        private ISeminarBll SeminarBll { get; set; }
        private IPostAccessRecordBll PostAccessRecordBll { get; set; }
        public ICommentBll CommentBll { get; set; }
        public IPostHistoryVersionBll PostHistoryVersionBll { get; set; }

        public PostController(IPostBll postBll, ICategoryBll categoryBll, IBroadcastBll broadcastBll, ISeminarBll seminarBll, IPostAccessRecordBll postAccessRecordBll, ICommentBll commentBll, IPostHistoryVersionBll postHistoryVersionBll)
        {
            PostBll = postBll;
            CategoryBll = categoryBll;
            BroadcastBll = broadcastBll;
            SeminarBll = seminarBll;
            PostAccessRecordBll = postAccessRecordBll;
            CommentBll = commentBll;
            PostHistoryVersionBll = postHistoryVersionBll;
        }

        [Route("{id:int}/{kw}"), Route("{id:int}")]
        public ActionResult Details(int id, string kw)
        {
            Post post = PostBll.GetById(id);
            if (post != null)
            {
                ViewBag.Keyword = post.Keyword + "," + post.Label;
                UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
                DateTime modifyDate = post.ModifyDate;
                ViewBag.Next = PostBll.GetFirstEntityFromL2CacheNoTracking(p => p.ModifyDate > modifyDate && (p.Status == Status.Pended || user.IsAdmin), p => p.ModifyDate);
                ViewBag.Prev = PostBll.GetFirstEntityFromL2CacheNoTracking(p => p.ModifyDate < modifyDate && (p.Status == Status.Pended || user.IsAdmin), p => p.ModifyDate, false);
                if (user.IsAdmin)
                {
                    return View("Details_Admin", post);
                }

                if (post.Status != Status.Pended)
                {
                    return RedirectToAction("Post", "Home");
                }

                if (string.IsNullOrEmpty(Session.GetByRedis<string>("post" + id)))
                {
                    HangfireHelper.CreateJob(typeof(IHangfireBackJob), nameof(HangfireBackJob.RecordPostVisit), args: id);
                    Session.SetByRedis("post" + id, id.ToString());
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
        [Route("{id:int}/history"), Route("{id:int}/history/{page:int}/{size:int}")]
        public ActionResult History(int id, int page = 1, int size = 20)
        {
            var p = PostBll.GetById(id).Mapper<PostOutputDto>();
            if (p != null)
            {
                ViewBag.Primary = p;
                var list = PostHistoryVersionBll.LoadPageEntitiesFromCacheNoTracking(page, size, out int total, v => v.PostId == id, v => v.ModifyDate, false).Select(v => new PostHistoryVersion()
                {
                    PostId = id,
                    Category = v.Category,
                    ModifyDate = v.ModifyDate,
                    Title = v.Title,
                    Id = v.Id,
                    CategoryId = v.CategoryId
                }).ToList();
                ViewBag.Total = total;
                ViewBag.PageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
                return View(list);
            }

            return Redirect(Request.UrlReferrer?.ToString() ?? "/error");
        }

        /// <summary>
        /// 文章历史版本
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hid"></param>
        /// <returns></returns>
        [Route("{id:int}/history/{hid:int}")]
        public ActionResult HistoryVersion(int id, int hid)
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var post = PostHistoryVersionBll.GetById(hid);
            if (post is null)
            {
                return RedirectToAction("History", new
                {
                    id
                });
            }

            ViewBag.Next = PostHistoryVersionBll.GetFirstEntityFromL2CacheNoTracking(p => p.PostId == id && p.ModifyDate > post.ModifyDate, p => p.ModifyDate);
            ViewBag.Prev = PostHistoryVersionBll.GetFirstEntityFromL2CacheNoTracking(p => p.PostId == id && p.ModifyDate < post.ModifyDate, p => p.ModifyDate, false);
            if (user.IsAdmin)
            {
                return View("HistoryVersion_Admin", post);
            }

            return View(post);
        }

        [Route("{id:int}/history/{v1:int}-{v2:int}")]
        public ActionResult CompareVersion(int id, int v1, int v2)
        {
            var main = PostBll.GetById(id).Mapper<PostHistoryVersion>();
            var left = v1 <= 0 ? main : PostHistoryVersionBll.GetById(v1);
            var right = v2 <= 0 ? main : PostHistoryVersionBll.GetById(v2);
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

        public ActionResult VoteDown(int id)
        {
            Post post = PostBll.GetById(id);
            if (Session["post-vote" + id] != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            if (post != null)
            {
                Session["post-vote" + id] = id;
                ++post.VoteDownCount;
                PostBll.UpdateEntity(post);
                bool b = PostBll.SaveChanges() > 0;
                return ResultData(null, b, b ? "投票成功！" : "投票失败！");
            }

            return ResultData(null, false, "非法操作");
        }

        public ActionResult VoteUp(int id)
        {
            Post post = PostBll.GetById(id);
            if (Session["post-vote" + id] != null)
            {
                return ResultData(null, false, "您刚才已经投过票了，感谢您的参与！");
            }

            if (post != null)
            {
                Session["post-vote" + id] = id;
                ++post.VoteUpCount;
                bool b = PostBll.UpdateEntitySaved(post);
                return ResultData(null, b, b ? "投票成功！" : "投票失败！");
            }

            return ResultData(null, false, "非法操作");
        }

        public ActionResult Publish()
        {
            List<string> list = PostBll.GetAll().Select(p => p.Label).ToList();
            List<string> result = new List<string>();
            list.ForEach(s =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    result.AddRange(s.Split(',', '，'));
                }
            });
            ViewBag.Category = CategoryBll.LoadEntitiesNoTracking(c => c.Status == Status.Available).ToList();
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            if (user != null)
            {
                return View("Publish_Admin", result.Distinct().OrderBy(s => s));
            }

            return View(result.Distinct().OrderBy(s => s));
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public ActionResult Publish(PostInputDto post)
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo);
            if (!CategoryBll.Any(c => c.Id == post.CategoryId && c.Status == Status.Available))
            {
                return ResultData(null, message: "请选择一个分类");
            }

            if (string.IsNullOrEmpty(post.Label?.Trim()))
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

            post.Status = Status.Pending;
            post.PostDate = DateTime.Now;
            post.ModifyDate = DateTime.Now;
            if (user != null && user.IsAdmin)
            {
                post.Status = Status.Pended;
            }
            else
            {
                post.Content = ReplaceImgSrc(Regex.Replace(post.Content.HtmlSantinizerStandard(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            }

            ViewBag.CategoryId = new SelectList(CategoryBll.LoadEntitiesNoTracking(c => c.Status == Status.Available), "Id", "Name", post.CategoryId);
            Post p = post.Mapper<Post>();
            p.PostAccessRecord.Add(new PostAccessRecord()
            {
                AccessTime = DateTime.Today,
                ClickCount = 0
            });
            p = PostBll.AddEntitySaved(p);
            if (p != null)
            {
                if (p.Status == Status.Pending)
                {
                    var email = GetSettings("ReceiveEmail");
                    string link = Url.Action("Details", "Post", new
                    {
                        id = p.Id
                    }, Request.Url?.Scheme ?? "http");
                    string content = System.IO.File.ReadAllText(Request.MapPath("/template/publish.html")).Replace("{{link}}", link).Replace("{{time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{title}}", p.Title);
                    BackgroundJob.Enqueue(() => SendMail(GetSettings("Title") + "有访客投稿：", content, email));
                    return ResultData(p.Mapper<PostOutputDto>(), message: "文章发表成功，待站长审核通过以后将显示到列表中！");
                }

                return ResultData(p.Mapper<PostOutputDto>(), message: "文章发表成功！");
            }

            return ResultData(null, false, "文章发表失败！");
        }

        public ActionResult GetTag()
        {
            List<string> list = PostBll.GetAll().Select(p => p.Label).ToList();
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

        [Route("all")]
        public ActionResult All()
        {
            UserInfoOutputDto user = Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            List<string> tags = PostBll.GetAll().Select(p => p.Label).ToList(); //tag
            List<string> result = new List<string>();
            tags.ForEach(s =>
            {
                if (!string.IsNullOrEmpty(s))
                {
                    result.AddRange(s.Split(',', '，'));
                }
            });
            ViewBag.tags = result.GroupBy(t => t).OrderByDescending(g => g.Count()).ThenBy(g => g.Key);
            ViewBag.cats = CategoryBll.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel()
            {
                Id = c.Id,
                Name = c.Name,
                Count = c.Post.Count(p => p.Status == Status.Pended || user.IsAdmin)
            }).ToList(); //category
            ViewBag.seminars = SeminarBll.GetAll(c => c.Post.Count, false).Select(c => new TagCloudViewModel
            {
                Id = c.Id,
                Name = c.Title,
                Count = c.Post.Count(p => p.Status == Status.Pended || user.IsAdmin)
            }).ToList(); //seminars
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CheckViewToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return ResultData(null, false, "请输入文章访问密码！");
            }

            var s = RedisHelper.GetString("ArticleViewToken");
            if (token.Equals(s))
            {
                Session.SetByRedis("ArticleViewToken", token);
                return ResultData(null);
            }

            return ResultData(null, false, "文章访问密码不正确！");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult GetViewToken(string email)
        {
            if (string.IsNullOrEmpty(email) && !email.MatchEmail())
            {
                return ResultData(null, false, "请输入正确的邮箱！");
            }

            if (BroadcastBll.Any(b => b.Email.Equals(email) && b.SubscribeType == SubscribeType.ArticleToken))
            {
                var s = RedisHelper.GetString("ArticleViewToken");
                Session.SetByRedis("ArticleViewToken", s);
                return ResultData(null);
            }

            return ResultData(null, false, "您目前没有权限访问这篇文章的加密部分，请联系站长开通这篇文章的访问权限！");
        }

        #region 后端管理

        [Authority]
        public ActionResult Fixtop(int id)
        {
            Post post = PostBll.GetById(id);
            post.IsFixedTop = !post.IsFixedTop;
            bool b = PostBll.UpdateEntitySaved(post);
            if (b)
            {
                return ResultData(null, true, post.IsFixedTop ? "置顶成功！" : "取消置顶成功！");
            }

            return ResultData(null, false, "操作失败！");
        }

        [Authority]
        public ActionResult Pass(int id)
        {
            Post post = PostBll.GetById(id);
            post.Status = Status.Pended;
            post.ModifyDate = DateTime.Now;
            post.PostDate = DateTime.Now;
            bool b = PostBll.UpdateEntitySaved(post);
            if ("false" == GetSettings("DisabledEmailBroadcast"))
            {
                var cast = BroadcastBll.LoadEntities(c => c.Status == Status.Subscribed).ToList();
                string link = Request.Url?.Scheme + "://" + Request.Url?.Authority + "/" + id;
                cast.ForEach(c =>
                {
                    var ts = DateTime.Now.GetTotalMilliseconds();
                    string content = System.IO.File.ReadAllText(Request.MapPath("/template/broadcast.html")).Replace("{{link}}", link + "?email=" + c.Email).Replace("{{time}}", post.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{title}}", post.Title).Replace("{{author}}", post.Author).Replace("{{content}}", post.Content.RemoveHtmlTag(150)).Replace("{{cancel}}", Url.Action("Subscribe", "Subscribe", new
                    {
                        c.Email,
                        act = "cancel",
                        validate = c.ValidateCode,
                        timespan = ts,
                        hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(ConfigurationManager.AppSettings["BaiduAK"])
                    }, Request.Url.Scheme));
                    BackgroundJob.Enqueue(() => SendMail(GetSettings("Title") + "博客有新文章发布了", content, c.Email));
                });
            }

            return ResultData(null, b, b ? "审核通过！" : "审核失败！");
        }

        [Authority]
        public ActionResult Delete(int id)
        {
            var post = PostBll.GetById(id);
            post.Status = Status.Deleted;
            bool b = PostBll.UpdateEntitySaved(post);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        [Authority]
        public ActionResult Restore(int id)
        {
            var post = PostBll.GetById(id);
            post.Status = Status.Pended;
            bool b = PostBll.UpdateEntitySaved(post);
            return ResultData(null, b, b ? "恢复成功！" : "恢复失败！");
        }

        [Authority]
        public ActionResult Truncate(int id)
        {
            var post = PostBll.GetById(id);
            if (post is null)
            {
                return ResultData(null, false, "文章已经被删除！");
            }

            if (post.IsWordDocument)
            {
                try
                {
                    System.IO.File.Delete(Path.Combine(Server.MapPath("/upload"), post.ResourceName));
                    Directory.Delete(Path.Combine(Server.MapPath("/upload"), Path.GetFileNameWithoutExtension(post.ResourceName)), true);
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
                        System.IO.File.Delete(Path.Combine(Server.MapPath("/"), path));
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            bool b = PostBll.DeleteByIdSaved(id);
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
            Post post = PostBll.GetById(id);
            PostOutputDto model = post.Mapper<PostOutputDto>();
            model.Seminars = string.Join(",", post.Seminar.Select(s => s.Title));
            return ResultData(model);
        }

        [Authority]
        public ActionResult Read(int id) => ResultData(PostBll.GetById(id).Mapper<PostOutputDto>());

        /// <summary>
        /// 获取所有文章
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAllData()
        {
            var plist = PostBll.LoadEntitiesNoTracking(p => p.Status != Status.Deleted).OrderBy(p => p.Status).ThenByDescending(p => p.IsFixedTop).ThenByDescending(p => p.ModifyDate).Select(p => new
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
                ViewCount = p.PostAccessRecord.Sum(r => r.ClickCount),
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
            var query = string.IsNullOrEmpty(kw) ? PostBll.GetAllNoTracking() : PostBll.LoadEntitiesNoTracking(p => p.Title.Contains(kw) || p.Author.Contains(kw) || p.Email.Contains(kw) || p.Label.Contains(kw) || p.Content.Contains(kw));
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
                    temp = order.ThenByDescending(p => p.PostAccessRecord.Any() ? p.PostAccessRecord.Sum(r => r.ClickCount) : 1);
                    break;
                case OrderBy.VoteCount:
                    temp = order.ThenByDescending(p => p.VoteUpCount);
                    break;
                case OrderBy.AverageViewCount:
                    temp = order.ThenByDescending(p => p.PostAccessRecord.Average(r => r.ClickCount));
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
                ViewCount = p.PostAccessRecord.Any() ? p.PostAccessRecord.Sum(r => r.ClickCount) : 1,
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

        [Authority]
        public ActionResult GetPending(int page = 1, int size = 10, string search = "")
        {
            int total;
            IQueryable<Post> temp;
            if (string.IsNullOrEmpty(search))
            {
                temp = PostBll.LoadPageEntitiesNoTracking(page, size, out total, p => p.Status == Status.Pending, p => p.Id);
            }
            else
            {
                temp = PostBll.LoadPageEntitiesNoTracking(page, size, out total, p => p.Status == Status.Pending && (p.Title.Contains(search) || p.Author.Contains(search) || p.Email.Contains(search) || p.Label.Contains(search)), p => p.Id);
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
                ViewCount = p.PostAccessRecord.Any() ? p.PostAccessRecord.Sum(r => r.ClickCount) : 1,
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

        [HttpPost, Authority, ValidateInput(false)]
        public ActionResult Edit(PostInputDto post, bool notify = true, bool reserve = true)
        {
            post.Content = ReplaceImgSrc(Regex.Replace(post.Content.Trim(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/");
            if (!CategoryBll.Any(c => c.Id == post.CategoryId && c.Status == Status.Available))
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

            Post p = PostBll.GetById(post.Id);
            if (reserve)
            {
                post.ModifyDate = DateTime.Now;
                var history = p.Mapper<PostHistoryVersion>();
                p.PostHistoryVersion.Add(history);
            }

            Mapper.Map(post, p);
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct();
                p.Seminar.Clear();
                tmp.ForEach(s =>
                {
                    p.Seminar.Add(SeminarBll.GetFirstEntity(e => e.Title.Equals(s)));
                });
            }

            bool b = PostBll.UpdateEntitySaved(p);
            if (b)
            {
#if !DEBUG
                if (notify && "false" == GetSettings("DisabledEmailBroadcast"))
                {
                    var cast = BroadcastBll.LoadEntities(c => c.Status == Status.Subscribed).ToList();
                    string link = Request.Url?.Scheme + "://" + Request.Url?.Authority + "/" + p.Id;
                    cast.ForEach(c =>
                    {
                        var ts = DateTime.Now.GetTotalMilliseconds();
                        string content = System.IO.File.ReadAllText(Request.MapPath("/template/broadcast.html"))
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
                                hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(ConfigurationManager.AppSettings["BaiduAK"])
                            }, Request.Url.Scheme));
                        BackgroundJob.Schedule(() => SendMail(GetSettings("Title") + "博客有新文章发布了", content, c.Email), (p.ModifyDate - DateTime.Now));
                    });
                }
#endif
                return ResultData(p.Mapper<PostOutputDto>(), message: "文章修改成功！");
            }

            return ResultData(null, false, "文章修改失败！");
        }

        [Authority, ValidateInput(false), HttpPost]
        public ActionResult Write(PostInputDto post, DateTime? timespan, bool schedule = false)
        {
            post.Content = ReplaceImgSrc(Regex.Replace(post.Content.Trim(), @"<img\s+[^>]*\s*src\s*=\s*['""]?(\S+\.\w{3,4})['""]?[^/>]*/>", "<img src=\"$1\"/>")).Replace("/thumb150/", "/large/"); //提取img标签，提取src属性并重新创建个只包含src属性的img标签
            if (!CategoryBll.Any(c => c.Id == post.CategoryId && c.Status == Status.Available))
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
            if (!string.IsNullOrEmpty(post.Seminars))
            {
                var tmp = post.Seminars.Split(',').Distinct();
                tmp.ForEach(s =>
                {
                    var id = s.ToInt32();
                    p.Seminar.Add(SeminarBll.GetById(id));
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

            p = PostBll.AddEntitySaved(p);
            if (p != null)
            {
                if ("false" == GetSettings("DisabledEmailBroadcast"))
                {
                    var cast = BroadcastBll.LoadEntities(c => c.Status == Status.Subscribed).ToList();
                    string link = Request.Url?.Scheme + "://" + Request.Url?.Authority + "/" + p.Id;
                    cast.ForEach(c =>
                    {
                        var ts = DateTime.Now.GetTotalMilliseconds();
                        string content = System.IO.File.ReadAllText(Request.MapPath("/template/broadcast.html")).Replace("{{link}}", link + "?email=" + c.Email).Replace("{{time}}", post.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss")).Replace("{{title}}", post.Title).Replace("{{author}}", post.Author).Replace("{{content}}", post.Content.RemoveHtmlTag(150)).Replace("{{cancel}}", Url.Action("Subscribe", "Subscribe", new
                        {
                            c.Email,
                            act = "cancel",
                            validate = c.ValidateCode,
                            timespan = ts,
                            hash = (c.Email + "cancel" + c.ValidateCode + ts).AESEncrypt(ConfigurationManager.AppSettings["BaiduAK"])
                        }, Request.Url.Scheme));
                        BackgroundJob.Schedule(() => SendMail(GetSettings("Title") + "博客有新文章发布了", content, c.Email), (p.ModifyDate - DateTime.Now));
                    });
                }

                return ResultData(null, true, "文章发表成功！");
            }

            return ResultData(null, false, "文章发表失败！");
        }

        /// <summary>
        /// 获取头图文章
        /// </summary>
        /// <returns></returns>
        public ActionResult GetTops()
        {
            var list = PostBll.LoadEntitiesNoTracking<DateTime, PostOutputDto>(p => p.Status == Status.Pended && p.IsBanner, p => p.ModifyDate, false).Select(p => new
            {
                p.Id,
                p.Description,
                p.Title,
                p.ImageUrl
            }).ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 获取非头图页文章
        /// </summary>
        /// <returns></returns>
        public ActionResult GetNotTops()
        {
            var list = PostBll.LoadEntitiesNoTracking(p => p.Status == Status.Pended && !p.IsBanner).GroupBy(p => p.Category.Name).Select(g => new
            {
                text = g.Key,
                children = g.OrderBy(p => p.Title).Select(p => new
                {
                    id = p.Id,
                    text = p.Title
                })
            }).ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 添加头图页
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [HttpPost, ValidateInput(false), Authority]
        public ActionResult AddTop(PostOutputDto p)
        {
            Post post = PostBll.GetById(p.Id);
            post.IsBanner = true;
            post.Description = p.Description;
            post.ImageUrl = p.ImageUrl;
            bool b = PostBll.UpdateEntitySaved(post);
            return ResultData(null, b, b ? "添加头图页成功" : "添加头图页失败！");
        }

        /// <summary>
        /// 取消头图页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority, HttpPost]
        public ActionResult RemoveTop(int id)
        {
            Post post = PostBll.GetById(id);
            post.IsBanner = false;
            bool b = PostBll.UpdateEntitySaved(post);
            return ResultData(null, b, b ? "取消头图页成功" : "取消头图页失败！");
        }

        [Authority]
        public ActionResult AddSeminar(int id, int sid)
        {
            var post = PostBll.GetById(id);
            Seminar seminar = SeminarBll.GetById(sid);
            post.Seminar.Add(seminar);
            bool b = PostBll.UpdateEntitySaved(post);
            return ResultData(null, b, b ? $"已将文章【{post.Title}】添加到专题【{seminar.Title}】" : "添加失败");
        }

        [Authority]
        public ActionResult RemoveSeminar(int id, int sid)
        {
            var post = PostBll.GetById(id);
            Seminar seminar = SeminarBll.GetById(sid);
            post.Seminar.Remove(seminar);
            bool b = PostBll.UpdateEntitySaved(post);
            return ResultData(null, b, b ? $"已将文章【{post.Title}】从【{seminar.Title}】专题移除" : "添加失败");
        }

        [Authority]
        public ActionResult DeleteHistory(int id)
        {
            bool b = PostHistoryVersionBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "历史版本文章删除成功！" : "历史版本文章删除失败！");
        }

        [Authority, HttpPost]
        public ActionResult ViewToken()
        {
            if (!RedisHelper.KeyExists("ArticleViewToken"))
            {
                RedisHelper.SetString("ArticleViewToken", string.Empty.CreateShortToken(6));
            }

            var token = RedisHelper.GetString("ArticleViewToken");
            return ResultData(token);
        }

        public ActionResult Revert(int id)
        {
            var history = PostHistoryVersionBll.GetById(id);
            if (history != null)
            {
                PostHistoryVersionBll.AddEntity(history.Post.Mapper<PostHistoryVersion>());
                history.Post.Category = history.Category;
                history.Post.CategoryId = history.CategoryId;
                history.Post.Content = history.Content;
                history.Post.Title = history.Title;
                history.Post.Label = history.Label;
                history.Post.Seminar = history.Seminar;
                history.Post.ModifyDate = history.ModifyDate;
                bool b = PostHistoryVersionBll.UpdateEntitySaved(history);
                return ResultData(null, b, b ? "回滚成功" : "回滚失败");
            }

            return ResultData(null, false, "版本不存在");
        }

        [HttpPost]
        public ActionResult Analyse(int id)
        {
            var list = PostBll.GetById(id).PostAccessRecord.OrderBy(r => r.AccessTime).GroupBy(r => r.AccessTime.Date).Select(r => new[] { r.Key.GetTotalMilliseconds(), r.Sum(p => p.ClickCount) }).ToList();
            var high = list.OrderByDescending(n => n[1]).FirstOrDefault();
            var average = list.Average(d => d[1]);
            return ResultData(new { list, aver = average, high = high[1], highDate = DateTime.Parse("1970-01-01").AddMilliseconds(high[0]) });
        }
        #endregion
    }
}