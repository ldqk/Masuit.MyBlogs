using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Enum;

namespace Models.Entity
{
    /// <summary>
    /// 文章分类
    /// </summary>
    [Table("Category")]
    public class Category : BaseEntity
    {
        public Category()
        {
            Post = new HashSet<Post>();
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

        public virtual ICollection<Post> Post { get; set; }
        public virtual ICollection<PostHistoryVersion> PostHistoryVersion { get; set; }
    }
}