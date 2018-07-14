
using IDAL;
using Models.Entity;
using System;
namespace DAL
{
	public partial class BroadcastDal :BaseDal<Broadcast>,IBroadcastDal{}
	public partial class CategoryDal :BaseDal<Category>,ICategoryDal{}
	public partial class CommentDal :BaseDal<Comment>,ICommentDal{}
	public partial class ContactsDal :BaseDal<Contacts>,IContactsDal{}
	public partial class DonateDal :BaseDal<Donate>,IDonateDal{}
	public partial class InternalMessageDal :BaseDal<InternalMessage>,IInternalMessageDal{}
	public partial class InterviewDal :BaseDal<Interview>,IInterviewDal{}
	public partial class InterviewDetailDal :BaseDal<InterviewDetail>,IInterviewDetailDal{}
	public partial class IssueDal :BaseDal<Issue>,IIssueDal{}
	public partial class LeaveMessageDal :BaseDal<LeaveMessage>,ILeaveMessageDal{}
	public partial class LinksDal :BaseDal<Links>,ILinksDal{}
	public partial class LoginRecordDal :BaseDal<LoginRecord>,ILoginRecordDal{}
	public partial class MenuDal :BaseDal<Menu>,IMenuDal{}
	public partial class MiscDal :BaseDal<Misc>,IMiscDal{}
	public partial class NoticeDal :BaseDal<Notice>,INoticeDal{}
	public partial class PostDal :BaseDal<Post>,IPostDal{}
	public partial class PostAccessRecordDal :BaseDal<PostAccessRecord>,IPostAccessRecordDal{}
	public partial class PostHistoryVersionDal :BaseDal<PostHistoryVersion>,IPostHistoryVersionDal{}
	public partial class SearchDetailsDal :BaseDal<SearchDetails>,ISearchDetailsDal{}
	public partial class SeminarDal :BaseDal<Seminar>,ISeminarDal{}
	public partial class SystemSettingDal :BaseDal<SystemSetting>,ISystemSettingDal{}
	public partial class UserInfoDal :BaseDal<UserInfo>,IUserInfoDal{}
}