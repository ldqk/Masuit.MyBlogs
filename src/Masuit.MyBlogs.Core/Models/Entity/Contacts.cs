using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 联系方式
    /// </summary>
    [Table("Contacts")]
    public class Contacts : BaseEntity
    {
        public Contacts()
        {
            Status = Status.Available;
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "标题不能为空")]
        public string Title { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [Required(ErrorMessage = "URL不能为空")]
        public string Url { get; set; }
    }
}