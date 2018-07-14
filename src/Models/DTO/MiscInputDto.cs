using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Entity;
using Models.Enum;
using Models.Validation;

namespace Models.DTO
{
    /// <summary>
    /// 杂项页输入模型
    /// </summary>
    public class MiscInputDto : BaseEntity
    {
        public MiscInputDto()
        {
            PostDate = DateTime.Now;
            ModifyDate = DateTime.Now;
            Status = Status.Display;
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "标题不能为空！"), MaxLength(64, ErrorMessage = "标题最长支持64个字符！"), MinLength(4, ErrorMessage = "标题至少4个字符！")]
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "内容不能为空！"), SubmitCheck(100000, false)]
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