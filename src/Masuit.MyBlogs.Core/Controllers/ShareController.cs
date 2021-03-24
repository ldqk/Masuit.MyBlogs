using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<ActionResult> Remove(int id)
        {
            bool b = await FastShareService.DeleteByIdSavedAsync(id) > 0;
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        /// <summary>
        /// 更新快速分享
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> Update(FastShare model)
        {
            var b = await FastShareService.GetQuery(s => s.Id == model.Id).UpdateFromQueryAsync(s => new FastShare()
            {
                Title = model.Title,
                Link = model.Link,
                Sort = model.Sort
            }) > 0;
            return ResultData(null, b, b ? "更新成功" : "更新失败");
        }
    }
}