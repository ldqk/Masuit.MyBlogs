using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class BroadcastRepository : BaseRepository<Broadcast>, IBroadcastRepository
    {
    }

    public partial class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
    }

    public partial class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
    }

    public partial class DonateRepository : BaseRepository<Donate>, IDonateRepository
    {
    }

    public partial class FastShareRepository : BaseRepository<FastShare>, IFastShareRepository
    {
    }

    public partial class InternalMessageRepository : BaseRepository<InternalMessage>, IInternalMessageRepository
    {
    }

    public partial class LeaveMessageRepository : BaseRepository<LeaveMessage>, ILeaveMessageRepository
    {
    }

    public partial class LinksRepository : BaseRepository<Links>, ILinksRepository
    {
    }

    public partial class LoginRecordRepository : BaseRepository<LoginRecord>, ILoginRecordRepository
    {
    }

    public partial class MenuRepository : BaseRepository<Menu>, IMenuRepository
    {
    }

    public partial class MiscRepository : BaseRepository<Misc>, IMiscRepository
    {
    }

    public partial class NoticeRepository : BaseRepository<Notice>, INoticeRepository
    {
    }

    public partial class PostHistoryVersionRepository : BaseRepository<PostHistoryVersion>, IPostHistoryVersionRepository
    {
    }

    public partial class SearchDetailsRepository : BaseRepository<SearchDetails>, ISearchDetailsRepository
    {
    }

    public partial class SeminarRepository : BaseRepository<Seminar>, ISeminarRepository
    {
    }

    public partial class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
    {
    }

    public partial class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
    }

    public partial class SeminarPostRepository : BaseRepository<SeminarPost>, ISeminarPostRepository
    {
    }

    public partial class SeminarPostHistoryVersionRepository : BaseRepository<SeminarPostHistoryVersion>, ISeminarPostHistoryVersionRepository
    {
    }

    public partial class BannerRepository : BaseRepository<Banner>, IBannerRepository
    {
    }
    public partial class PostMergeRequestRepository : BaseRepository<PostMergeRequest>, IPostMergeRequestRepository
    {
    }
}