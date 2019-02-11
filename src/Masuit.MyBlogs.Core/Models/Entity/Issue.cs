using Masuit.LuceneEFCore.SearchEngine;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 网站问题
    /// </summary>
    [Table("Issue")]
    public class Issue : BaseEntity
    {
        public Issue()
        {
            Status = Status.WaitingHandle;
            SubmitTime = DateTime.Now;
            Level = BugLevel.General;
        }

        /// <summary>
        /// 提交人昵称
        /// </summary>
        [Required(ErrorMessage = "昵称不能为空！")]
        public string Name { get; set; }

        /// <summary>
        /// 提交人邮箱
        /// </summary>
        [IsEmail]
        public string Email { get; set; }

        /// <summary>
        /// 问题标题
        /// </summary>
        [Required(ErrorMessage = "标题不能为空！"), LuceneIndex]
        public string Title { get; set; }

        /// <summary>
        /// 存在问题的页面链接
        /// </summary>
        [Required(ErrorMessage = "链接不能为空！"), LuceneIndex]
        public string Link { get; set; }

        /// <summary>
        /// 问题的详细描述
        /// </summary>
        [Required(ErrorMessage = "问题描述不能为空！"), SubmitCheck(20, 5000), LuceneIndex(IsHtml = true)]
        public string Description { get; set; }

        /// <summary>
        /// 问题严重级别
        /// </summary>
        [Required]
        public BugLevel Level { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime SubmitTime { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        public DateTime? HandleTime { get; set; }

        /// <summary>
        /// 开发者回信
        /// </summary>
        public string Msg { get; set; }

        /// <summary>
        /// 提交人IP
        /// </summary>
        public string IPAddress { get; set; }
    }

    /// <summary>
    /// 问题级别
    /// </summary>
    public enum BugLevel
    {
        [Display(Name = "一般")] General,
        [Display(Name = "严重")] Serious,
        [Display(Name = "异常")] Exception,
        [Display(Name = "致命")] Fatal
    }
}