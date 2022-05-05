using AutoMapper;
using Hangfire;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Infrastructure.Repository;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Command;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.ModelBinder;
using Masuit.Tools.Core.Net;
using Masuit.Tools.Linq;
using Masuit.Tools.Strings;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Controllers
{
    [Route("merge/")]
    public class MergeController : AdminController
    {
        public IPostMergeRequestService PostMergeRequestService { get; set; }

        public IWebHostEnvironment HostEnvironment { get; set; }

        public MapperConfiguration MapperConfig { get; set; }

        /// <summary>
        /// 获取合并详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var p = Mapper.Map<PostMergeRequestDto>(await PostMergeRequestService.GetByIdAsync(id));
            if (p != null)
            {
                p.SubmitTime = p.SubmitTime.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return ResultData(p);
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

            var list = PostMergeRequestService.GetQuery(where).OrderByDescending(d => d.MergeState == MergeStatus.Pending).ThenByDescending(r => r.Id).ToPagedList<PostMergeRequest, PostMergeRequestDtoBase>(page, size, MapperConfig);
            foreach (var item in list.Data)
            {
                item.SubmitTime = item.SubmitTime.ToTimeZone(HttpContext.Session.Get<string>(SessionKey.TimeZone));
            }

            return Ok(list);
        }

        /// <summary>
        /// 版本对比
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        [HttpGet("compare/{mid}")]
        public async Task<IActionResult> MergeCompare(int mid)
        {
            var newer = await PostMergeRequestService.GetByIdAsync(mid) ?? throw new NotFoundException("待合并文章未找到");
            var old = newer.Post;
            var diffHelper = new HtmlDiff.HtmlDiff(old.Content, newer.Content);
            string diffOutput = diffHelper.Build();
            old.Content = Regex.Replace(Regex.Replace(diffOutput, "<ins.+?</ins>", string.Empty), @"<\w+></\w+>", string.Empty);
            newer.Content = Regex.Replace(Regex.Replace(diffOutput, "<del.+?</del>", string.Empty), @"<\w+></\w+>", string.Empty);
            return ResultData(new { old = old.Mapper<PostMergeRequestDto>(), newer = newer.Mapper<PostMergeRequestDto>() });
        }

        /// <summary>
        /// 直接合并
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}")]
        public async Task<IActionResult> Merge(int id)
        {
            var merge = await PostMergeRequestService.GetByIdAsync(id) ?? throw new NotFoundException("待合并文章未找到");
            var history = merge.Post.Mapper<PostHistoryVersion>();
            history.Id = 0;
            merge.Post = Mapper.Map(merge, merge.Post);
            merge.Post.PostHistoryVersion.Add(history);
            merge.Post.ModifyDate = DateTime.Now;
            merge.MergeState = MergeStatus.Merged;
            var b = await PostMergeRequestService.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "文章合并失败！");
            }

            string link = Request.Scheme + "://" + Request.Host + "/" + merge.Post.Id;
            string content = new Template(await new FileInfo(HostEnvironment.WebRootPath + "/template/merge-pass.html").ShareReadWrite().ReadAllTextAsync(Encoding.UTF8)).Set("link", link).Set("title", merge.Post.Title).Render();
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客你提交的修改已通过", content, merge.ModifierEmail, "127.0.0.1"));
            return ResultData(null, true, "文章合并完成！");
        }

        /// <summary>
        /// 编辑并合并
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Merge([FromBodyOrDefault] PostMergeRequestCommandBase dto)
        {
            var merge = await PostMergeRequestService.GetByIdAsync(dto.Id) ?? throw new NotFoundException("待合并文章未找到");
            Mapper.Map(dto, merge);
            var b = await PostMergeRequestService.SaveChangesAsync() > 0;
            return b ? await Merge(merge.Id) : ResultData(null, false, "文章合并失败！");
        }

        /// <summary>
        /// 拒绝合并
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        [HttpPost("reject/{id}")]
        public async Task<ActionResult> Reject(int id, [Required(ErrorMessage = "请填写拒绝理由"), FromBodyOrDefault] string reason)
        {
            var merge = await PostMergeRequestService.GetByIdAsync(id) ?? throw new NotFoundException("待合并文章未找到");
            merge.MergeState = MergeStatus.Reject;
            var b = await PostMergeRequestService.SaveChangesAsync() > 0;
            if (!b)
            {
                return ResultData(null, false, "操作失败！");
            }

            var link = Request.Scheme + "://" + Request.Host + "/" + merge.Post.Id + "/merge/" + id;
            var content = new Template(await new FileInfo(HostEnvironment.WebRootPath + "/template/merge-reject.html").ShareReadWrite().ReadAllTextAsync(Encoding.UTF8)).Set("link", link).Set("title", merge.Post.Title).Set("reason", reason).Render();
            BackgroundJob.Enqueue(() => CommonHelper.SendMail(CommonHelper.SystemSettings["Title"] + "博客你提交的修改已被拒绝", content, merge.ModifierEmail, "127.0.0.1"));
            return ResultData(null, true, "合并已拒绝！");
        }

        /// <summary>
        /// 标记为恶意修改
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("block/{id}")]
        public async Task<ActionResult> Block(int id)
        {
            var merge = await PostMergeRequestService.GetByIdAsync(id) ?? throw new NotFoundException("待合并文章未找到");
            merge.MergeState = MergeStatus.Block;
            var b = await PostMergeRequestService.SaveChangesAsync() > 0;
            return b ? ResultData(null, true, "操作成功！") : ResultData(null, false, "操作失败！");
        }
    }
}
