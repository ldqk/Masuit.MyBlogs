using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.Validation;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.Command
{
    /// <summary>
    /// 杂项页输入模型
    /// </summary>
    public class MiscCommand : BaseEntity
    {
        public MiscCommand()
        {
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
    }
}