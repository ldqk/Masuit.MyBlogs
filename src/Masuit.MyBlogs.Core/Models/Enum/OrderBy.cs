using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.Enum
{
    /// <summary>
    /// 文章排序方式
    /// </summary>
    public enum OrderBy
    {
        /// <summary>
        /// 按发表时间
        /// </summary>
        [Display(Name = nameof(PostDate))]
        PostDate,

        /// <summary>
        /// 按修改时间
        /// </summary>
        [Display(Name = nameof(ModifyDate))]
        ModifyDate,

        /// <summary>
        /// 按访问次数
        /// </summary>
        [Display(Name = nameof(TotalViewCount))]
        TotalViewCount,

        /// <summary>
        /// 按评论数
        /// </summary>
        [Display(Name = nameof(CommentCount))]
        CommentCount,

        /// <summary>
        /// 按投票数
        /// </summary>
        [Display(Name = nameof(VoteUpCount))]
        VoteUpCount,

        /// <summary>
        /// 每日平均访问量
        /// </summary>
        [Display(Name = nameof(AverageViewCount))]
        AverageViewCount,
    }
}