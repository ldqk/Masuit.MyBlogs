using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 文章修改请求
    /// </summary>
    public class PostMergeRequestDtoBase : BaseDto
    {
        /// <summary>
        /// 原文id
        /// </summary>
        public int PostId { get; set; }

        /// <summary>
        /// 原标题
        /// </summary>
        public string PostTitle { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public string Modifier { get; set; }

        /// <summary>
        /// 修改人邮箱
        /// </summary>
        public string ModifierEmail { get; set; }

        /// <summary>
        /// 合并状态
        /// </summary>
        public MergeStatus MergeState { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime SubmitTime { get; set; }

        /// <summary>
        /// 提交人IP
        /// </summary>
        public string IP { get; set; }
    }
}