using Masuit.LuceneEFCore.SearchEngine;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.MyBlogs.Core.Models.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 文章
    /// </summary>
    [Table("Post")]
    public class Post : BaseEntity
    {
        public Post()
        {
            Comment = new HashSet<Comment>();
            PostDate = DateTime.Now;
            ModifyDate = DateTime.Now;
            IsFixedTop = false;
            Status = Status.Pending;
            Seminar = new HashSet<Seminar>();
            PostMergeRequests = new HashSet<PostMergeRequest>();
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "文章标题不能为空！"), LuceneIndex]
        public string Title { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [Required, MaxLength(24, ErrorMessage = "作者名最长支持24个字符！"), LuceneIndex]
        public string Author { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "文章内容不能为空！"), SubmitCheck(20, 1000000, false), LuceneIndex(IsHtml = true)]
        public string Content { get; set; }

        /// <summary>
        /// 受保护的内容
        /// </summary>
        [LuceneIndex(IsHtml = true)]
        public string ProtectContent { get; set; }

        /// <summary>
        /// 受保护内容模式
        /// </summary>
        public ProtectContentMode ProtectContentMode { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public DateTime PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [LuceneIndex]
        public DateTime ModifyDate { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        [DefaultValue(false)]
        public bool IsFixedTop { get; set; }

        /// <summary>
        /// 分类id
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// 作者邮箱
        /// </summary>
        [Required(ErrorMessage = "作者邮箱不能为空！"), EmailAddress, LuceneIndex]
        public string Email { get; set; }

        /// <summary>
        /// 修改人名字
        /// </summary>
        [LuceneIndex]
        public string Modifier { get; set; }

        /// <summary>
        /// 修改人邮箱
        /// </summary>
        [LuceneIndex]
        public string ModifierEmail { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [StringLength(256, ErrorMessage = "标签最大允许255个字符"), LuceneIndex]
        public string Label { get; set; }

        /// <summary>
        /// 文章关键词
        /// </summary>
        [StringLength(256, ErrorMessage = "文章关键词最大允许255个字符"), LuceneIndex]
        public string Keyword { get; set; }

        /// <summary>
        /// 支持数
        /// </summary>
        [DefaultValue(0), ConcurrencyCheck]
        public int VoteUpCount { get; set; }

        /// <summary>
        /// 反对数
        /// </summary>
        [DefaultValue(0), ConcurrencyCheck]
        public int VoteDownCount { get; set; }

        /// <summary>
        /// 每日平均访问量
        /// </summary>
        [ConcurrencyCheck]
        public double AverageViewCount { get; set; }

        /// <summary>
        /// 总访问量
        /// </summary>
        [ConcurrencyCheck]
        public int TotalViewCount { get; set; }

        /// <summary>
        /// 提交人IP地址
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// 禁止评论
        /// </summary>
        public bool DisableComment { get; set; }

        /// <summary>
        /// 禁止转载
        /// </summary>
        public bool DisableCopy { get; set; }

        /// <summary>
        /// 限制模式
        /// </summary>
        public RegionLimitMode? LimitMode { get; set; }

        /// <summary>
        /// 限制地区，竖线分隔
        /// </summary>
        public string Regions { get; set; }

        /// <summary>
        /// 限制排除地区，竖线分隔
        /// </summary>
        public string ExceptRegions { get; set; }

        /// <summary>
        /// 限制模式
        /// </summary>
        public RegionLimitMode? ProtectContentLimitMode { get; set; }

        /// <summary>
        /// 限制地区，竖线分隔
        /// </summary>
        public string ProtectContentRegions { get; set; }

        /// <summary>
        /// 保护密码
        /// </summary>
        public string ProtectPassword { get; set; }

        /// <summary>
        /// 开启rss订阅
        /// </summary>
        public bool Rss { get; set; }

        /// <summary>
        /// 锁定编辑
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// 重定向到第三方链接
        /// </summary>
        public string Redirect { get; set; }

        /// <summary>
        /// 分类
        /// </summary>
        public virtual Category Category { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public virtual ICollection<Comment> Comment { get; set; }

        /// <summary>
        /// 专题
        /// </summary>
        public virtual ICollection<Seminar> Seminar { get; set; }

        /// <summary>
        /// 文章历史版本
        /// </summary>
        public virtual ICollection<PostHistoryVersion> PostHistoryVersion { get; set; }

        /// <summary>
        /// 文章修改请求
        /// </summary>
        public virtual ICollection<PostMergeRequest> PostMergeRequests { get; set; }

        /// <summary>
        /// 访问记录
        /// </summary>
        public virtual ICollection<PostVisitRecord> PostVisitRecords { get; set; }

        /// <summary>
        /// 访问记录统计
        /// </summary>
        public virtual ICollection<PostVisitRecordStats> PostVisitRecordStats { get; set; }
    }

    /// <summary>
    /// 地区限制
    /// </summary>
    public enum RegionLimitMode
    {
        [Description("不限")]
        All,

        [Description("指定地区可见：{0}")]
        AllowRegion,

        [Description("指定地区不可见：{0}")]
        ForbidRegion,

        [Description("可见地区：{0}，排除地区：{1}")]
        AllowRegionExceptForbidRegion,

        [Description("不可见地区：{0}，排除地区：{1}")]
        ForbidRegionExceptAllowRegion,

        [Description("仅搜索引擎可见")]
        OnlyForSearchEngine,
    }

    /// <summary>
    /// 受保护内容模式
    /// </summary>
    public enum ProtectContentMode
    {
        None,

        /// <summary>
        /// 评论可见
        /// </summary>
        CommentVisiable,

        /// <summary>
        /// 地区可见
        /// </summary>
        Regions,

        /// <summary>
        /// 授权可见
        /// </summary>
        AuthorizeVisiable,

        /// <summary>
        /// 密码可见
        /// </summary>
        Password,

        /// <summary>
        /// 仅搜索引擎可见
        /// </summary>
        OnlyForSearchEngine,
    }
}
