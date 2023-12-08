using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed partial class CommentService(IBaseRepository<Comment> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Comment>(repository, searchEngine, searcher), ICommentService;
