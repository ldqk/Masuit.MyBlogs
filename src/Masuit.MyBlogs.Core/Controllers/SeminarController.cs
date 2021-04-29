using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Repository;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

        /// <summary>
        /// 专题页
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        [Route("special/{id:int}"), Route("c/{id:int}", Order = 1), ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "page", "size", "orderBy" }, VaryByHeader = "Cookie")]
        public async Task<ActionResult> Index(int id, [Optional] OrderBy? orderBy, [Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var s = await SeminarService.GetByIdAsync(id) ?? throw new NotFoundException("文章未找到");
            var posts = await PostService.GetQuery(p => p.Seminar.Any(x => x.Id == id) && p.Status == Status.Published).OrderBy($"{nameof(Post.IsFixedTop)} desc,{(orderBy ?? OrderBy.ModifyDate).GetDisplay()} desc").ToCachedPagedListAsync<Post, PostDto>(page, size, MapperConfig);
            ViewBag.Title = s.Title;
            ViewBag.Desc = s.Description;
            ViewBag.SubTitle = s.SubTitle;
            ViewBag.Ads = AdsService.GetByWeightedPrice(AdvertiseType.PostList);
            ViewData["page"] = new Pagination(page, size, posts.TotalCount, orderBy);
            return View(posts);
        }

        #region 管理端

        /// <summary>
        /// 保存专题
        /// </summary>
        /// <param name="seminar"></param>
        /// <returns></returns>
        [MyAuthorize]
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
        [MyAuthorize]
        public async Task<ActionResult> Delete(int id)
        {
            bool b = await SeminarService.DeleteByIdSavedAsync(id) > 0;
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 获取专题详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> Get(int id)
        {
            Seminar seminar = await SeminarService.GetByIdAsync(id);
            return ResultData(seminar.Mapper<SeminarDto>());
        }

        /// <summary>
        /// 专题分页列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页码必须大于0")] int page = 1, [Range(1, 50, ErrorMessage = "页大小必须在0到50之间")] int size = 15)
        {
            var list = SeminarService.GetPages<int, SeminarDto>(page, size, s => true, s => s.Id, false);
            return Ok(list);
        }

        /// <summary>
        /// 获取所有专题
        /// </summary>
        /// <returns></returns>
        [MyAuthorize]
        public ActionResult GetAll()
        {
            var list = SeminarService.GetAll<string, SeminarDto>(s => s.Title).ToList();
            return ResultData(list);
        }

        /// <summary>
        /// 给专题添加文章
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> AddPost(int id, int pid)
        {
            Seminar seminar = await SeminarService.GetByIdAsync(id);
            Post post = await PostService.GetByIdAsync(pid);
            seminar.Post.Add(post);
            bool b = await SeminarService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? $"已成功将【{post.Title}】添加到专题【{seminar.Title}】" : "添加失败！");
        }

        /// <summary>
        /// 移除文章
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        [MyAuthorize]
        public async Task<ActionResult> RemovePost(int id, int pid)
        {
            Seminar seminar = await SeminarService.GetByIdAsync(id);
            Post post = await PostService.GetByIdAsync(pid);
            //bool b = await seminarPostService.DeleteEntitySavedAsync(s => s.SeminarId == id && s.PostId == pid) > 0;
            seminar.Post.Remove(post);
            var b = await SeminarService.SaveChangesAsync() > 0;
            return ResultData(null, b, b ? $"已成功将【{post.Title}】从专题【{seminar.Title}】移除" : "添加失败！");
        }

        #endregion
    }
}