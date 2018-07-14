using System.Web.Mvc;
using AutoMapper;
using IBLL;
using Models.DTO;
using Models.Entity;

namespace Masuit.MyBlogs.WebApp.Controllers
{
    public class UserController : AdminController
    {
        public UserController(IUserInfoBll userInfoBll)
        {
            UserInfoBll = userInfoBll;
        }

        public ActionResult ChangeUsername(int id, string username)
        {
            UserInfo userInfo = UserInfoBll.GetById(id);
            if (!username.Equals(userInfo.Username) && UserInfoBll.UsernameExist(username))
            {
                return ResultData(null, false, $"用户名{username}已经存在，请尝试更换其他用户名！");
            }
            userInfo.Username = username;
            bool b = UserInfoBll.UpdateEntitySaved(userInfo);
            return ResultData(Mapper.Map<UserInfoOutputDto>(userInfo), b, b ? $"用户名修改成功，新用户名为{username}。" : "用户名修改失败！");
        }

        public ActionResult ChangeNickName(int id, string username)
        {
            UserInfo userInfo = UserInfoBll.GetById(id);
            userInfo.NickName = username;
            bool b = UserInfoBll.UpdateEntitySaved(userInfo);
            return ResultData(Mapper.Map<UserInfoOutputDto>(userInfo), b, b ? $"昵称修改成功，新昵称为{username}。" : "昵称修改失败！");
        }

        public ActionResult ChangePassword(int id, string old, string pwd, string pwd2)
        {
            if (pwd.Equals(pwd2))
            {
                bool b = UserInfoBll.ChangePassword(id, old, pwd);
                return ResultData(null, b, b ? $"密码修改成功，新密码为：{pwd}！" : "密码修改失败，可能是原密码不正确！");
            }
            return ResultData(null, false, "两次输入的密码不一致！");
        }

        public ActionResult ChangeAvatar(int id, string path)
        {
            UserInfo userInfo = UserInfoBll.GetById(id);
            userInfo.Avatar = path;
            bool b = UserInfoBll.UpdateEntitySaved(userInfo);
            return ResultData(Mapper.Map<UserInfoOutputDto>(userInfo), b, b ? $"头像修改成功。" : "头像修改失败！");
        }
    }
}