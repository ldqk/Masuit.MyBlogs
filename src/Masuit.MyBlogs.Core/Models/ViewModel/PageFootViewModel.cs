using Masuit.MyBlogs.Core.Models.DTO;
using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    /// <summary>
    /// 页脚视图模型
    /// </summary>
    public class PageFootViewModel
    {
        /// <summary>
        /// 友情链接
        /// </summary>
        public IList<LinksDto> Links { get; set; }
    }
}