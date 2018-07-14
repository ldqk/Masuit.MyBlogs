using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Common;
using IBLL;
using Masuit.Tools.Systems;
using Models.DTO;
using Models.Entity;
using Models.Enum;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class MenuController : AdminController
    {
        public IMenuBll MenuBll { get; set; }

        public MenuController(IMenuBll menuBll)
        {
            MenuBll = menuBll;
        }

        public ActionResult GetMenus()
        {
            var menus = MenuBll.GetAll(m => m.Sort).ToList();
            return ResultData(menus);
        }

        public ActionResult GetMenuType()
        {
            Array array = Enum.GetValues(typeof(MenuType));
            var list = new List<object>();
            foreach (Enum e in array)
            {
                list.Add(new
                {
                    e,
                    name = e.GetDisplay()
                });
            }
            return ResultData(list);
        }

        public ActionResult Delete(int id)
        {
            DbRawSqlQuery<Menu> menus = MenuBll.GetChildrenMenusByParentId(id);
            bool b = MenuBll.DeleteEntitiesSaved(menus);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }


        public ActionResult Save(MenuInputDto model)
        {
            if (string.IsNullOrEmpty(model.Icon) || !model.Icon.Contains("/"))
            {
                model.Icon = null;
            }
            Menu m = MenuBll.GetById(model.Id);
            if (m == null)
            {
                var menu = MenuBll.AddEntitySaved(model.Mapper<Menu>());
                if (menu != null)
                {
                    return ResultData(menu, true, "添加成功");
                }
                return ResultData(null, false, "添加失败");
            }
            Mapper.Map(model, m);
            bool b = MenuBll.UpdateEntitySaved(m);
            return ResultData(null, b, b ? "修改成功" : "修改失败");
        }
    }
}