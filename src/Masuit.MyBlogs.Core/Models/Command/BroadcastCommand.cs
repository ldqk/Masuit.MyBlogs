using Masuit.MyBlogs.Core.Models.Entity;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.Core.Validator;
using System;

namespace Masuit.MyBlogs.Core.Models.Command
{
    /// <summary>
    /// 访客订阅表
    /// </summary>
    public class BroadcastCommand : BaseEntity
    {
        public BroadcastCommand()
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
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 订阅类型
        /// </summary>
        public SubscribeType SubscribeType { get; set; }
    }
}