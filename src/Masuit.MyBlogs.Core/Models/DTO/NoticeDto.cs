using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 网站公告输出模型
    /// </summary>
    public class NoticeDto : BaseDto
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
        public DateTime PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// 访问次数
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 公告状态
        /// </summary>
        public NoticeStatus NoticeStatus { get; set; }
    }
}