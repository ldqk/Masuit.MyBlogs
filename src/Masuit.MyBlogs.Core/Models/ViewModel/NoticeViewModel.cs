using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    /// <summary>
    /// 网站公告视图模型
    /// </summary>
    public class NoticeViewModel : BaseEntity
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public string PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string ModifyDate { get; set; }
    }
}