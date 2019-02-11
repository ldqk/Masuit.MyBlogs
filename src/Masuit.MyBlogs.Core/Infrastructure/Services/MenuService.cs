using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Application;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class MenuService : BaseService<Menu>, IMenuService
    {
        /// <summary>
        /// 通过存储过程获得自己以及自己所有的子元素集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Menu> GetChildrenMenusByParentId(int id)
        {
            return SqlQuery<Menu>("exec sp_getChildrenMenusByParentId " + id);
        }

        public MenuService(IBaseRepository<Menu> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }
    }
}