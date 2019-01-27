using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 联系方式输入模型
    /// </summary>
    public class ContactsInputDto : BaseEntity
    {
        public ContactsInputDto()
        {
            Status = Status.Available;
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "标题不能为空"), MaxLength(255, ErrorMessage = "标题最长支持255个字符"), MinLength(4, ErrorMessage = "标题至少4个字")]
        public string Title { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [Required(ErrorMessage = "URL不能为空"), MaxLength(255, ErrorMessage = "URL最长支持255个字符")]
        public string Url { get; set; }
    }
}