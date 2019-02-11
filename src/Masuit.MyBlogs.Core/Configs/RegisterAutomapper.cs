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
                m.CreateMap<Broadcast, BroadcastInputDto>();
                m.CreateMap<BroadcastInputDto, Broadcast>();
                m.CreateMap<Broadcast, BroadcastOutputDto>();
                m.CreateMap<BroadcastOutputDto, Broadcast>();
                m.CreateMap<BroadcastOutputDto, BroadcastInputDto>();
                m.CreateMap<BroadcastInputDto, BroadcastOutputDto>();

                m.CreateMap<Category, CategoryInputDto>();
                m.CreateMap<CategoryInputDto, Category>();
                m.CreateMap<Category, CategoryOutputDto>().ForMember(c => c.TotalPostCount, e => e.MapFrom(c => c.Post.Count)).ForMember(c => c.PendedPostCount, e => e.MapFrom(c => c.Post.Count(p => p.Status == Status.Pended)));
                m.CreateMap<CategoryOutputDto, Category>();
                m.CreateMap<CategoryInputDto, CategoryOutputDto>();
                m.CreateMap<CategoryOutputDto, CategoryInputDto>();

                m.CreateMap<Comment, CommentInputDto>();
                m.CreateMap<CommentInputDto, Comment>();
                m.CreateMap<Comment, CommentOutputDto>();
                m.CreateMap<CommentOutputDto, Comment>();
                m.CreateMap<CommentInputDto, CommentOutputDto>();
                m.CreateMap<CommentOutputDto, CommentInputDto>();
                m.CreateMap<Comment, CommentViewModel>().ForMember(c => c.CommentDate, e => e.MapFrom(c => c.CommentDate.ToString("yyyy-MM-dd HH:mm:ss")));

                m.CreateMap<Contacts, ContactsInputDto>();
                m.CreateMap<ContactsInputDto, Contacts>();
                m.CreateMap<Contacts, ContactsOutputDto>();
                m.CreateMap<ContactsOutputDto, Contacts>();
                m.CreateMap<ContactsInputDto, ContactsOutputDto>();
                m.CreateMap<ContactsOutputDto, ContactsInputDto>();

                m.CreateMap<LeaveMessage, LeaveMessageInputDto>();
                m.CreateMap<LeaveMessageInputDto, LeaveMessage>();
                m.CreateMap<LeaveMessage, LeaveMessageOutputDto>();
                m.CreateMap<LeaveMessageOutputDto, LeaveMessage>();
                m.CreateMap<LeaveMessageInputDto, LeaveMessageOutputDto>();
                m.CreateMap<LeaveMessageOutputDto, LeaveMessageInputDto>();
                m.CreateMap<LeaveMessage, LeaveMessageViewModel>().ForMember(l => l.PostDate, e => e.MapFrom(l => l.PostDate.ToString("yyyy-MM-dd HH:mm:ss")));

                m.CreateMap<Links, LinksInputDto>();
                m.CreateMap<LinksInputDto, Links>();
                m.CreateMap<Links, LinksOutputDto>();
                m.CreateMap<LinksOutputDto, Links>();
                m.CreateMap<LinksInputDto, LinksOutputDto>();
                m.CreateMap<LinksOutputDto, LinksInputDto>();

                m.CreateMap<Menu, MenuInputDto>();
                m.CreateMap<MenuInputDto, Menu>();
                m.CreateMap<Menu, MenuOutputDto>();
                m.CreateMap<MenuOutputDto, Menu>();
                m.CreateMap<MenuInputDto, MenuOutputDto>();
                m.CreateMap<MenuOutputDto, MenuInputDto>();

                m.CreateMap<Misc, MiscInputDto>();
                m.CreateMap<MiscInputDto, Misc>();
                m.CreateMap<Misc, MiscOutputDto>();
                m.CreateMap<MiscOutputDto, Misc>();
                m.CreateMap<MiscInputDto, MiscOutputDto>();
                m.CreateMap<MiscOutputDto, MiscInputDto>();
                m.CreateMap<Misc, MiscViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss")));

                m.CreateMap<Notice, NoticeInputDto>();
                m.CreateMap<NoticeInputDto, Notice>();
                m.CreateMap<Notice, NoticeOutputDto>();
                m.CreateMap<NoticeOutputDto, Notice>();
                m.CreateMap<NoticeInputDto, NoticeOutputDto>();
                m.CreateMap<NoticeOutputDto, NoticeInputDto>();
                m.CreateMap<Notice, NoticeViewModel>().ForMember(c => c.PostDate, e => e.MapFrom(c => c.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(c => c.ModifyDate, e => e.MapFrom(c => c.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss")));

                m.CreateMap<Post, PostInputDto>();
                m.CreateMap<Post, PostHistoryVersion>().ForMember(v => v.PostId, e => e.MapFrom(p => p.Id));
                m.CreateMap<PostInputDto, Post>();
                m.CreateMap<Post, PostOutputDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name));
                m.CreateMap<PostOutputDto, Post>();
                m.CreateMap<PostInputDto, PostOutputDto>();
                m.CreateMap<PostHistoryVersion, PostOutputDto>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name));
                m.CreateMap<PostOutputDto, PostInputDto>();
                m.CreateMap<Post, PostViewModel>().ForMember(p => p.CategoryName, e => e.MapFrom(p => p.Category.Name)).ForMember(p => p.PostDate, e => e.MapFrom(p => p.PostDate.ToString("yyyy-MM-dd HH:mm:ss"))).ForMember(p => p.ModifyDate, e => e.MapFrom(p => p.ModifyDate.ToString("yyyy-MM-dd HH:mm:ss")));

                m.CreateMap<SearchDetails, SearchDetailsInputDto>();
                m.CreateMap<SearchDetailsInputDto, SearchDetails>();
                m.CreateMap<SearchDetails, SearchDetailsOutputDto>();
                m.CreateMap<SearchDetailsOutputDto, SearchDetails>();
                m.CreateMap<SearchDetailsInputDto, SearchDetailsOutputDto>();
                m.CreateMap<SearchDetailsOutputDto, SearchDetailsInputDto>();

                m.CreateMap<UserInfo, UserInfoInputDto>();
                m.CreateMap<UserInfoInputDto, UserInfo>();
                m.CreateMap<UserInfo, UserInfoOutputDto>();
                m.CreateMap<UserInfoOutputDto, UserInfo>();
                m.CreateMap<UserInfoInputDto, UserInfoOutputDto>();
                m.CreateMap<UserInfoOutputDto, UserInfoInputDto>();

                m.CreateMap<LoginRecord, LoginRecordOutputDto>();
                m.CreateMap<LoginRecordOutputDto, LoginRecord>();

                m.CreateMap<Seminar, SeminarInputDto>();
                m.CreateMap<SeminarInputDto, Seminar>();
                m.CreateMap<Seminar, SeminarOutputDto>();
                m.CreateMap<SeminarOutputDto, Seminar>();
                m.CreateMap<SeminarInputDto, SeminarOutputDto>();
                m.CreateMap<SeminarOutputDto, SeminarInputDto>();

                m.CreateMap<SeminarPost, SeminarPostHistoryVersion>().ForMember(s => s.PostHistoryVersionId, e => e.MapFrom(s => s.PostId)).ReverseMap();
            });
        }
    }
}