namespace Models.DTO
{
    /// <summary>
    /// 联系方式输出模型
    /// </summary>
    public class ContactsOutputDto : BaseDto
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; set; }
    }
}