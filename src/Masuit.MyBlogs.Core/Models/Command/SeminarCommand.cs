using System.ComponentModel.DataAnnotations;
using Masuit.MyBlogs.Core.Models.DTO;

namespace Masuit.MyBlogs.Core.Models.Command
{
    /// <summary>
    /// 文章专题输入模型
    /// </summary>
    public partial class SeminarCommand : BaseDto
    {
        /// <summary>
        /// 专题名称
        /// </summary>
        [Required(ErrorMessage = "专题名称不能为空！"), MinLength(2, ErrorMessage = "专题名称至少2个字符！"), MaxLength(16, ErrorMessage = "专题名称最多16个字符！")]
        public string Title { get; set; }

        /// <summary>
        /// 专题子标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 专题描述
        /// </summary>
        [Required(ErrorMessage = "专题描述不能为空！"), MinLength(15, ErrorMessage = "专题描述至少15个字符！"), MaxLength(1500, ErrorMessage = "专题描述最多1500个字符！")]
        public string Description { get; set; }
    }
}