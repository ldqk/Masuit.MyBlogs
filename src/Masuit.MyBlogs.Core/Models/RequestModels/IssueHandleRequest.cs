namespace Masuit.MyBlogs.Core.Models.RequestModels
{
    /// <summary>
    /// bug处理
    /// </summary>
    public class IssueHandleRequest : RequestModelBase
    {
        /// <summary>
        /// 处理意见
        /// </summary>
        public string Text { get; set; }

    }
}