using System.ComponentModel.DataAnnotations;
using Masuit.MyBlogs.Core.Models.Validation;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 文章修改请求
    /// </summary>
    public class PostMergeRequestInputDtoBase : BaseDto
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "文章标题不能为空！"), MaxLength(128, ErrorMessage = "文章标题最长支持128个字符！"), MinLength(4, ErrorMessage = "文章标题最少4个字符！")]
        public string Title { get; set; }

        /// <summary>
        /// 文章内容
        /// </summary>
        [Required(ErrorMessage = "文章内容不能为空！"), SubmitCheck(20, 1000000, false)]
        public string Content { get; set; }
    }
}