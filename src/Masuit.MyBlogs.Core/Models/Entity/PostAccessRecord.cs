using System;
using System.ComponentModel.DataAnnotations.Schema;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 文章访问记录
    /// </summary>
    [Table("PostAccessRecord")]
    public partial class PostAccessRecord : BaseEntity
    {
        public PostAccessRecord()
        {
            Status = Status.Default;
        }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        /// <summary>
        /// 访问时间
        /// </summary>
        public DateTime AccessTime { get; set; }

        /// <summary>
        /// 点击次数
        /// </summary>
        public int ClickCount { get; set; }

        public virtual Post Post { get; set; }
    }
}