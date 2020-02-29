namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 文章专题输出模型
    /// </summary>
    public partial class SeminarDto : BaseDto
    {
        /// <summary>
        /// 专题名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 专题子标题
        /// </summary>
        public string SubTitle { get; set; }

        /// <summary>
        /// 专题描述
        /// </summary>
        public string Description { get; set; }
    }
}