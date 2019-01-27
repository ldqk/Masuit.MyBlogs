using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 联系方式管理
    /// </summary>
    public class ContactController : AdminController
    {
        /// <summary>
        /// ContactService
        /// </summary>
        public IContactsService ContactService { get; set; }

        /// <summary>
        /// 联系方式管理
        /// </summary>
        /// <param name="contactsService"></param>
        public ContactController(IContactsService contactsService)
        {
            ContactService = contactsService;
        }

        /// <summary>
        /// 添加联系方式
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        public ActionResult Add(Contacts links)
        {
            var e = ContactService.AddEntitySaved(links);
            return e != null ? ResultData(null, message: "添加成功！") : ResultData(null, false, "添加失败！");
        }

        /// <summary>
        /// 删除联系方式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            bool b = ContactService.DeleteByIdSaved(id);
            return ResultData(null, b, b ? "删除成功！" : "删除失败！");
        }

        /// <summary>
        /// 编辑联系方式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Edit(Contacts model)
        {
            var contacts = ContactService.GetById(model.Id);
            contacts.Title = model.Title;
            contacts.Url = model.Url;
            bool b = ContactService.UpdateEntitySaved(contacts);
            return ResultData(null, b, b ? "保存成功" : "保存失败");
        }

        /// <summary>
        /// 分页数据
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public ActionResult GetPageData(int page = 1, int size = 10)
        {
            var list = ContactService.LoadPageEntitiesNoTracking(page, size, out int total, l => true, l => l.Id, false).ToList();
            var pageCount = Math.Ceiling(total * 1.0 / size).ToInt32();
            return PageResult(list, pageCount, total);
        }
    }
}