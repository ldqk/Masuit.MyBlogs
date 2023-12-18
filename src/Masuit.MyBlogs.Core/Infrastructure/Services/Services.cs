using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed partial class DonateService(IBaseRepository<Donate> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Donate>(repository, searchEngine, searcher), IDonateService;

public sealed partial class FastShareService(IBaseRepository<FastShare> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<FastShare>(repository, searchEngine, searcher), IFastShareService;

public sealed partial class InternalMessageService(IBaseRepository<InternalMessage> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<InternalMessage>(repository, searchEngine, searcher), IInternalMessageService;

public sealed partial class LinksService(IBaseRepository<Links> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Links>(repository, searchEngine, searcher), ILinksService;

public sealed partial class LinkLoopbackService(IBaseRepository<LinkLoopback> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<LinkLoopback>(repository, searchEngine, searcher), ILinkLoopbackService;

public sealed partial class LoginRecordService(IBaseRepository<LoginRecord> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<LoginRecord>(repository, searchEngine, searcher), ILoginRecordService;

public sealed partial class MiscService(IBaseRepository<Misc> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Misc>(repository, searchEngine, searcher), IMiscService;

public sealed partial class NoticeService(IBaseRepository<Notice> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Notice>(repository, searchEngine, searcher), INoticeService;

public sealed partial class PostHistoryVersionService(IBaseRepository<PostHistoryVersion> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<PostHistoryVersion>(repository, searchEngine, searcher), IPostHistoryVersionService;

public sealed partial class SeminarService(IBaseRepository<Seminar> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Seminar>(repository, searchEngine, searcher), ISeminarService;

public sealed partial class SystemSettingService(IBaseRepository<SystemSetting> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<SystemSetting>(repository, searchEngine, searcher), ISystemSettingService;

public sealed partial class PostMergeRequestService(IBaseRepository<PostMergeRequest> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<PostMergeRequest>(repository, searchEngine, searcher), IPostMergeRequestService;

public sealed partial class VariablesService(IBaseRepository<Variables> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<Variables>(repository, searchEngine, searcher), IVariablesService;

public sealed partial class PostVisitRecordService(IBaseRepository<PostVisitRecord> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<PostVisitRecord>(repository, searchEngine, searcher), IPostVisitRecordService;

public sealed partial class PostVisitRecordStatsService(IBaseRepository<PostVisitRecordStats> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<PostVisitRecordStats>(repository, searchEngine, searcher), IPostVisitRecordStatsService;

public sealed partial class AdvertisementClickRecordService(IBaseRepository<AdvertisementClickRecord> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<AdvertisementClickRecord>(repository, searchEngine, searcher), IAdvertisementClickRecordService;

public sealed partial class PostTagService(IBaseRepository<PostTag> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<PostTag>(repository, searchEngine, searcher), IPostTagService;

public sealed partial class EmailBlocklistService(IBaseRepository<EmailBlocklist> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : BaseService<EmailBlocklist>(repository, searchEngine, searcher), IEmailBlocklistService;
