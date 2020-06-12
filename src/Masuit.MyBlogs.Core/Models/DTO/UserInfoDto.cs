using Masuit.Tools.Core.Validator;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 用户信息输出模型
    /// </summary>
    public class UserInfoDto : BaseDto
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        public string Username { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空")]
        public string NickName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [IsEmail]
        public string Email { get; set; }

        /// <summary>
        /// 是否是管理员
        /// </summary>
        [DefaultValue(false)]
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// QQ或微信
        /// </summary>
        public string QQorWechat { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string Avatar { get; set; }
    }
}