using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.Validation;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 文章输入模型
    /// </summary>
    public class PostInputDto : BaseEntity
    {
        public PostInputDto()
        {
            PostDate = DateTime.Now;
            ModifyDate = DateTime.Now;
            IsFixedTop = false;
            Status = Status.Pending;
            IsWordDocument = false;
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "文章标题不能为空！"), MaxLength(128, ErrorMessage = "文章标题最长支持128个字符！"), MinLength(4, ErrorMessage = "文章标题最少4个字符！")]
        public string Title { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [Required, MaxLength(36, ErrorMessage = "作者名最长支持36个字符！"), MinLength(2, ErrorMessage = "作者名最少2个字符！")]
        public string Author { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "文章内容不能为空！"), SubmitCheck(20, 1000000, false)]
        public string Content { get; set; }

        /// <summary>
        /// 文章关键词
        /// </summary>
        [StringLength(256, ErrorMessage = "文章关键词最大允许255个字符")]
        public string Keyword { get; set; }

        /// <summary>
        /// 受保护的内容
        /// </summary>
        public string ProtectContent { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public DateTime PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        [DefaultValue(false)]
        public bool IsFixedTop { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 资源名
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// 是否是Word文档
        /// </summary>
        [DefaultValue(false)]
        public bool IsWordDocument { get; set; }

        /// <summary>
        /// 作者邮箱
        /// </summary>
        [Required(ErrorMessage = "作者邮箱不能为空！"), MinLength(6, ErrorMessage = "邮箱格式不正确！"), IsEmail]
        public string Email { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [StringLength(255, ErrorMessage = "标签最大允许255个字符")]
        public string Label { get; set; }

        /// <summary>
        /// 专题
        /// </summary>
        public string Seminars { get; set; }

    }
}