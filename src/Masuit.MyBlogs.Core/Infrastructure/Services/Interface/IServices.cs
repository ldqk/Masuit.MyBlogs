using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface
{
    public partial interface IBroadcastService : IBaseService<Broadcast> { }

    public partial interface IDonateService : IBaseService<Donate> { }

    public partial interface IFastShareService : IBaseService<FastShare> { }

    public partial interface IInternalMessageService : IBaseService<InternalMessage> { }

    public partial interface ILinksService : IBaseService<Links> { }

    public partial interface ILoginRecordService : IBaseService<LoginRecord> { }

    public partial interface IMiscService : IBaseService<Misc> { }

    public partial interface INoticeService : IBaseService<Notice> { }

    public partial interface IPostAccessRecordService : IBaseService<PostAccessRecord> { }

    public partial interface IPostHistoryVersionService : IBaseService<PostHistoryVersion> { }

    public partial interface ISearchDetailsService : IBaseService<SearchDetails> { }

    public partial interface ISeminarService : IBaseService<Seminar> { }

    public partial interface ISystemSettingService : IBaseService<SystemSetting> { }

    public partial interface ISeminarPostService : IBaseService<SeminarPost> { }
    public partial interface ISeminarPostHistoryVersionService : IBaseService<SeminarPostHistoryVersion> { }
    public partial interface IBannerService : IBaseService<Banner> { }

}