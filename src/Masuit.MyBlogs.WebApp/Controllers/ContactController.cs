using System;
using System.Linq;
using System.Web.Mvc;
using IBLL;
using Masuit.Tools;
using Models.Entity;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class ContactController : AdminController
    {
        public IContactsBll ContactBll { get; set; }

        public ContactController(IContactsBll contactsBll)
        {
            ContactBll = contactsBll;
        }

        public ActionResult Add(Contacts links)
        {
            var e = ContactBll.AddEntitySaved(links);
            return e != null ? ResultData(null, message: "添加成功！") : ResultData(null, false, "添加失败！");
        }

        public ActionResult Delete(int id)
        {
            bool b = ContactBll.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        public ActionResult Edit(Contacts model)
        {
            var contacts = ContactBll.GetById(model.Id);
            contacts.Title = model.Title;
            contacts.Url = model.Url;
            bool b = ContactBll.UpdateEntitySaved(contacts);
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = ContactBll.LoadPageEntitiesNoTracking(page, size, out int total, l => true, l => l.Id, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }
    }
}