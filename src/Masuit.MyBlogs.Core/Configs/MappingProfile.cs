using AutoMapper;
using Masuit.MyBlogs.Core.Models.Command;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Systems;
using System;
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
            CreateMap<Broadcast, BroadcastCommand>().ReverseMap();
            CreateMap<Broadcast, BroadcastDto>().ReverseMap();
            CreateMap<BroadcastDto, BroadcastCommand>().ReverseMap();

            CreateMap<Category, CategoryCommand>().ReverseMap();
            CreateMap<Category, CategoryDto>().ForMember(c => c.TotalPostCount, e => e.MapFrom(c => c.Post.Count)).ForMember(c => c.PendedPostCount, e => e.MapFrom(c => c.Post.Count())).ReverseMap();
            CreateMap<CategoryCommand, CategoryDto>().ReverseMap();

            CreateMap<CommentCommand, Comment>().ForMember(c => c.Status, e => e.MapFrom(c => Status.Pending)).ReverseMap();
            CreateMap<Comment, CommentDto>().ReverseMap();
            CreateMap<CommentCommand, CommentDto>().ReverseMap();
            CreateMap<Comment, CommentViewModel>().ForMember(c => c.CommentDate, e => e.MapFrom(c => c.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<LeaveMessageCommand, LeaveMessage>().ForMember(c => c.Status, e => e.MapFrom(c => Status.Pending)).ReverseMap();
            CreateMap<LeaveMessage, LeaveMessageDto>().ReverseMap();
            CreateMap<LeaveMessageCommand, LeaveMessageDto>().ReverseMap();
            CreateMap<LeaveMessage, LeaveMessageViewModel>().ForMember(l => l.PostDate, e => e.MapFrom(l => l.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<Links, LinksCommand>().ReverseMap();
            CreateMap<Links, LinksDto>().ReverseMap();
            CreateMap<LinksCommand, LinksDto>().ReverseMap();

            CreateMap<Menu, MenuCommand>().ReverseMap();
            CreateMap<Menu, MenuDto>().ReverseMap();
            CreateMap<MenuCommand, MenuDto>().ReverseMap();

            CreateMap<Misc, MiscCommand>().ReverseMap();
            CreateMap<Misc, MiscDto>().ReverseMap();
            CreateMap<MiscCommand, MiscDto>().ReverseMap();
            CreateMap<Misc, MiscViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<Notice, NoticeCommand>().ReverseMap();
            CreateMap<Notice, NoticeDto>().ReverseMap();
            CreateMap<NoticeCommand, NoticeDto>().ReverseMap();
            CreateMap<Notice, NoticeViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

            CreateMap<Post, PostCommand>().ReverseMap();
            CreateMap<Post, PostModelBase>();
            CreateMap<Post, PostHistoryVersion>().ForMember(p => p.Id, e => e.Ignore()).ForMember(v => v.PostId, e => e.MapFrom(p => p.Id));
            CreateMap<Post, PostDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.CommentCount, e => e.MapFrom(p => p.Comment.Count(c => c.Status == Status.Pended))).ReverseMap();
            CreateMap<PostCommand, PostDto>().ReverseMap();
            CreateMap<PostHistoryVersion, PostDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ReverseMap();
            CreateMap<Post, PostViewModel>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.PostDate, e => e.MapFrom(p => p.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(p => p.ModifyDate, e => e.MapFrom(p => p.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();
            CreateMap<Post, PostDataModel>().ForMember(p => p.ModifyDate, e => e.MapFrom(p => p.ModifyDate.ToString("yyyy-MM-dd HH:mm"))).ForMember(p => p.PostDate, e => e.MapFrom(p => p.PostDate.ToString("yyyy-MM-dd HH:mm"))).ForMember(p => p.Status, e => e.MapFrom(p => p.Status.GetDisplay())).ForMember(p => p.ModifyCount, e => e.MapFrom(p => p.PostHistoryVersion.Count)).ForMember(p => p.ViewCount, e => e.MapFrom(p => p.TotalViewCount));

            CreateMap<SearchDetails, SearchDetailsCommand>().ReverseMap();
            CreateMap<SearchDetails, SearchDetailsDto>().ReverseMap();
            CreateMap<SearchDetailsCommand, SearchDetailsDto>().ReverseMap();

            CreateMap<UserInfo, UserInfoCommand>().ReverseMap();
            CreateMap<UserInfo, UserInfoDto>().ReverseMap();
            CreateMap<UserInfoCommand, UserInfoDto>().ReverseMap();

            CreateMap<LoginRecord, LoginRecordViewModel>().ReverseMap();

            CreateMap<Seminar, SeminarCommand>().ReverseMap();
            CreateMap<Seminar, SeminarDto>().ReverseMap();
            CreateMap<SeminarCommand, SeminarDto>().ReverseMap();

            CreateMap<SeminarPost, SeminarPostHistoryVersion>().ForMember(s => s.PostHistoryVersionId, e => e.MapFrom(s => s.PostId)).ReverseMap();

            CreateMap<PostMergeRequestCommandBase, PostMergeRequest>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.MergeState, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequestCommand, PostMergeRequest>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.MergeState, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequestCommand, Post>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.Status, e => e.Ignore()).ReverseMap();
            CreateMap<PostMergeRequest, PostMergeRequestDtoBase>().ForMember(p => p.PostTitle, e => e.MapFrom(r => r.Post.Title));
            CreateMap<PostMergeRequest, PostMergeRequestDto>().ForMember(p => p.PostTitle, e => e.MapFrom(r => r.Post.Title));
            CreateMap<PostMergeRequest, Post>().ForMember(p => p.Id, e => e.Ignore()).ForMember(p => p.Status, e => e.Ignore()).ReverseMap();
            CreateMap<Post, PostMergeRequestDto>().ReverseMap();

            CreateMap<Advertisement, AdvertisementViewModel>();
            CreateMap<AdvertisementDto, Advertisement>().ForMember(a => a.Status, e => e.Ignore()).ForMember(a => a.UpdateTime, e => e.MapFrom(a => DateTime.Now));

            CreateMap<Donate, DonateDto>();
            CreateMap<Donate, DonateDtoBase>();
        }
    }
}