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
    /// <param name="id"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<ActionResult> ChangeUsername([FromBodyOrDefault] int id, [FromBodyOrDefault] string username)
    {
        UserInfo userInfo = await UserInfoService.GetByIdAsync(id);
        if (!username.Equals(userInfo.Username) && UserInfoService.UsernameExist(username))
        {
            return ResultData(null, false, $"用户名{username}已经存在，请尝试更换其他用户名！");
        }

        userInfo.Username = username;
        bool b = await UserInfoService.SaveChangesAsync() > 0;
        return ResultData(userInfo.ToDto(), b, b ? $"用户名修改成功，新用户名为{username}。" : "用户名修改失败！");
    }

    /// <summary>
    /// 修改昵称
    /// </summary>
    /// <param name="id"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<ActionResult> ChangeNickName([FromBodyOrDefault] int id, [FromBodyOrDefault] string username)
    {
        UserInfo userInfo = await UserInfoService.GetByIdAsync(id);
        userInfo.NickName = username;
        bool b = await UserInfoService.SaveChangesAsync() > 0;
        return ResultData(userInfo.ToDto(), b, b ? $"昵称修改成功，新昵称为{username}。" : "昵称修改失败！");
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    /// <param name="id"></param>
    /// <param name="old"></param>
    /// <param name="pwd"></param>
    /// <param name="pwd2"></param>
    /// <returns></returns>
    public ActionResult ChangePassword([FromBodyOrDefault] int id, [FromBodyOrDefault] string old, [FromBodyOrDefault] string pwd, [FromBodyOrDefault] string pwd2)
    {
        if (pwd.Equals(pwd2))
        {
            bool b = UserInfoService.ChangePassword(id, old, pwd);
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
    public ActionResult ResetPassword([FromBodyOrDefault] string name, [FromBodyOrDefault] string pwd)
    {
        bool b = UserInfoService.ResetPassword(name, pwd);
        return ResultData(null, b, b ? $"密码重置成功，新密码为：{pwd}！" : "密码重置失败！");
    }

    /// <summary>
    /// 修改头像
    /// </summary>
    /// <param name="id"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async Task<ActionResult> ChangeAvatar([FromBodyOrDefault] int id, [FromBodyOrDefault] string path)
    {
        UserInfo userInfo = await UserInfoService.GetByIdAsync(id);
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

        model.Update(userInfo);
        var b = await UserInfoService.SaveChangesAsync() > 0;
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
        await UserInfoService.DeleteByIdAsync(id);
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