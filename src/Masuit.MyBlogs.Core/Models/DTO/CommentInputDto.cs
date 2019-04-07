using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.Validation;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 评论表输入模型
    /// </summary>
    public class CommentInputDto : BaseEntity
    {
        public CommentInputDto()
        {
            Status = Status.Pending;
            IsMaster = false;
        }
        /// <summary>
        /// 昵称
        /// </summary>
        [Required(ErrorMessage = "既然要评论，不留名怎么行呢！"), MaxLength(36, ErrorMessage = "别闹，你这名字太长了吧！"), MinLength(2, ErrorMessage = "昵称至少2个字！")]
        public string NickName { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [IsEmail]
        public string Email { get; set; }

        /// <summary>
        /// QQ或微信
        /// </summary>
        [StringLength(32, ErrorMessage = "QQ或微信不合法")]
        public string QQorWechat { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        [Required(ErrorMessage = "评论内容不能为空！"), SubmitCheck(2, 500)]
        public string Content { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 文章ID
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public DateTime CommentDate { get; set; }

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
        /// 支持数
        /// </summary>
        public int VoteCount { get; set; }

        /// <summary>
        /// 反对数
        /// </summary>
        public int AgainstCount { get; set; }

        /// <summary>
        /// 访问者IP
        /// </summary>
        public string IP { get; set; }

    }

}