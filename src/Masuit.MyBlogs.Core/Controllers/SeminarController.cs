using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;

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

        public ISeminarPostService SeminarPostService { get; set; }

        /// <summary>
        /// 专题页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("c/{id:int}/{page:int?}/{size:int?}"), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "id", "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public ActionResult Index(int id, [Optional]OrderBy? orderBy, int page = 1, int size = 15)
        {
            var s = SeminarService.GetById(id) ?? throw new NotFoundException("文章未找到");
            var temp = PostService.GetQuery<PostOutputDto>(p => p.Seminar.Any(x => x.SeminarId == id) && (p.Status == Status.Pended || CurrentUser.IsAdmin));
            var posts = temp.OrderBy($"{nameof(Post.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").Skip(size * (page - 1)).Take(size).ToList();
            ViewBag.Total = temp.Count();
            ViewBag.Title = s.Title;
            ViewBag.Desc = s.Description;
            ViewBag.SubTitle = s.SubTitle;
            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.PostList);
            return View(posts);
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

            var entry = SeminarService.GetById(seminar.Id);
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
                b = SeminarService.SaveChanges() > 0;
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
            var list = SeminarService.GetPages<int, SeminarOutputDto>(page, size, out int total, s => true, s => s.Id, false).ToList();
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
            var list = SeminarService.GetAll<string, SeminarOutputDto>(s => s.Title).ToList();
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
            bool b = SeminarService.SaveChanges() > 0;
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
            bool b = SeminarPostService.DeleteEntitySaved(s => s.SeminarId == id && s.PostId == pid) > 0;
            return ResultData(null, b, b ? $"已成功将【{post.Title}】从专题【{seminar.Title}】移除" : "添加失败！");
        }

        #endregion
    }
}