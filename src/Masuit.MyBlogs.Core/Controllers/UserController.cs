using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Masuit.MyBlogs.Core.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserController : AdminController
    {
        /// <summary>
        /// 修改用户名
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public ActionResult ChangeUsername(int id, string username)
        {
            UserInfo userInfo = UserInfoService.GetById(id);
            if (!username.Equals(userInfo.Username) && UserInfoService.UsernameExist(username))
            {
                return ResultData(null, false, $"用户名{username}已经存在，请尝试更换其他用户名！");
            }
            userInfo.Username = username;
            bool b = UserInfoService.UpdateEntitySaved(userInfo);
            return ResultData(Mapper.Map<UserInfoOutputDto>(userInfo), b, b ? $"用户名修改成功，新用户名为{username}。" : "用户名修改失败！");
        }

        /// <summary>
        /// 修改昵称
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public ActionResult ChangeNickName(int id, string username)
        {
            UserInfo userInfo = UserInfoService.GetById(id);
            userInfo.NickName = username;
            bool b = UserInfoService.UpdateEntitySaved(userInfo);
            return ResultData(Mapper.Map<UserInfoOutputDto>(userInfo), b, b ? $"昵称修改成功，新昵称为{username}。" : "昵称修改失败！");
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="old"></param>
        /// <param name="pwd"></param>
        /// <param name="pwd2"></param>
        /// <returns></returns>
        public ActionResult ChangePassword(int id, string old, string pwd, string pwd2)
        {
            if (pwd.Equals(pwd2))
            {
                bool b = UserInfoService.ChangePassword(id, old, pwd);
                return ResultData(null, b, b ? $"密码修改成功，新密码为：{pwd}！" : "密码修改失败，可能是原密码不正确！");
            }
            return ResultData(null, false, "两次输入的密码不一致！");
        }

        /// <summary>
        /// 修改头像
        /// </summary>
        /// <param name="id"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public ActionResult ChangeAvatar(int id, string path)
        {
            UserInfo userInfo = UserInfoService.GetById(id);
            userInfo.Avatar = path;
            bool b = UserInfoService.UpdateEntitySaved(userInfo);
            return ResultData(Mapper.Map<UserInfoOutputDto>(userInfo), b, b ? $"头像修改成功。" : "头像修改失败！");
        }
    }
}