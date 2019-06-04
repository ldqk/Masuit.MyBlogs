using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 捐赠表
    /// </summary>
    [Table("Donate")]
    public class Donate : BaseEntity
    {
        /// <summary>
        /// 捐赠人
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 捐赠人邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 捐赠人的QQ或微信
        /// </summary>
        public string QQorWechat { get; set; }

        /// <summary>
        /// 捐赠人的显式邮箱
        /// </summary>
        public string EmailDisplay { get; set; }

        /// <summary>
        /// 捐赠人的显式QQ或微信
        /// </summary>
        public string QQorWechatDisplay { get; set; }

        /// <summary>
        /// 捐赠金额
        /// </summary>
        public string Amount { get; set; }

        /// <summary>
        /// 捐赠途径
        /// </summary>
        public string Via { get; set; }

        /// <summary>
        /// 捐赠时间
        /// </summary>
        public DateTime DonateTime { get; set; }
    }
}