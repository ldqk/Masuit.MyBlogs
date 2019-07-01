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
    public static class RegisterAutomapper
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Excute()
        {
            Mapper.Initialize(m =>
            {
                m.CreateMap<Broadcast, BroadcastInputDto>().ReverseMap();
                m.CreateMap<Broadcast, BroadcastOutputDto>().ReverseMap();
                m.CreateMap<BroadcastOutputDto, BroadcastInputDto>().ReverseMap();

                m.CreateMap<Category, CategoryInputDto>().ReverseMap();
                m.CreateMap<Category, CategoryOutputDto>().ForMember(c => c.TotalPostCount, e => e.MapFrom(c => c.Post.Count)).ForMember(c => c.PendedPostCount, e => e.MapFrom(c => c.Post.Count(p => p.Status == Status.Pended))).ReverseMap();
                m.CreateMap<CategoryInputDto, CategoryOutputDto>().ReverseMap();

                m.CreateMap<Comment, CommentInputDto>().ReverseMap();
                m.CreateMap<Comment, CommentOutputDto>().ReverseMap();
                m.CreateMap<CommentInputDto, CommentOutputDto>().ReverseMap();
                m.CreateMap<Comment, CommentViewModel>().ForMember(c => c.CommentDate, e => e.MapFrom(c => c.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

                m.CreateMap<LeaveMessage, LeaveMessageInputDto>().ReverseMap();
                m.CreateMap<LeaveMessage, LeaveMessageOutputDto>().ReverseMap();
                m.CreateMap<LeaveMessageInputDto, LeaveMessageOutputDto>().ReverseMap();
                m.CreateMap<LeaveMessage, LeaveMessageViewModel>().ForMember(l => l.PostDate, e => e.MapFrom(l => l.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

                m.CreateMap<Links, LinksInputDto>().ReverseMap();
                m.CreateMap<Links, LinksOutputDto>().ReverseMap();
                m.CreateMap<LinksInputDto, LinksOutputDto>().ReverseMap();

                m.CreateMap<Menu, MenuInputDto>().ReverseMap();
                m.CreateMap<Menu, MenuOutputDto>().ReverseMap();
                m.CreateMap<MenuInputDto, MenuOutputDto>().ReverseMap();

                m.CreateMap<Misc, MiscInputDto>().ReverseMap();
                m.CreateMap<Misc, MiscOutputDto>().ReverseMap();
                m.CreateMap<MiscInputDto, MiscOutputDto>().ReverseMap();
                m.CreateMap<Misc, MiscViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

                m.CreateMap<Notice, NoticeInputDto>().ReverseMap();
                m.CreateMap<Notice, NoticeOutputDto>().ReverseMap();
                m.CreateMap<NoticeInputDto, NoticeOutputDto>().ReverseMap();
                m.CreateMap<Notice, NoticeViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

                m.CreateMap<Post, PostInputDto>().ReverseMap();
                m.CreateMap<Post, PostModelBase>();
                m.CreateMap<Post, PostHistoryVersion>().ForMember(v => v.PostId, e => e.MapFrom(p => p.Id));
                m.CreateMap<Post, PostOutputDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.CommentCount, e => e.MapFrom(p => p.Comment.Count(c => c.Status == Status.Pended))).ReverseMap();
                m.CreateMap<PostInputDto, PostOutputDto>().ReverseMap();
                m.CreateMap<PostHistoryVersion, PostOutputDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ReverseMap();
                m.CreateMap<Post, PostViewModel>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.PostDate, e => e.MapFrom(p => p.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(p => p.ModifyDate, e => e.MapFrom(p => p.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss"))).ReverseMap();

                m.CreateMap<SearchDetails, SearchDetailsInputDto>().ReverseMap();
                m.CreateMap<SearchDetails, SearchDetailsOutputDto>().ReverseMap();
                m.CreateMap<SearchDetailsInputDto, SearchDetailsOutputDto>().ReverseMap();

                m.CreateMap<UserInfo, UserInfoInputDto>().ReverseMap();
                m.CreateMap<UserInfo, UserInfoOutputDto>().ReverseMap();
                m.CreateMap<UserInfoInputDto, UserInfoOutputDto>().ReverseMap();

                m.CreateMap<LoginRecord, LoginRecordOutputDto>().ReverseMap();

                m.CreateMap<Seminar, SeminarInputDto>().ReverseMap();
                m.CreateMap<Seminar, SeminarOutputDto>().ReverseMap();
                m.CreateMap<SeminarInputDto, SeminarOutputDto>().ReverseMap();

                m.CreateMap<SeminarPost, SeminarPostHistoryVersion>().ForMember(s => s.PostHistoryVersionId, e => e.MapFrom(s => s.PostId)).ReverseMap();
            });
        }
    }
}