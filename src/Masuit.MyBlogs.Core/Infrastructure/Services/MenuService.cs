using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed partial class MenuService(IBaseRepository<Menu> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Menu>(repository, searchEngine, searcher), IMenuService;
