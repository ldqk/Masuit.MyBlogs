using System;
using System.Linq;
using System.Web.Mvc;
using IBLL;
using Models.Entity;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class DonateController : AdminController
    {
        public IDonateBll DonateBll { get; set; }
        public DonateController(IDonateBll donateBll)
        {
            DonateBll = donateBll;
        }
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = DonateBll.LoadPageEntitiesFromL2CacheNoTracking(page, size, out int total, d => true, d => d.DonateTime, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }

        public ActionResult Get(int id)
        {
            Donate donate = DonateBll.GetById(id);
            return ResultData(donate);
        }

        public ActionResult Save(Donate donate)
        {
            bool b = DonateBll.AddOrUpdateSaved(d => d.Id, donate) > 0;
            return ResultData(null, b, b ? "保存成功！" : "保存失败！");
        }

        public ActionResult Delete(int id)
        {
            bool b = DonateBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }
    }
}