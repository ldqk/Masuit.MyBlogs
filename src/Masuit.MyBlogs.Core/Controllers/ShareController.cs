using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 快速分享
    /// </summary>
    public class ShareController : AdminController
    {
        /// <summary>
        /// 快速分享
        /// </summary>
        public IFastShareService FastShareService { get; set; }

        /// <summary>
        /// 快速分享
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var shares = FastShareService.GetAll(s => s.Sort).ToList();
            return ResultData(shares);
        }

        /// <summary>
        /// 添加快速分享
        /// </summary>
        /// <param name="share"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(FastShare share)
        {
            bool b = FastShareService.AddEntitySaved(share) != null;
            return ResultData(null, b, b ? "添加成功" : "添加失败");
        }

        /// <summary>
        /// 移除快速分享
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Remove(int id)
        {
            bool b = FastShareService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 更新快速分享
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Update(FastShare model)
        {
            FastShare share = FastShareService.GetById(model.Id);
            share.Title = model.Title;
            share.Link = model.Link;
            share.Sort = model.Sort;
            bool b = FastShareService.SaveChanges() > 0;
            return ResultData(null, b, b ? "更新成功" : "更新失败");
        }
    }
}