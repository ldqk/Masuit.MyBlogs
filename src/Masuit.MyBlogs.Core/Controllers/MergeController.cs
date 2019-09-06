using AutoMapper;
using AutoMapper.QueryableExtensions;
using Hangfire;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Controllers
{
    [Route("merge/")]
    public class MergeController : AdminController
    {
        public IPostMergeRequestService PostMergeRequestService { get; set; }
        public IHostingEnvironment HostingEnvironment { get; set; }
        public MapperConfiguration MapperConfig { get; set; }

        /// <summary>
        /// 获取合并详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            return ResultData(Mapper.Map<PostMergeRequestOutputDto>(PostMergeRequestService.GetById(id)));
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="kw"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GetPageData(int page = 1, int size = 10, string kw = "")
        {
            Expression<Func<PostMergeRequest, bool>> where = r => true;
            if (!string.IsNullOrEmpty(kw))
            {
                where = where.And(r => r.Title.Contains(kw) || r.Content.Contains(kw) || r.Modifier.Contains(kw) || r.ModifierEmail.Contains(kw));
            }

            var list = PostMergeRequestService.LoadEntities(where).OrderBy(d => d.MergeState).ThenByDescending(r => r.Id).Skip((page - 1) * size).Take(size).ProjectTo<PostMergeRequestOutputDtoBase>(MapperConfig).ToList();
            var count = PostMergeRequestService.Count(where);
            var pageCount = Math.Ceiling(count * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, count);
        }

        /// <summary>
        /// 版本对比
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        [HttpGet("compare/{mid}")]
        public IActionResult MergeCompare(int mid)
        {
            var newer = PostMergeRequestService.GetById(mid) ?? throw new NotFoundException("待合并文章未找到");
            var old = newer.Post;
            HtmlDiff.HtmlDiff diffHelper = new HtmlDiff.HtmlDiff(old.Content, newer.Content);
            string diffOutput = diffHelper.Build();
            old.Content = Regex.Replace(Regex.Replace(diffOutput, "<ins.+?</ins>", string.Empty), @"<\w+></\w+>", string.Empty);
            newer.Content = Regex.Replace(Regex.Replace(diffOutput, "<del.+?</del>", string.Empty), @"<\w+></\w+>", string.Empty);
            return ResultData(new { old = old.Mapper<PostMergeRequestOutputDto>(), newer = newer.Mapper<PostMergeRequestOutputDto>() });
        }

        /// <summary>
        /// 直接合并
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public IActionResult Merge(int id)
        {
            var merge = PostMergeRequestService.GetById(id) ?? throw new NotFoundException("待合并文章未找到");
            var history = merge.Post.Mapper<PostHistoryVersion>();
            history.Id = 0;
            merge.Post = Mapper.Map(merge, merge.Post);
            merge.Post.PostHistoryVersion.Add(history);
            merge.Post.ModifyDate = DateTime.Now;
            merge.MergeState = MergeStatus.Merged;
            var b = PostMergeRequestService.UpdateEntitySaved(merge);
            if (b)
            {
                string link = Request.Scheme + "://" + Request.Host + "/" + merge.Post.Id;
                string content = System.IO.File.ReadAllText(HostingEnvironment.WebRootPath + "/template/merge-pass.html").Replace("{{link}}", link).Replace("{{title}}", merge.Post.Title);
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客你提交的修改已通过", content, merge.ModifierEmail));
                return ResultData(null, true, "文章合并完成！");
            }

            return ResultData(null, false, "文章合并失败！");
        }

        /// <summary>
        /// 编辑并合并
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Merge([FromForm]PostMergeRequestInputDtoBase dto)
        {
            var merge = PostMergeRequestService.GetById(dto.Id) ?? throw new NotFoundException("待合并文章未找到");
            Mapper.Map(dto, merge);
            var b = PostMergeRequestService.UpdateEntitySaved(merge);
            if (b)
            {
                return Merge(merge.Id);
            }

            return ResultData(null, false, "文章合并失败！");
        }

        /// <summary>
        /// 拒绝合并
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        [HttpPost("reject/{id}")]
        public ActionResult Reject(int id, string reason)
        {
            var merge = PostMergeRequestService.GetById(id) ?? throw new NotFoundException("待合并文章未找到");
            merge.MergeState = MergeStatus.Reject;
            var b = PostMergeRequestService.UpdateEntitySaved(merge);
            if (b)
            {
                string link = Request.Scheme + "://" + Request.Host + "/" + merge.Post.Id + "/merge/" + id;
                string content = System.IO.File.ReadAllText(HostingEnvironment.WebRootPath + "/template/merge-reject.html").Replace("{{link}}", link).Replace("{{title}}", merge.Post.Title).Replace("{{reason}}", reason);
                BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客你提交的修改已被拒绝", content, merge.ModifierEmail));
                return ResultData(null, true, "合并已拒绝！");
            }

            return ResultData(null, false, "操作失败！");
        }
    }
}