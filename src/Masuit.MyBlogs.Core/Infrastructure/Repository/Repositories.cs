using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository;

public partial class CategoryRepository : BaseRepository<Category>, ICategoryRepository
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

public partial class LinksRepository : BaseRepository<Links>, ILinksRepository
{
}

public partial class LinkLoopbackRepository : BaseRepository<LinkLoopback>, ILinkLoopbackRepository
{
}

public partial class LoginRecordRepository : BaseRepository<LoginRecord>, ILoginRecordRepository
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

public partial class SeminarRepository : BaseRepository<Seminar>, ISeminarRepository
{
}

public partial class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
{
}

public partial class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
{
}

public partial class PostMergeRequestRepository : BaseRepository<PostMergeRequest>, IPostMergeRequestRepository
{
}

public partial class AdvertisementRepository : BaseRepository<Advertisement>, IAdvertisementRepository
{
}

public partial class AdvertisementClickRecordRepository : BaseRepository<AdvertisementClickRecord>, IAdvertisementClickRecordRepository
{
}

public partial class VariablesRepository : BaseRepository<Variables>, IVariablesRepository
{
}

public partial class PostVisitRecordRepository : BaseRepository<PostVisitRecord>, IPostVisitRecordRepository
{
}

public partial class PostVisitRecordStatsRepository : BaseRepository<PostVisitRecordStats>, IPostVisitRecordStatsRepository
{
}

public partial class PostTagsRepository : BaseRepository<PostTag>, IPostTagsRepository
{
}