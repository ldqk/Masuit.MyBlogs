using Dispose.Scope;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed partial class CategoryService(IBaseRepository<Category> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Category>(repository, searchEngine, searcher), ICategoryService
{
    /// <summary>
	/// 删除分类，并将该分类下的文章移动到新分类下
	/// </summary>
	/// <param name="id"></param>
	/// <param name="mid"></param>
	/// <returns></returns>
	public async Task<bool> Delete(int id, int mid)
    {
        var category = await GetByIdAsync(id);
        var categories = GetQuery(c => c.Path.StartsWith(category.Path)).ToPooledListScope();
        var moveCat = await GetByIdAsync(mid);
        foreach (var c in categories)
        {
            for (var j = 0; j < c.Post.Count; j++)
            {
                var p = c.Post.ElementAt(j);
                moveCat.Post.Add(p);
                for (var i = 0; i < p.PostHistoryVersion.Count; i++)
                {
                    moveCat.PostHistoryVersion.Add(p.PostHistoryVersion.ElementAt(i));
                }
            }

            for (var i = 0; i < c.PostHistoryVersion.Count; i++)
            {
                var p = c.PostHistoryVersion.ElementAt(i);
                p.CategoryId = moveCat.Id;
                moveCat.PostHistoryVersion.Add(p);
            }
        }

        bool b = await DeleteByIdAsync(id) > 0;
        return b;
    }
}
