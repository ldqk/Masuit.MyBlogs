using System;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 文章实体输出模型
    /// </summary>
    public class PostOutputDto : BaseDto
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 文章关键词
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 受保护的内容
        /// </summary>
        public string ProtectContent { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        public int TotalViewCount { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public DateTime PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public bool IsFixedTop { get; set; }

        /// <summary>
        /// 资源名
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// 是否是Word文档
        /// </summary>
        public bool IsWordDocument { get; set; }

        /// <summary>
        /// 作者邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 修改人名字
        /// </summary>
        public string Modifier { get; set; }

        /// <summary>
        /// 修改人邮箱
        /// </summary>
        public string ModifierEmail { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 支持数
        /// </summary>
        public int VoteUpCount { get; set; }

        /// <summary>
        /// 反对数
        /// </summary>
        public int VoteDownCount { get; set; }

        ///// <summary>
        ///// 评论
        ///// </summary>
        //public virtual ICollection<CommentOutputDto> Comment { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 所属分类名
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 所属专题名
        /// </summary>
        public string Seminars { get; set; }

        /// <summary>
        /// 每日平均访问量
        /// </summary>
        public double AverageViewCount { get; set; }

    }
}