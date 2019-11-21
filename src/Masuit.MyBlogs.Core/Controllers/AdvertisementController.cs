using AutoMapper.QueryableExtensions;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Controllers
{
    [Route("ads/[action]")]
    public class AdvertisementController : BaseController
    {
        /// <summary>
        /// 前往
        /// </summary>
        /// <param name="id">广告id</param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Redirect(int id)
        {
            var ad = AdsService.GetById(id) ?? throw new NotFoundException("广告不存在");
            if (!HttpContext.Request.IsRobot() && string.IsNullOrEmpty(HttpContext.Session.Get<string>("ads" + id)))
            {
                HttpContext.Session.Set("ads" + id, id.ToString());
                ad.ViewCount++;
                await AdsService.SaveChangesAsync();
            }

            return RedirectPermanent(ad.Url);
        }

        /// <summary>
        /// 获取文章分页
        /// </summary>
        /// <returns></returns>
        [Authority]
        public ActionResult GetPageData([Range(1, int.MaxValue, ErrorMessage = "页数必须大于0")]int page = 1, [Range(1, int.MaxValue, ErrorMessage = "页大小必须大于0")]int size = 10, string kw = "")
        {
            Expression<Func<Advertisement, bool>> where = p => true;
            if (!string.IsNullOrEmpty(kw))
            {
                where = where.And(p => p.Title.Contains(kw) || p.Description.Contains(kw));
            }

            var query = AdsService.GetQuery(where);
            var total = query.Count();
            var list = query.OrderByDescending(p => p.Price).ThenByDescending(a => a.Weight).Skip((page - 1) * size).Take(size).ProjectTo<AdvertisementViewModel>(MapperConfig).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        /// <summary>
        /// 保存banner
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Authority]
        public async Task<IActionResult> Save(AdvertisementInputDto model)
        {
            model.CategoryId = model.CategoryId?.Replace("null", "");
            var entity = AdsService.GetById(model.Id);
            if (entity != null)
            {
                Mapper.Map(model, entity);
                bool b1 = await AdsService.SaveChangesAsync() > 0;
                return ResultData(null, b1, b1 ? "修改成功" : "修改失败");
            }

            bool b = AdsService.AddEntitySaved(model.Mapper<Advertisement>()) != null;
            return ResultData(null, b, b ? "添加成功" : "添加失败");
        }

        /// <summary>
        /// 删除banner
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("{id}"), HttpGet("{id}"), Authority]
        public IActionResult Delete(int id)
        {
            bool b = AdsService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }


        /// <summary>
        /// 禁用或开启文章评论
        /// </summary>
        /// <param name="id">文章id</param>
        /// <returns></returns>
        [Authority, HttpPost("{id}")]
        public ActionResult ChangeState(int id)
        {
            var ad = AdsService.GetById(id);
            if (ad != null)
            {
                ad.Status = ad.Status == Status.Available ? Status.Unavailable : Status.Available;
                return ResultData(null, AdsService.SaveChanges() > 0, ad.Status == Status.Available ? $"【{ad.Title}】已上架！" : $"【{ad.Title}】已下架！");
            }

            return ResultData(null, false, "广告不存在");
        }

    }
}