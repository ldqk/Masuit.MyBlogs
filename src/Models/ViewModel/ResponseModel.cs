namespace Models.ViewModel
{
    /// <summary>
    /// 响应模型
    /// </summary>
    public class ResponseModel
    {
        /// <summary>
        /// 成功状态
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 登录状态
        /// </summary>
        public bool IsLogin { get; set; } = true;

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public object Data { get; set; }
    }
}