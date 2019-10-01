using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class MenuService : BaseService<Menu>, IMenuService
    {
        public MenuService(IBaseRepository<Menu> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }

        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Menu> GetChildrenMenusByParentId(int id)
        {
            Menu c = GetFromCache(m => m.Id == id);
            var menus = new List<Menu>() { c };
            GetChildrenMenusByParentId(c, menus);
            return menus;
        }

        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private void GetChildrenMenusByParentId(Menu menu, List<Menu> list)
        {
            var menus = GetQueryFromCache(x => x.ParentId == menu.Id).ToList();
            if (menus.Any())
            {
                list.AddRange(menus);
                foreach (var c in menus)
                {
                    GetChildrenMenusByParentId(c, list);
                }
            }
        }

    }
}