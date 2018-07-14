using System.ComponentModel.DataAnnotations;
using Models.Entity;
using Models.Enum;

namespace Models.DTO
{
    /// <summary>
    /// 文章分类输入模型
    /// </summary>
    public class CategoryInputDto : BaseEntity
    {
        public CategoryInputDto()
        {
            Status = Status.Available;
        }

        /// <summary>
        /// 分类名
        /// </summary>
        [Required(ErrorMessage = "分类名不能为空"), MaxLength(64, ErrorMessage = "分类名最大允许64个字符"), MinLength(2, ErrorMessage = "分类名至少2个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 分类描述
        /// </summary>
        public string Description { get; set; }
    }
}