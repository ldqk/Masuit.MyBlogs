namespace Models.Enum
{
    /// <summary>
    /// 文章排序方式
    /// </summary>
    public enum OrderBy
    {
        /// <summary>
        /// 按发表时间
        /// </summary>
        PostDate,

        /// <summary>
        /// 按修改时间
        /// </summary>
        ModifyDate,

        /// <summary>
        /// 按访问次数
        /// </summary>
        ViewCount,

        /// <summary>
        /// 按评论数
        /// </summary>
        CommentCount,

        /// <summary>
        /// 按投票数
        /// </summary>
        VoteCount,
    }
}