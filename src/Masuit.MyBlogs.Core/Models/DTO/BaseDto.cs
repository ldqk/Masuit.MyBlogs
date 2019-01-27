using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// DTO基类
    /// </summary>
    public class BaseDto
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 条目状态
        /// </summary>
        public Status Status { get; set; }
    }
}