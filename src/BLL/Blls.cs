
using IBLL;
using Models.Entity;
namespace BLL
{
	
	public partial class BroadcastBll :BaseBll<Broadcast>,IBroadcastBll{}   
	
	public partial class CategoryBll :BaseBll<Category>,ICategoryBll{}   
	
	public partial class CommentBll :BaseBll<Comment>,ICommentBll{}   
	
	public partial class ContactsBll :BaseBll<Contacts>,IContactsBll{}   
	
	public partial class DonateBll :BaseBll<Donate>,IDonateBll{}   
	
	public partial class InternalMessageBll :BaseBll<InternalMessage>,IInternalMessageBll{}   
	
	public partial class InterviewBll :BaseBll<Interview>,IInterviewBll{}   
	
	public partial class InterviewDetailBll :BaseBll<InterviewDetail>,IInterviewDetailBll{}   
	
	public partial class IssueBll :BaseBll<Issue>,IIssueBll{}   
	
	public partial class LeaveMessageBll :BaseBll<LeaveMessage>,ILeaveMessageBll{}   
	
	public partial class LinksBll :BaseBll<Links>,ILinksBll{}   
	
	public partial class LoginRecordBll :BaseBll<LoginRecord>,ILoginRecordBll{}   
	
	public partial class MenuBll :BaseBll<Menu>,IMenuBll{}   
	
	public partial class MiscBll :BaseBll<Misc>,IMiscBll{}   
	
	public partial class NoticeBll :BaseBll<Notice>,INoticeBll{}   
	
	public partial class PostBll :BaseBll<Post>,IPostBll{}   
	
	public partial class PostAccessRecordBll :BaseBll<PostAccessRecord>,IPostAccessRecordBll{}   
	
	public partial class PostHistoryVersionBll :BaseBll<PostHistoryVersion>,IPostHistoryVersionBll{}   
	
	public partial class SearchDetailsBll :BaseBll<SearchDetails>,ISearchDetailsBll{}   
	
	public partial class SeminarBll :BaseBll<Seminar>,ISeminarBll{}   
	
	public partial class SystemSettingBll :BaseBll<SystemSetting>,ISystemSettingBll{}   
	
	public partial class UserInfoBll :BaseBll<UserInfo>,IUserInfoBll{}   
}