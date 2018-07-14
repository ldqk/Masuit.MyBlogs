using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Entity;
using Models.Enum;
using Models.Validation;

namespace Models.DTO
{
    /// <summary>
    /// 留言板输入模型
    /// </summary>
    public class LeaveMessageInputDto : BaseEntity
    {
        public LeaveMessageInputDto()
        {
            PostDate = DateTime.Now;
            Status = Status.Pending;
            IsMaster = false;
        }

        /// <summary>
        /// 昵称
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空！"), MaxLength(24, ErrorMessage = "昵称最大支持24个字符"), MinLength(2, ErrorMessage = "昵称至少2个字")]
        public string NickName { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "留言内容不能为空！"), SubmitCheck]
        public string Content { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime PostDate { get; set; }

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
        /// 是否是博主
        /// </summary>
        [DefaultValue(false)]
        public bool IsMaster { get; set; }

        /// <summary>
        /// 提交人IP地址
        /// </summary>
        public string IP { get; set; }
    }
}