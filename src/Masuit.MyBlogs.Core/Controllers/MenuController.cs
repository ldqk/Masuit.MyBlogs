using AutoMapper;
using Common;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    public class MenuController : AdminController
    {
        /// <summary>
        /// 菜单数据服务
        /// </summary>
        public IMenuService MenuService { get; set; }

        /// <summary>
        /// 菜单管理
        /// </summary>
        /// <param name="menuService"></param>
        public MenuController(IMenuService menuService)
        {
            MenuService = menuService;
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult GetMenus()
        {
            var menus = MenuService.GetAll(m => m.ParentId).ThenBy(m => m.Sort).ToList();
            return ResultData(menus);
        }

        /// <summary>
        /// 获取菜单类型
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(int id)
        {
            var menus = MenuService.GetChildrenMenusByParentId(id);
            bool b = MenuService.DeleteEntitiesSaved(menus);
            return ResultData(null, b, b ? "删除成功" : "删除失败");
        }


        /// <summary>
        /// 保持菜单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult Save(MenuInputDto model)
        {
            if (string.IsNullOrEmpty(model.Icon) || !model.Icon.Contains("/"))
            {
                model.Icon = null;
            }
            Menu m = MenuService.GetById(model.Id);
            if (m == null)
            {
                var menu = MenuService.AddEntitySaved(model.Mapper<Menu>());
                if (menu != null)
                {
                    return ResultData(menu, true, "添加成功");
                }
                return ResultData(null, false, "添加失败");
            }
            Mapper.Map(model, m);
            bool b = MenuService.UpdateEntitySaved(m);
            return ResultData(null, b, b ? "修改成功" : "修改失败");
        }
    }
}