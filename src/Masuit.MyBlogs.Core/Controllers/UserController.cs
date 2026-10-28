using Masuit.MyBlogs.Core.Extensions;
using Masuit.Tools.AspNetCore.ModelBinder;
using System.Net;
using Masuit.MyBlogs.Core.Models;

namespace Masuit.MyBlogs.Core.Controllers;

/// <summary>
/// 用户管理
/// </summary>
public sealed class UserController : AdminController
{
    /// <summary>
    /// 修改用户名
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> ChangeUsername([FromBodyOrDefault] string username)
    {
        UserInfo userInfo = await UserInfoService.GetByIdAsync(CurrentUser.Id);
        if (!username.Equals(userInfo.Username) && UserInfoService.UsernameExist(username))
        {
            return ResultData(null, false, $"用户名{username}已经存在，请尝试更换其他用户名！");
        }

        userInfo.Username = username;
        bool b = await UserInfoService.SaveChangesAsync() > 0;
        LoginSessionService.SignOut(HttpContext);
        return ResultData(userInfo.ToDto(), b, b ? $"用户名修改成功，新用户名为{username}。" : "用户名修改失败！");
    }

    /// <summary>
    /// 修改昵称
    /// </summary>
    /// <param name="id"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> ChangeNickName([FromBodyOrDefault] string username)
    {
        UserInfo userInfo = await UserInfoService.GetByIdAsync(CurrentUser.Id);
        userInfo.NickName = username;
        bool b = await UserInfoService.SaveChangesAsync() > 0;
        return ResultData(userInfo.ToDto(), b, b ? $"昵称修改成功，新昵称为{username}。" : "昵称修改失败！");
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="old"></param>
    /// <param name="pwd"></param>
    /// <param name="pwd2"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> ChangePassword([FromBodyOrDefault] string old, [FromBodyOrDefault] string pwd, [FromBodyOrDefault] string pwd2)
    {
        if (pwd.Equals(pwd2))
        {
            bool b = UserInfoService.ChangePassword(CurrentUser.Id, old, pwd);
            if (b)
            {
                var user = await UserInfoService.GetByIdAsync(CurrentUser.Id);
                AccessViewTicketService.Revoke(user?.Email);
                LoginSessionService.SignOut(HttpContext);
            }

            return ResultData(null, b, b ? $"密码修改成功，新密码为：{pwd}！" : "密码修改失败，可能是原密码不正确！");
        }

        return ResultData(null, false, "两次输入的密码不一致！");
    }

    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="name">用户名</param>
    /// <param name="pwd"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> ResetPassword([FromBodyOrDefault] string name, [FromBodyOrDefault] string pwd)
    {
        var user = UserInfoService.GetByUsername(name);
        bool b = UserInfoService.ResetPassword(name, pwd);
        if (b)
        {
            AccessViewTicketService.Revoke(user?.Email);
        }

        return ResultData(null, b, b ? $"密码重置成功，新密码为：{pwd}！" : "密码重置失败！");
    }

    /// <summary>
    /// 修改头像
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<ActionResult> ChangeAvatar([FromBodyOrDefault] string path)
    {
        UserInfo userInfo = await UserInfoService.GetByIdAsync(CurrentUser.Id);
        userInfo.Avatar = path;
        bool b = await UserInfoService.SaveChangesAsync() > 0;
        return ResultData(userInfo.ToDto(), b, b ? "头像修改成功。" : "头像修改失败！");
    }

    /// <summary>
    /// 手动添加或修改用户
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost, DistributedLockFilter]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Save([FromBodyOrDefault] UserInfoDto model)
    {
        var userInfo = UserInfoService.GetByUsername(model.Username);
        if (userInfo is null)
        {
            userInfo = model.ToUserInfo();
            userInfo.Password = "123456";
            UserInfoService.Register(userInfo);
            return ResultData(null, true, "用户保存成功");
        }

        var previousEmail = userInfo.Email;
        var previousAdmin = userInfo.IsAdmin;
        model.Update(userInfo);
        if (previousAdmin != userInfo.IsAdmin)
        {
            LoginSessionService.RevokeUser(userInfo.Id);
        }
        var b = await UserInfoService.SaveChangesAsync() > 0;
        if (b && !string.Equals(previousEmail, userInfo.Email, StringComparison.OrdinalIgnoreCase))
        {
            AccessViewTicketService.Revoke(previousEmail);
        }

        return ResultData(null, b, b ? "用户保存成功" : "用户保存失败");
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost, DistributedLockFilter]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Delete([FromBodyOrDefault] int id)
    {
        var user = await UserInfoService.GetByIdAsync(id);
        LoginSessionService.RevokeUser(id);
        await UserInfoService.DeleteByIdAsync(id);
        AccessViewTicketService.Revoke(user?.Email);
        return ResultData(null);
    }

    /// <summary>
    /// 获取用户列表
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="search"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public IActionResult GetUsers(int page, int size, string search)
    {
        Expression<Func<UserInfo, bool>> where = info => true;
        if (!string.IsNullOrEmpty(search))
        {
            where = u => u.Username.Contains(search) || u.NickName.Contains(search) || u.Email.Contains(search) || u.QQorWechat.Contains(search);
        }

        var pages = UserInfoService.GetQuery(where, u => u.Id, false).ProjectDto().ToPagedListNoLock(page, size);
        return Ok(pages);
    }
}