namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 文章分类输出模型
    /// </summary>
    public class CategoryDto : BaseDto
    {
        /// <summary>
        /// 分类名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 分类描述
        /// </summary>
        public string Description { get; set; }

        public virtual int TotalPostCount { get; set; }
        public virtual int PendedPostCount { get; set; }
    }
}