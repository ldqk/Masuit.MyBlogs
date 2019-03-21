using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class BroadcastRepository : BaseRepository<Broadcast>, IBroadcastRepository
    {
        public BroadcastRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class DonateRepository : BaseRepository<Donate>, IDonateRepository
    {
        public DonateRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class FastShareRepository : BaseRepository<FastShare>, IFastShareRepository
    {
        public FastShareRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class InternalMessageRepository : BaseRepository<InternalMessage>, IInternalMessageRepository
    {
        public InternalMessageRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class LeaveMessageRepository : BaseRepository<LeaveMessage>, ILeaveMessageRepository
    {
        public LeaveMessageRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class LinksRepository : BaseRepository<Links>, ILinksRepository
    {
        public LinksRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class LoginRecordRepository : BaseRepository<LoginRecord>, ILoginRecordRepository
    {
        public LoginRecordRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class MenuRepository : BaseRepository<Menu>, IMenuRepository
    {
        public MenuRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class MiscRepository : BaseRepository<Misc>, IMiscRepository
    {
        public MiscRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class NoticeRepository : BaseRepository<Notice>, INoticeRepository
    {
        public NoticeRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class PostAccessRecordRepository : BaseRepository<PostAccessRecord>, IPostAccessRecordRepository
    {
        public PostAccessRecordRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class PostHistoryVersionRepository : BaseRepository<PostHistoryVersion>, IPostHistoryVersionRepository
    {
        public PostHistoryVersionRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class SearchDetailsRepository : BaseRepository<SearchDetails>, ISearchDetailsRepository
    {
        public SearchDetailsRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class SeminarRepository : BaseRepository<Seminar>, ISeminarRepository
    {
        public SeminarRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
    {
        public SystemSettingRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
        public UserInfoRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class SeminarPostRepository : BaseRepository<SeminarPost>, ISeminarPostRepository
    {
        public SeminarPostRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class SeminarPostHistoryVersionRepository : BaseRepository<SeminarPostHistoryVersion>, ISeminarPostHistoryVersionRepository
    {
        public SeminarPostHistoryVersionRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

    public partial class BannerRepository : BaseRepository<Banner>, IBannerRepository
    {
        public BannerRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}