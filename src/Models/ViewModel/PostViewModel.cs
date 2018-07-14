using System;
using System.Collections.Generic;
using Models.Entity;

namespace Models.ViewModel
{
    /// <summary>
    /// 文章视图模型
    /// </summary>
    public class PostViewModel : BaseEntity
    {
        public PostViewModel()
        {
            Comment = new HashSet<CommentViewModel>();
        }

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
        /// 分类id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 浏览次数
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public string PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string ModifyDate { get; set; }

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

        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public virtual ICollection<CommentViewModel> Comment { get; set; }

        /// <summary>
        /// 所属分类名
        /// </summary>
        public string CategoryName { get; set; }
    }
}