using System;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Enum;
using Models.Validation;

namespace Models.Entity
{
    /// <summary>
    /// 订阅表
    /// </summary>
    [Table("Broadcast")]
    public class Broadcast : BaseEntity
    {
        public Broadcast()
        {
            Status = Status.Subscribing;
            UpdateTime = DateTime.Now;
        }

        /// <summary>
        /// 订阅接收邮箱
        /// </summary>
        [IsEmail]
        public string Email { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string ValidateCode { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 订阅类型
        /// </summary>
        public SubscribeType SubscribeType { get; set; }
    }
}