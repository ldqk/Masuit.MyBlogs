using System.Collections.Generic;
using Models.DTO;

namespace Masuit.MyBlogs.WebApp.Models
{
    /// <summary>
    /// 页脚视图模型
    /// </summary>
    public class PageFootViewModel
    {
        /// <summary>
        /// 友情链接
        /// </summary>
        public IList<LinksOutputDto> Links { get; set; }
        /// <summary>
        /// 联系方式
        /// </summary>
        public IList<ContactsOutputDto> Contacts { get; set; }
    }
}