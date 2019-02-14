using Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 专题页
    /// </summary>
    public class SeminarController : BaseController
    {
        /// <summary>
        /// 专题
        /// </summary>
        public ISeminarService SeminarService { get; set; }

        /// <summary>
        /// 文章
        /// </summary>
        public IPostService PostService { get; set; }

        private readonly ISeminarPostService _seminarPostService;

        /// <summary>
        /// 专题页
        /// </summary>
        /// <param name="seminarService"></param>
        /// <param name="postService"></param>
        /// <param name="seminarPostService"></param>
        public SeminarController(ISeminarService seminarService, IPostService postService, ISeminarPostService seminarPostService)
        {
            SeminarService = seminarService;
            PostService = postService;
            _seminarPostService = seminarPostService;
        }

        /// <summary>
        /// 专题页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("c/{id:int}/{page:int?}/{size:int?}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "page", "size", "orderBy" }, VaryByHeader = HeaderNames.Cookie)]
        public ActionResult Index(int id, int page = 1, int size = 15, OrderBy orderBy = OrderBy.ModifyDate)
        {
            IList<Post> posts;
            UserInfoOutputDto user = HttpContext.Session.GetByRedis<UserInfoOutputDto>(SessionKey.UserInfo) ?? new UserInfoOutputDto();
            var s = SeminarService.GetById(id);
            if (s is null)
            {
                return RedirectToAction("Index", "Error");
            }
            var temp = PostService.LoadEntities(p => p.Seminar.Any(x => x.SeminarId == id) && (p.Status == Status.Pended || user.IsAdmin)).OrderByDescending(p => p.IsFixedTop);
            switch (orderBy)
            {
                case OrderBy.CommentCount:
                    posts = temp.ThenByDescending(p => p.Comment.Count).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.PostDate:
                    posts = temp.ThenByDescending(p => p.PostDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.ViewCount:
                    posts = temp.ThenByDescending(p => p.TotalViewCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                case OrderBy.VoteCount:
                    posts = temp.ThenByDescending(p => p.VoteUpCount).Skip(size * (page - 1)).Take(size).ToList();
                    break;
                default:
                    posts = temp.ThenByDescending(p => p.ModifyDate).Skip(size * (page - 1)).Take(size).ToList();
                    break;
            }
            ViewBag.Total = temp.Count();
            ViewBag.Title = s.Title;
            ViewBag.Desc = s.Description;
            ViewBag.SubTitle = s.SubTitle;
            return View(posts.Mapper<IList<PostOutputDto>>());
        }

        #region 管理端

        /// <summary>
        /// 保存专题
        /// </summary>
        /// <param name="seminar"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Save(Seminar seminar)
        {
            bool contain;
            if (seminar.Id > 0)
            {
                //更新
                contain = SeminarService.GetAll().Select(s => s.Title).Except(new List<string>()
                {
                    SeminarService.GetById(seminar.Id).Title
                }).Contains(seminar.Title);
            }
            else
            {
                //添加
                contain = SeminarService.GetAll().Select(s => s.Title).Contains(seminar.Title);
            }
            if (contain)
            {
                return ResultData(null, false, $"{seminar.Title} 已经存在了");
            }
            //var b = SeminarService.AddOrUpdateSaved(s => s.Id, seminar) > 0;
            Seminar entry = SeminarService.GetById(seminar.Id);
            bool b;
            if (entry is null)
            {
                b = SeminarService.AddEntitySaved(seminar) != null;
            }
            else
            {
                entry.Description = seminar.Description;
                entry.Title = seminar.Title;
                entry.SubTitle = seminar.SubTitle;
                b = SeminarService.UpdateEntitySaved(entry);
            }
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        /// <summary>
        /// 删除专题
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Delete(int id)
        {
            bool b = SeminarService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 获取专题详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult Get(int id)
        {
            Seminar seminar = SeminarService.GetById(id);
            return ResultData(seminar.Mapper<SeminarOutputDto>());
        }

        /// <summary>
        /// 专题分页列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPageData(int page, int size)
        {
            List<SeminarOutputDto> list = SeminarService.LoadPageEntities<int, SeminarOutputDto>(page, size, out int total, s => true, s => s.Id, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 获取所有专题
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult GetAll()
        {
            List<SeminarOutputDto> list = SeminarService.GetAll<string, SeminarOutputDto>(s => s.Title).ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 给专题添加文章
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult AddPost(int id, int pid)
        {
            Seminar seminar = SeminarService.GetById(id);
            Post post = PostService.GetById(pid);
            seminar.Post.Add(new SeminarPost()
            {
                Post = post,
                Seminar = seminar,
                PostId = post.Id,
                SeminarId = id
            });
            bool b = SeminarService.UpdateEntitySaved(seminar);
            return ResultData(null, b, b ? $"已成功将【{post.Title}】添加到专题【{seminar.Title}】" : "添加失败！");
        }

        /// <summary>
        /// 移除文章
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        [Authority]
        public ActionResult RemovePost(int id, int pid)
        {
            Seminar seminar = SeminarService.GetById(id);
            Post post = PostService.GetById(pid);
            bool b = _seminarPostService.DeleteEntitySaved(s => s.SeminarId == id && s.PostId == pid) > 0;
            return ResultData(null, b, b ? $"已成功将【{post.Title}】从专题【{seminar.Title}】移除" : "添加失败！");
        }

        #endregion
    }
}