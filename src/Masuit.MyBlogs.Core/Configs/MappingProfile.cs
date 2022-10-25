using AutoMapper;
using Masuit.MyBlogs.Core.Models.Command;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Systems;

namespace Masuit.MyBlogs.Core.Configs
{
    /// <summary>
    /// 注册automapper
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CategoryCommand, Category>().ForMember(c => c.ParentId, e => e.MapFrom(c => c.ParentId > 0 ? c.ParentId : null)).ReverseMap();
            CreateMap<Category, CategoryDto_P>().ReverseMap();
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryCommand, CategoryDto>().ReverseMap();

            CreateMap<CommentCommand, Comment>().ForMember(c => c.Status, e => e.MapFrom(c => Status.Pending)).ForMember(c => c.ParentId, e => e.MapFrom(c => c.ParentId > 0 ? c.ParentId : null)).ReverseMap();
            CreateMap<Comment, CommentDto>().ReverseMap();
            CreateMap<CommentCommand, CommentDto>().ReverseMap();
            CreateMap<Comment, CommentViewModel>().ForMember(c => c.CommentDate, e => e.MapFrom(c => c.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<LeaveMessageCommand, LeaveMessage>().ForMember(c => c.Status, e => e.MapFrom(c => Status.Pending)).ForMember(c => c.ParentId, e => e.MapFrom(c => c.ParentId > 0 ? c.ParentId : null)).ReverseMap();
            CreateMap<LeaveMessage, LeaveMessageDto>().ReverseMap();
            CreateMap<LeaveMessageCommand, LeaveMessageDto>().ReverseMap();
            CreateMap<LeaveMessage, LeaveMessageViewModel>().ForMember(l => l.PostDate, e => e.MapFrom(l => l.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<Links, LinksDto>().ForMember(e => e.Loopbacks, e => e.MapFrom(m => m.Loopbacks.GroupBy(e =>
                e.IP).Count())).ReverseMap();

            CreateMap<MenuCommand, Menu>().ForMember(c => c.ParentId, e => e.MapFrom(c => c.ParentId > 0 ? c.ParentId : null)).ReverseMap();
            CreateMap<Menu, MenuDto>().ForMember(m => m.Children, e => e.MapFrom(m => m.Children.OrderBy(c => c.Sort).ToList())).ReverseMap();

            CreateMap<Misc, MiscDto>().ReverseMap();

            CreateMap<Notice, NoticeDto>().ReverseMap();

            CreateMap<PostCommand, Post>().ReverseMap();
            CreateMap<Post, PostModelBase>();
            CreateMap<Post, PostHistoryVersion>().ForMember(p => p.Id, e => e.Ignore()).ForMember(v => v.PostId, e => e.MapFrom(p => p.Id));
            CreateMap<Post, PostDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.LimitMode, e => e.MapFrom(p => p.LimitMode ?? RegionLimitMode.All)).ForMember(p => p.Category, e => e.Ignore()).ReverseMap();
            CreateMap<PostCommand, PostDto>().ReverseMap();
            CreateMap<PostHistoryVersion, PostDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ReverseMap();
            CreateMap<Post, PostDataModel>().ForMember(p => p.ModifyDate, e => e.MapFrom(p => p.ModifyDate))
                .ForMember(p => p.PostDate, e => e.MapFrom(p => p.PostDate))
                .ForMember(p => p.Status, e => e.MapFrom(p => p.Status.GetDisplay()))
                .ForMember(p => p.ModifyCount, e => e.MapFrom(p => p.PostHistoryVersion.Count))
                .ForMember(p => p.ViewCount, e => e.MapFrom(p => p.TotalViewCount))
                .ForMember(p => p.Seminars, e => e.MapFrom(p => p.Seminar.Select(s => s.Id).ToArray()))
                .ForMember(p => p.LimitDesc, e => e.MapFrom(p => p.LimitMode > RegionLimitMode.All ? string.Format(p.LimitMode.GetDescription(), p.Regions, p.ExceptRegions) : "无限制"));

            CreateMap<SearchDetails, SearchDetailsDto>().ReverseMap();

            CreateMap<UserInfo, UserInfoDto>();
            CreateMap<UserInfoDto, UserInfo>()
                .ForMember(u => u.Id, e => e.Ignore())
                .ForMember(u => u.Password, e => e.Ignore())
                .ForMember(u => u.SaltKey, e => e.Ignore());

            CreateMap<LoginRecord, LoginRecordViewModel>().ReverseMap();

            CreateMap<Seminar, SeminarDto>().ReverseMap();

            CreateMap<PostMergeRequestCommandBase, PostMergeRequest>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.MergeState, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequestCommand, PostMergeRequest>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.MergeState, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequestCommand, Post>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.Status, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequest, PostMergeRequestDtoBase>().ForMember(p => p.PostTitle, e => e.MapFrom(r => r.Post.Title));
            CreateMap<PostMergeRequest, PostMergeRequestDto>().ForMember(p => p.PostTitle, e => e.MapFrom(r => r.Post.Title));
            CreateMap<PostMergeRequest, Post>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.Status, e => e.Ignore()).ReverseMap();
            CreateMap<Post, PostMergeRequestDto>().ReverseMap();

            CreateMap<Advertisement, AdvertisementViewModel>()
                .ForMember(m => m.AverageViewCount, e => e.MapFrom(a => a.ClickRecords.Where(o => o.Time >= DateTime.Today.AddMonths(-1)).GroupBy(r => r.Time.Date).Select(g => g.Count()).DefaultIfEmpty().Average()))
                .ForMember(m => m.ViewCount, e => e.MapFrom(a => a.ClickRecords.Count(o => o.Time >= DateTime.Today.AddMonths(-1))));
            CreateMap<AdvertisementDto, Advertisement>().ForMember(a => a.ClickRecords, e => e.Ignore()).ForMember(a => a.Status, e => e.Ignore()).ForMember(a => a.UpdateTime, e => e.MapFrom(a => DateTime.Now));

            CreateMap<Donate, DonateDto>();

            CreateMap<PostVisitRecord, PostVisitRecordViewModel>().ForMember(m => m.Time, e => e.MapFrom(m => m.Time.ToString("yyyy-MM-dd HH:mm:ss")));

            CreateMap<AdvertisementClickRecord, AdvertisementClickRecordViewModel>().ForMember(m => m.Time, e => e.MapFrom(m => m.Time.ToString("yyyy-MM-dd HH:mm:ss")));
        }
    }
}
