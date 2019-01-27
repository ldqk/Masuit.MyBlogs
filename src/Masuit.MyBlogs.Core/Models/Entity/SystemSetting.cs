using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 系统设置
    /// </summary>
    [Table("SystemSetting")]
    public class SystemSetting : BaseEntity
    {
        public SystemSetting()
        {
            Status = Status.Available;
        }
        /// <summary>
        /// 参数项名
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [Required]
        public string Value { get; set; }

    }
}