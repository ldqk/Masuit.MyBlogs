using Models.Entity;

namespace Models.ViewModel
{
    /// <summary>
    /// 杂项页视图模型
    /// </summary>
    public class MiscViewModel : BaseEntity
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
        public string PostDate { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public string ModifyDate { get; set; }
    }
}