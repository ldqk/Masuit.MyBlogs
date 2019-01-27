using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 导航菜单输入模型
    /// </summary>
    public class MenuInputDto : BaseEntity
    {
        public MenuInputDto()
        {
            ParentId = 0;
            Status = Status.Available;
        }

        /// <summary>
        /// 名字
        /// </summary>
        [Required(ErrorMessage = "菜单名不能为空！"), MaxLength(16, ErrorMessage = "菜单名最长支持16个字符！"), MinLength(2, ErrorMessage = "菜单名至少需要2个字符！")]
        public string Name { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [Required(ErrorMessage = "菜单的URL不能为空！"), StringLength(256, ErrorMessage = "URL最长支持256个字符！")]
        public string Url { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        [DefaultValue(0)]
        public int ParentId { get; set; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        [Required]
        public MenuType MenuType { get; set; }

        /// <summary>
        /// 是否在新标签页打开
        /// </summary>
        public bool NewTab { get; set; }
    }
}