using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Application;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class CategoryService : BaseService<Category>, ICategoryService
    {
        /// <summary>
        /// 删除分类，并将该分类下的文章移动到新分类下
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mid"></param>
        /// <returns></returns>
        public bool Delete(int id, int mid)
        {
            Category category = GetById(id);
            Category moveCat = GetById(mid);
            category.Post.ForEach(p =>
            {
                moveCat.Post.Add(p);
                p.PostHistoryVersion.ForEach(v => moveCat.PostHistoryVersion.Add(v));
            });
            category.PostHistoryVersion.ForEach(p =>
            {
                moveCat.PostHistoryVersion.Add(p);
            });
            UpdateEntity(moveCat);
            bool b = DeleteByIdSaved(id);
            return b;
        }

        public CategoryService(IBaseRepository<Category> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }
    }
}