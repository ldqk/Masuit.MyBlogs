namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 打赏表
    /// </summary>
    public class DonateDto : DonateDtoBase
    {

        /// <summary>
        /// 打赏人邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 打赏人的QQ或微信
        /// </summary>
        public string QQorWechat { get; set; }

    }
}