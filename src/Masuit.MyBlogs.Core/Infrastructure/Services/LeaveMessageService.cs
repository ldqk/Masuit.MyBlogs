using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed partial class LeaveMessageService(IBaseRepository<LeaveMessage> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<LeaveMessage>(repository, searchEngine, searcher), ILeaveMessageService;
