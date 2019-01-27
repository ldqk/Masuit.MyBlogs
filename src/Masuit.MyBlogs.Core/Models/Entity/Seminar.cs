using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 文章专题
    /// </summary>
    [Table("Seminar")]
    public partial class Seminar : BaseEntity
    {
        public Seminar()
        {
            this.Post = new HashSet<SeminarPost>();
        }

        /// <summary>
        /// 专题名
        /// </summary>
        [Required(ErrorMessage = "专题名称不能为空！")]
        public string Title { get; set; }

        /// <summary>
        /// 专题子标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 专题描述
        /// </summary>
        [Required(ErrorMessage = "专题描述不能为空！")]
        public string Description { get; set; }

        public virtual ICollection<SeminarPost> Post { get; set; }
        public virtual ICollection<SeminarPostHistoryVersion> PostHistoryVersion { get; set; }
    }
}