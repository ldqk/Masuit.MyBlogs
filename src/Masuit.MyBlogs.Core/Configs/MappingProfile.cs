using AutoMapper;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using System.Linq;

namespace Masuit.MyBlogs.Core.Configs
{
    /// <summary>
    /// 注册automapper
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Broadcast, BroadcastInputDto>().ReverseMap();
            CreateMap<Broadcast, BroadcastOutputDto>().ReverseMap();
            CreateMap<BroadcastOutputDto, BroadcastInputDto>().ReverseMap();

            CreateMap<Category, CategoryInputDto>().ReverseMap();
            CreateMap<Category, CategoryOutputDto>().ForMember(c => c.TotalPostCount, e => e.MapFrom(c => c.Post.Count)).ForMember(c => c.PendedPostCount, e => e.MapFrom(c => c.Post.Count())).ReverseMap();
            CreateMap<CategoryInputDto, CategoryOutputDto>().ReverseMap();

            CreateMap<Comment, CommentInputDto>().ReverseMap();
            CreateMap<Comment, CommentOutputDto>().ReverseMap();
            CreateMap<CommentInputDto, CommentOutputDto>().ReverseMap();
            CreateMap<Comment, CommentViewModel>().ForMember(c => c.CommentDate, e => e.MapFrom(c => c.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<LeaveMessage, LeaveMessageInputDto>().ReverseMap();
            CreateMap<LeaveMessage, LeaveMessageOutputDto>().ReverseMap();
            CreateMap<LeaveMessageInputDto, LeaveMessageOutputDto>().ReverseMap();
            CreateMap<LeaveMessage, LeaveMessageViewModel>().ForMember(l => l.PostDate, e => e.MapFrom(l => l.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<Links, LinksInputDto>().ReverseMap();
            CreateMap<Links, LinksOutputDto>().ReverseMap();
            CreateMap<LinksInputDto, LinksOutputDto>().ReverseMap();

            CreateMap<Menu, MenuInputDto>().ReverseMap();
            CreateMap<Menu, MenuOutputDto>().ReverseMap();
            CreateMap<MenuInputDto, MenuOutputDto>().ReverseMap();

            CreateMap<Misc, MiscInputDto>().ReverseMap();
            CreateMap<Misc, MiscOutputDto>().ReverseMap();
            CreateMap<MiscInputDto, MiscOutputDto>().ReverseMap();
            CreateMap<Misc, MiscViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<Notice, NoticeInputDto>().ReverseMap();
            CreateMap<Notice, NoticeOutputDto>().ReverseMap();
            CreateMap<NoticeInputDto, NoticeOutputDto>().ReverseMap();
            CreateMap<Notice, NoticeViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<Post, PostInputDto>().ReverseMap();
            CreateMap<Post, PostModelBase>();
            CreateMap<Post, PostHistoryVersion>().ForMember(p => p.Id, e => e.Ignore()).ForMember(v => v.PostId, e => e.MapFrom(p => p.Id));
            CreateMap<Post, PostOutputDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.CommentCount, e => e.MapFrom(p => p.Comment.Count(c => c.Status == Status.Pended))).ReverseMap();
            CreateMap<PostInputDto, PostOutputDto>().ReverseMap();
            CreateMap<PostHistoryVersion, PostOutputDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ReverseMap();
            CreateMap<Post, PostViewModel>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.PostDate, e => e.MapFrom(p => p.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(p => p.ModifyDate, e => e.MapFrom(p => p.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<SearchDetails, SearchDetailsInputDto>().ReverseMap();
            CreateMap<SearchDetails, SearchDetailsOutputDto>().ReverseMap();
            CreateMap<SearchDetailsInputDto, SearchDetailsOutputDto>().ReverseMap();

            CreateMap<UserInfo, UserInfoInputDto>().ReverseMap();
            CreateMap<UserInfo, UserInfoOutputDto>().ReverseMap();
            CreateMap<UserInfoInputDto, UserInfoOutputDto>().ReverseMap();

            CreateMap<LoginRecord, LoginRecordOutputDto>().ReverseMap();

            CreateMap<Seminar, SeminarInputDto>().ReverseMap();
            CreateMap<Seminar, SeminarOutputDto>().ReverseMap();
            CreateMap<SeminarInputDto, SeminarOutputDto>().ReverseMap();

            CreateMap<SeminarPost, SeminarPostHistoryVersion>().ForMember(s => s.PostHistoryVersionId, e => e.MapFrom(s => s.PostId)).ReverseMap();

            CreateMap<PostMergeRequestInputDtoBase, PostMergeRequest>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.MergeState, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequestInputDto, PostMergeRequest>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.MergeState, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequestInputDto, Post>().ForMember(p => p.Id, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequest, PostMergeRequestOutputDtoBase>().ForMember(p => p.PostTitle, e => e.MapFrom(r => r.Post.Title));
            CreateMap<PostMergeRequest, PostMergeRequestOutputDto>().ForMember(p => p.PostTitle, e => e.MapFrom(r => r.Post.Title));
            CreateMap<PostMergeRequest, Post>().ForMember(p => p.Id, e => e.Ignore()).ReverseMap();
            CreateMap<Post, PostMergeRequestOutputDto>().ReverseMap();
        }
    }
}