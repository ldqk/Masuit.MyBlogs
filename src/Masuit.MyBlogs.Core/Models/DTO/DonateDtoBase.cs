using System;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    /// <summary>
    /// 打赏表
    /// </summary>
    public class DonateDtoBase : BaseDto
    {
        /// <summary>
        /// 打赏人
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 打赏人的显式邮箱
        /// </summary>
        public string EmailDisplay { get; set; }

        /// <summary>
        /// 打赏人的显式QQ或微信
        /// </summary>
        public string QQorWechatDisplay { get; set; }

        /// <summary>
        /// 打赏金额
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// 打赏途径
        /// </summary>
        public string Via { get; set; }

        /// <summary>
        /// 打赏时间
        /// </summary>
        public DateTime DonateTime { get; set; }
    }
}