using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository;

public sealed partial class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
}

public sealed partial class DonateRepository : BaseRepository<Donate>, IDonateRepository
{
}

public sealed partial class FastShareRepository : BaseRepository<FastShare>, IFastShareRepository
{
}

public sealed partial class InternalMessageRepository : BaseRepository<InternalMessage>, IInternalMessageRepository
{
}

public sealed partial class LinksRepository : BaseRepository<Links>, ILinksRepository
{
}

public sealed partial class LinkLoopbackRepository : BaseRepository<LinkLoopback>, ILinkLoopbackRepository
{
}

public sealed partial class LoginRecordRepository : BaseRepository<LoginRecord>, ILoginRecordRepository
{
}

public sealed partial class MiscRepository : BaseRepository<Misc>, IMiscRepository
{
}

public sealed partial class NoticeRepository : BaseRepository<Notice>, INoticeRepository
{
}

public sealed partial class PostHistoryVersionRepository : BaseRepository<PostHistoryVersion>, IPostHistoryVersionRepository
{
}

public sealed partial class SeminarRepository : BaseRepository<Seminar>, ISeminarRepository
{
}

public sealed partial class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
{
}

public sealed partial class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
{
}

public sealed partial class PostMergeRequestRepository : BaseRepository<PostMergeRequest>, IPostMergeRequestRepository
{
}

public sealed partial class AdvertisementRepository : BaseRepository<Advertisement>, IAdvertisementRepository
{
}

public sealed partial class AdvertisementClickRecordRepository : BaseRepository<AdvertisementClickRecord>, IAdvertisementClickRecordRepository
{
}

public sealed partial class VariablesRepository : BaseRepository<Variables>, IVariablesRepository
{
}

public sealed partial class PostVisitRecordRepository : BaseRepository<PostVisitRecord>, IPostVisitRecordRepository
{
}

public sealed partial class PostVisitRecordStatsRepository : BaseRepository<PostVisitRecordStats>, IPostVisitRecordStatsRepository
{
}

public sealed partial class PostTagsRepository : BaseRepository<PostTag>, IPostTagsRepository
{
}