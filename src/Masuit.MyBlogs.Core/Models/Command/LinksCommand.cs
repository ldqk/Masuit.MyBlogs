using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.Command
{
    /// <summary>
    /// 友情链接
    /// </summary>
    public class LinksCommand : BaseEntity
    {
        public LinksCommand()
        {
            Status = Status.Available;
            Except = false;
        }

        /// <summary>
        /// 名字
        /// </summary>
        [Required(ErrorMessage = "站点名不能为空！"), MaxLength(16, ErrorMessage = "站点名称最长只能是16个字符！"), MinLength(2, ErrorMessage = "站点名称至少2个字")]
        public string Name { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        [Required(ErrorMessage = "站点的URL不能为空！"), StringLength(256, ErrorMessage = "URL最长支持256个字符！")]
        public string Url { get; set; }

        /// <summary>
        /// 是否检测白名单
        /// </summary>
        public bool Except { get; set; }
    }
}