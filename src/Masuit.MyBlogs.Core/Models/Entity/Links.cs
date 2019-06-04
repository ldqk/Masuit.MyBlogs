using Masuit.MyBlogs.Core.Models.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 友情链接
    /// </summary>
    [Table("Links")]
    public class Links : BaseEntity
    {
        public Links()
        {
            Status = Status.Available;
            Except = false;
        }

        /// <summary>
        /// 名字
        /// </summary>
        [Required(ErrorMessage = "站点名不能为空！")]
        public string Name { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [Required(ErrorMessage = "站点的URL不能为空！")]
        public string Url { get; set; }

        /// <summary>
        /// 是否检测白名单
        /// </summary>
        public bool Except { get; set; }

        /// <summary>
        /// 是否是推荐站点
        /// </summary>
        public bool Recommend { get; set; }
    }
}