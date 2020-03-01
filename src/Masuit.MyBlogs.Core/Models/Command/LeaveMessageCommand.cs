using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.Validation;
using Masuit.Tools.Core.Validator;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.Command
{
    /// <summary>
    /// 留言板输入模型
    /// </summary>
    public class LeaveMessageCommand : BaseEntity
    {
        public LeaveMessageCommand()
        {
            Status = Status.Pending;
        }

        /// <summary>
        /// 昵称
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空！"), MaxLength(36, ErrorMessage = "昵称最大支持36个字符"), MinLength(2, ErrorMessage = "昵称至少2个字")]
        public string NickName { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "留言内容不能为空！"), SubmitCheck(2, 500)]
        public string Content { get; set; }

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
        /// 父级ID
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 浏览器版本
        /// </summary>
        [StringLength(255)]
        public string Browser { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        [StringLength(255)]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// 是否已接受条款
        /// </summary>
        [AssignTrue(ErrorMessage = "请先同意接受本站的《留言须知》")]
        public bool Agree { get; set; }
    }
}