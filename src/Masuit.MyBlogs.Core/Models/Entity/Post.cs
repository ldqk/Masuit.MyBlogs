using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 文章
    /// </summary>
    [Table("Post")]
    public class Post : BaseEntity
    {
        public Post()
        {
            Comment = new HashSet<Comment>();
            PostDate = DateTime.Now;
            ModifyDate = DateTime.Now;
            IsFixedTop = false;
            Status = Status.Pending;
            IsWordDocument = false;
            Seminar = new HashSet<SeminarPost>();
            PostAccessRecord = new HashSet<PostAccessRecord>();
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "文章标题不能为空！")]
        public string Title { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [Required, MaxLength(24, ErrorMessage = "作者名最长支持24个字符！")]
        public string Author { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "文章内容不能为空！"), SubmitCheck(20, 1000000, false)]
        public string Content { get; set; }

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
        [Required(ErrorMessage = "作者邮箱不能为空！"), IsEmail]
        public string Email { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [StringLength(256, ErrorMessage = "标签最大允许255个字符")]
        public string Label { get; set; }

        /// <summary>
        /// 文章关键词
        /// </summary>
        [StringLength(256, ErrorMessage = "文章关键词最大允许255个字符")]
        public string Keyword { get; set; }

        /// <summary>
        /// 支持数
        /// </summary>
        [DefaultValue(0)]
        public int VoteUpCount { get; set; }

        /// <summary>
        /// 反对数
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Computed), DefaultValue(0)]
        public int VoteDownCount { get; set; }

        /// <summary>
        /// 是否是头图文章
        /// </summary>
        [Required]
        [DefaultValue(false)]
        public bool IsBanner { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(255)]
        public string Description { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [StringLength(255)]
        public string ImageUrl { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public virtual Category Category { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public virtual ICollection<Comment> Comment { get; set; }

        /// <summary>
        /// 专题
        /// </summary>
        public virtual ICollection<SeminarPost> Seminar { get; set; }

        /// <summary>
        /// 点击记录
        /// </summary>
        public virtual ICollection<PostAccessRecord> PostAccessRecord { get; set; }

        /// <summary>
        /// 文章历史版本
        /// </summary>
        public virtual ICollection<PostHistoryVersion> PostHistoryVersion { get; set; }
    }
}