namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface;

public partial interface IUserInfoService : IBaseService<UserInfo>
{
	/// <summary>
	/// 根据用户名获取
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	UserInfo GetByUsername(string name);

	/// <summary>
	/// 登录
	/// </summary>
	/// <param name="username"></param>
	/// <param name="password"></param>
	/// <returns></returns>
	UserInfoDto Login(string username, string password);

	/// <summary>
	/// 注册
	/// </summary>
	/// <param name="userInfo"></param>
	/// <returns></returns>
	UserInfo Register(UserInfo userInfo);

	/// <summary>
	/// 检查用户名是否存在
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	bool UsernameExist(string name);

	/// <summary>
	/// 检查邮箱是否存在
	/// </summary>
	/// <param name="email"></param>
	/// <returns></returns>
	bool EmailExist(string email);

	/// <summary>
	/// 修改密码
	/// </summary>
	/// <param name="name">用户名，邮箱或者电话号码</param>
	/// <param name="oldPwd">旧密码</param>
	/// <param name="newPwd">新密码</param>
	/// <returns></returns>
	bool ChangePassword(string name, string oldPwd, string newPwd);
	bool ChangePassword(int id, string oldPwd, string newPwd);

	/// <summary>
	/// 重置密码
	/// </summary>
	/// <returns></returns>
	bool ResetPassword(string name, string newPwd = "123456");

}