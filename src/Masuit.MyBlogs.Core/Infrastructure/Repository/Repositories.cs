using Masuit.MyBlogs.Core.Infrastructure.Application;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using System.Data;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class BroadcastRepository : BaseRepository<Broadcast>, IBroadcastRepository
    {
        public BroadcastRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class DonateRepository : BaseRepository<Donate>, IDonateRepository
    {
        public DonateRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class FastShareRepository : BaseRepository<FastShare>, IFastShareRepository
    {
        public FastShareRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class InternalMessageRepository : BaseRepository<InternalMessage>, IInternalMessageRepository
    {
        public InternalMessageRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }

    public partial class LeaveMessageRepository : BaseRepository<LeaveMessage>, ILeaveMessageRepository
    {
        public LeaveMessageRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class LinksRepository : BaseRepository<Links>, ILinksRepository
    {
        public LinksRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class LoginRecordRepository : BaseRepository<LoginRecord>, ILoginRecordRepository
    {
        public LoginRecordRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class MenuRepository : BaseRepository<Menu>, IMenuRepository
    {
        public MenuRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class MiscRepository : BaseRepository<Misc>, IMiscRepository
    {
        public MiscRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class NoticeRepository : BaseRepository<Notice>, INoticeRepository
    {
        public NoticeRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }

    public partial class PostAccessRecordRepository : BaseRepository<PostAccessRecord>, IPostAccessRecordRepository
    {
        public PostAccessRecordRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class PostHistoryVersionRepository : BaseRepository<PostHistoryVersion>, IPostHistoryVersionRepository
    {
        public PostHistoryVersionRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class SearchDetailsRepository : BaseRepository<SearchDetails>, ISearchDetailsRepository
    {
        public SearchDetailsRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class SeminarRepository : BaseRepository<Seminar>, ISeminarRepository
    {
        public SeminarRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
    {
        public SystemSettingRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
        public UserInfoRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }

    public partial class SeminarPostRepository : BaseRepository<SeminarPost>, ISeminarPostRepository
    {
        public SeminarPostRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
    public partial class SeminarPostHistoryVersionRepository : BaseRepository<SeminarPostHistoryVersion>, ISeminarPostHistoryVersionRepository
    {
        public SeminarPostHistoryVersionRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
}