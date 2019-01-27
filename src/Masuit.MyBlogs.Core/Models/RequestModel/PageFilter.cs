namespace Masuit.MyBlogs.Core.Models.RequestModel
{
    /// <summary>
    /// 分页筛选
    /// </summary>
    public class PageFilter
    {
        /// <summary>
        /// 第几页
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 页大小
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// 关键词
        /// </summary>
        public string Kw { get; set; }
    }
}