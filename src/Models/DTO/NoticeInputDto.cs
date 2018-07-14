using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Entity;
using Models.Enum;
using Models.Validation;

namespace Models.DTO
{
    /// <summary>
    /// 网站公告输入模型
    /// </summary>
    public class NoticeInputDto : BaseEntity
    {
        public NoticeInputDto()
        {
            PostDate = DateTime.Now;
            ModifyDate = DateTime.Now;
            Status = Status.Display;
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "公告标题不能为空！"), MaxLength(64, ErrorMessage = "公告标题最长64个字符！"), MinLength(2, ErrorMessage = "公告标题至少2个字符")]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "公告内容不能为空！"), SubmitCheck(3000, false)]
        public string Content { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Column(TypeName = "datetime2")]
        public DateTime ModifyDate { get; set; }
    }
}