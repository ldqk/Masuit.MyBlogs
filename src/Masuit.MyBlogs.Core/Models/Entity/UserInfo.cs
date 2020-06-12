using Masuit.Tools.Core.Validator;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 用户
    /// </summary>
    [Table("UserInfo")]
    public class UserInfo : BaseEntity
    {
        public UserInfo()
        {
            IsAdmin = false;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空！")]
        public string Username { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空！")]
        public string NickName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不能为空！")]
        public string Password { get; set; }

        /// <summary>
        /// 加密盐
        /// </summary>
        [Required]
        public string SaltKey { get; set; }

        /// <summary>
        /// 是否是管理员
        /// </summary>
        [DefaultValue(false)]
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [IsEmail]
        public string Email { get; set; }

        /// <summary>
        /// QQ或微信
        /// </summary>
        public string QQorWechat { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string Avatar { get; set; }

        public virtual ICollection<LoginRecord> LoginRecord { get; set; }
    }
}