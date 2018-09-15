using IBLL;
using Models.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class ShareController : AdminController
    {
        public IFastShareBll FastShareBll { get; set; }

        public ActionResult Index()
        {
            var shares = FastShareBll.GetAll(s => s.Sort).ToList();
            return ResultData(shares);
        }

        [HttpPost]
        public ActionResult Add(FastShare share)
        {
            bool b = FastShareBll.AddEntitySaved(share) != null;
            return ResultData(null, b, b ? "添加成功" : "添加失败");
        }

        [HttpPost]
        public ActionResult Remove(int id)
        {
            bool b = FastShareBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }

        [HttpPost]
        public ActionResult Update(FastShare model)
        {
            FastShare share = FastShareBll.GetById(model.Id);
            share.Title = model.Title;
            share.Link = model.Link;
            share.Sort = model.Sort;
            bool b = FastShareBll.UpdateEntitySaved(share);
            return ResultData(null, b, b ? "更新成功" : "更新失败");
        }
    }
}