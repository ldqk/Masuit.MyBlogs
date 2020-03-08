namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    /// <summary>
    /// 文章数据模型
    /// </summary>
    public class PostDataModel : PostModelBase
    {
        public string Status { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }

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

        public string CategoryName { get; set; }

        /// <summary>
        /// 修改次数
        /// </summary>
        public int ModifyCount { get; set; }

        /// <summary>
        /// 禁止评论
        /// </summary>
        public bool DisableComment { get; set; }

        /// <summary>
        /// 禁止转载
        /// </summary>
        public bool DisableCopy { get; set; }

    }
}