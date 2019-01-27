using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class BroadcastService : BaseService<Broadcast>, IBroadcastService
    {
        public BroadcastService(IBaseRepository<Broadcast> repository) : base(repository)
        {
        }
    }

    public partial class ContactsService : BaseService<Contacts>, IContactsService
    {
        public ContactsService(IBaseRepository<Contacts> repository) : base(repository)
        {
        }
    }

    public partial class DonateService : BaseService<Donate>, IDonateService
    {
        public DonateService(IBaseRepository<Donate> repository) : base(repository)
        {
        }
    }

    public partial class FastShareService : BaseService<FastShare>, IFastShareService
    {
        public FastShareService(IBaseRepository<FastShare> repository) : base(repository)
        {
        }
    }

    public partial class InternalMessageService : BaseService<InternalMessage>, IInternalMessageService
    {
        public InternalMessageService(IBaseRepository<InternalMessage> repository) : base(repository)
        {
        }
    }

    public partial class IssueService : BaseService<Issue>, IIssueService
    {
        public IssueService(IBaseRepository<Issue> repository) : base(repository)
        {
        }
    }

    public partial class LinksService : BaseService<Links>, ILinksService
    {
        public LinksService(IBaseRepository<Links> repository) : base(repository)
        {
        }
    }

    public partial class LoginRecordService : BaseService<LoginRecord>, ILoginRecordService
    {
        public LoginRecordService(IBaseRepository<LoginRecord> repository) : base(repository)
        {
        }
    }

    public partial class MiscService : BaseService<Misc>, IMiscService
    {
        public MiscService(IBaseRepository<Misc> repository) : base(repository)
        {
        }
    }

    public partial class NoticeService : BaseService<Notice>, INoticeService
    {
        public NoticeService(IBaseRepository<Notice> repository) : base(repository)
        {
        }
    }

    public partial class PostAccessRecordService : BaseService<PostAccessRecord>, IPostAccessRecordService
    {
        public PostAccessRecordService(IBaseRepository<PostAccessRecord> repository) : base(repository)
        {
        }
    }

    public partial class PostHistoryVersionService : BaseService<PostHistoryVersion>, IPostHistoryVersionService
    {
        public PostHistoryVersionService(IBaseRepository<PostHistoryVersion> repository) : base(repository)
        {
        }
    }

    public partial class SearchDetailsService : BaseService<SearchDetails>, ISearchDetailsService
    {
        public SearchDetailsService(IBaseRepository<SearchDetails> repository) : base(repository)
        {
        }
    }

    public partial class SeminarService : BaseService<Seminar>, ISeminarService
    {
        public SeminarService(IBaseRepository<Seminar> repository) : base(repository)
        {
        }
    }

    public partial class SystemSettingService : BaseService<SystemSetting>, ISystemSettingService
    {
        public SystemSettingService(IBaseRepository<SystemSetting> repository) : base(repository)
        {
        }
    }

    public partial class SeminarPostService : BaseService<SeminarPost>, ISeminarPostService
    {
        public SeminarPostService(IBaseRepository<SeminarPost> repository) : base(repository)
        {
        }
    }

    public partial class SeminarPostHistoryVersionService : BaseService<SeminarPostHistoryVersion>, ISeminarPostHistoryVersionService
    {
        public SeminarPostHistoryVersionService(IBaseRepository<SeminarPostHistoryVersion> repository) : base(repository)
        {
        }
    }
}