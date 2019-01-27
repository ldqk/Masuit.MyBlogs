namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 搜索统计输出模型
    /// </summary>
    public class KeywordsRankOutputDto : BaseDto
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string KeyWords { get; set; }

        /// <summary>
        /// 搜索次数
        /// </summary>
        public long? SearchCount { get; set; }
    }
}