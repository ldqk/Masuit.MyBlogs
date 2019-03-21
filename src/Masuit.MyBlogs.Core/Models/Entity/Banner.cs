using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// banner
    /// </summary>
    [Table("Banner")]
    public class Banner : BaseEntity
    {
        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "Banner标题不能为空！")]
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Required(ErrorMessage = "banner描述不能为空！"), StringLength(255)]
        public string Description { get; set; }

        /// <summary>
        /// banner指向地址
        /// </summary>
        [Required(ErrorMessage = "banner目标地址不能为空！"), StringLength(255)]
        public string Url { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        [Required(ErrorMessage = "banner图片不能为空！"), StringLength(255)]
        public string ImageUrl { get; set; }
    }
}