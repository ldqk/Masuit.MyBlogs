using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.MyBlogs.Core.Common;
using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Security;
using System;

namespace Masuit.MyBlogs.Core.Infrastructure.Services
{
    public partial class UserInfoService : BaseService<UserInfo>, IUserInfoService
    {
        /// <summary>
        /// 根据用户名获取
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public UserInfo GetByUsername(string name)
        {
            return Get(u => u.Username.Equals(name) || u.Email.Equals(name));
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserInfoDto Login(string username, string password)
        {
            UserInfo userInfo = GetByUsername(username);
            if (userInfo != null)
            {
                UserInfoDto user = userInfo.Mapper<UserInfoDto>();
                string key = userInfo.SaltKey;
                string pwd = userInfo.Password;
                password = password.MDString3(key);
                if (pwd.Equals(password))
                {
                    return user;
                }
            }
            return null;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        public UserInfo Register(UserInfo userInfo)
        {
            UserInfo exist = Get(u => u.Username.Equals(userInfo.Username) || u.Email.Equals(userInfo.Email));
            if (exist is null)
            {
                var salt = $"{new Random().StrictNext()}{DateTime.Now.GetTotalMilliseconds()}".MDString2(Guid.NewGuid().ToString()).AESEncrypt();
                userInfo.Password = userInfo.Password.MDString3(salt);
                userInfo.SaltKey = salt;
                UserInfo added = AddEntity(userInfo);
                int line = SaveChanges();
                return line > 0 ? added : null;
            }
            return null;
        }

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool UsernameExist(string name)
        {
            UserInfo userInfo = GetByUsername(name);
            return userInfo != null;
        }

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool EmailExist(string email) => GetNoTracking(u => u.Email.Equals(email)) != null;

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="name">用户名，邮箱或者电话号码</param>
        /// <param name="oldPwd">旧密码</param>
        /// <param name="newPwd">新密码</param>
        /// <returns></returns>
        public bool ChangePassword(string name, string oldPwd, string newPwd)
        {
            UserInfo userInfo = GetByUsername(name);
            if (userInfo != null)
            {
                string key = userInfo.SaltKey;
                string pwd = userInfo.Password;
                oldPwd = oldPwd.MDString3(key);
                if (pwd.Equals(oldPwd))
                {
                    var salt = $"{new Random().StrictNext()}{DateTime.Now.GetTotalMilliseconds()}".MDString2(Guid.NewGuid().ToString()).AESEncrypt();
                    userInfo.Password = newPwd.MDString3(salt);
                    userInfo.SaltKey = salt;
                    return SaveChanges() > 0;
                }
            }
            return false;
        }

        public bool ChangePassword(int id, string oldPwd, string newPwd)
        {
            UserInfo userInfo = GetById(id);
            if (userInfo != null)
            {
                string key = userInfo.SaltKey;
                string pwd = userInfo.Password;
                oldPwd = oldPwd.MDString3(key);
                if (pwd.Equals(oldPwd))
                {
                    var salt = $"{new Random().StrictNext()}{DateTime.Now.GetTotalMilliseconds()}".MDString2(Guid.NewGuid().ToString()).AESEncrypt();
                    userInfo.Password = newPwd.MDString3(salt);
                    userInfo.SaltKey = salt;
                    return SaveChanges() > 0;
                }
            }
            return false;
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <returns></returns>
        public bool ResetPassword(string name, string newPwd = "123456")
        {
            UserInfo userInfo = GetByUsername(name);
            if (userInfo != null)
            {
                var salt = $"{new Random().StrictNext()}{DateTime.Now.GetTotalMilliseconds()}".MDString2(Guid.NewGuid().ToString()).AESEncrypt();
                userInfo.Password = newPwd.MDString3(salt);
                userInfo.SaltKey = salt;
                return SaveChanges() > 0;
            }
            return false;
        }

        public UserInfoService(IBaseRepository<UserInfo> repository, ISearchEngine<DataContext> searchEngine, ILuceneIndexSearcher searcher) : base(repository, searchEngine, searcher)
        {
        }
    }
}