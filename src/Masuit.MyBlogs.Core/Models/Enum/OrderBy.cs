using System.ComponentModel;
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
        [Description("最后发表时间")]
        [Display(Name = nameof(PostDate))]
        PostDate,

        /// <summary>
        /// 按修改时间
        /// </summary>
        [Description("最后修改时间")]
        [Display(Name = nameof(ModifyDate))]
        ModifyDate,

        /// <summary>
        /// 按访问次数
        /// </summary>
        [Description("访问量最多")]
        [Display(Name = nameof(TotalViewCount))]
        TotalViewCount,

        /// <summary>
        /// 按评论数
        /// </summary>
        [Description("评论最多")]
        [Display(Name = "Comment.Count")]
        CommentCount,

        /// <summary>
        /// 按投票数
        /// </summary>
        [Description("支持最多")]
        [Display(Name = nameof(VoteUpCount))]
        VoteUpCount,

        /// <summary>
        /// 每日平均访问量
        /// </summary>
        [Description("最热门")]
        [Display(Name = nameof(AverageViewCount))]
        AverageViewCount,

        /// <summary>
        /// 今日热榜
        /// </summary>
        [Description("今日热榜")]
        [Display(Name = nameof(Trending))]
        Trending,
    }
}