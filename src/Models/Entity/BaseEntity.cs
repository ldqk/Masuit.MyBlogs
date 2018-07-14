using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Models.Enum;

namespace Models.Entity
{
    /// <summary>
    /// 基类型
    /// </summary>
    public class BaseEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        [Key]
        public int Id { get; set; }

        [DefaultValue(Status.Default)]
        public Status Status { get; set; }
    }
}