using System;

namespace Models.DTO
{
    /// <summary>
    /// 留言板输出模型
    /// </summary>
    public class LeaveMessageOutputDto : BaseDto
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public DateTime PostDate { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// QQ或微信
        /// </summary>
        public string QQorWechat { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public int ParentId { get; set; }

        /// <summary>
        /// 浏览器版本
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// 是否是博主
        /// </summary>
        public bool IsMaster { get; set; }
    }
}