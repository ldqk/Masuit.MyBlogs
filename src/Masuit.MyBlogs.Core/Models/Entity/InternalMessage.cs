using System;
using System.ComponentModel.DataAnnotations.Schema;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 站内信消息通知
    /// </summary>
    [Table("InternalMessage")]
    public class InternalMessage : BaseEntity
    {
        public InternalMessage()
        {
            Time = DateTime.Now;
        }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 消息链接
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// 消息时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 已读状态
        /// </summary>
        public bool Read { get; set; }
    }
}