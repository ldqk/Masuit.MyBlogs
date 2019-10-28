using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class BroadcastRepository : BaseRepository<Broadcast>, IBroadcastRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Broadcast AddEntity(Broadcast t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Category AddEntity(Category t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Comment AddEntity(Comment t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class DonateRepository : BaseRepository<Donate>, IDonateRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Donate AddEntity(Donate t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class FastShareRepository : BaseRepository<FastShare>, IFastShareRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override FastShare AddEntity(FastShare t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class InternalMessageRepository : BaseRepository<InternalMessage>, IInternalMessageRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override InternalMessage AddEntity(InternalMessage t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class LeaveMessageRepository : BaseRepository<LeaveMessage>, ILeaveMessageRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override LeaveMessage AddEntity(LeaveMessage t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class LinksRepository : BaseRepository<Links>, ILinksRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Links AddEntity(Links t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class LoginRecordRepository : BaseRepository<LoginRecord>, ILoginRecordRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override LoginRecord AddEntity(LoginRecord t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class MenuRepository : BaseRepository<Menu>, IMenuRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Menu AddEntity(Menu t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class MiscRepository : BaseRepository<Misc>, IMiscRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Misc AddEntity(Misc t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class NoticeRepository : BaseRepository<Notice>, INoticeRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Notice AddEntity(Notice t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class PostHistoryVersionRepository : BaseRepository<PostHistoryVersion>, IPostHistoryVersionRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override PostHistoryVersion AddEntity(PostHistoryVersion t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class SearchDetailsRepository : BaseRepository<SearchDetails>, ISearchDetailsRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override SearchDetails AddEntity(SearchDetails t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class SeminarRepository : BaseRepository<Seminar>, ISeminarRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Seminar AddEntity(Seminar t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class SystemSettingRepository : BaseRepository<SystemSetting>, ISystemSettingRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override SystemSetting AddEntity(SystemSetting t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class UserInfoRepository : BaseRepository<UserInfo>, IUserInfoRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override UserInfo AddEntity(UserInfo t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class SeminarPostRepository : BaseRepository<SeminarPost>, ISeminarPostRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override SeminarPost AddEntity(SeminarPost t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class SeminarPostHistoryVersionRepository : BaseRepository<SeminarPostHistoryVersion>, ISeminarPostHistoryVersionRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override SeminarPostHistoryVersion AddEntity(SeminarPostHistoryVersion t)
        {
            DataContext.Add(t);
            return t;
        }
    }

    public partial class PostMergeRequestRepository : BaseRepository<PostMergeRequest>, IPostMergeRequestRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override PostMergeRequest AddEntity(PostMergeRequest t)
        {
            DataContext.Add(t);
            return t;
        }
    }
    public partial class AdvertisementRepository : BaseRepository<Advertisement>, IAdvertisementRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Advertisement AddEntity(Advertisement t)
        {
            DataContext.Add(t);
            return t;
        }
    }
}