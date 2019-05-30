using Masuit.MyBlogs.Core.Common;

namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    /// <summary>
    /// 邮箱配置
    /// </summary>
    public static class EmailConfig
    {
        /// <summary>
        /// smtp服务器地址
        /// </summary>
        public static string Smtp => CommonHelper.SystemSettings["SMTP"];

        /// <summary>
        /// 发送邮箱用户名
        /// </summary>
        public static string SendFrom => CommonHelper.SystemSettings["EmailFrom"];

        /// <summary>
        /// 发送邮箱密码
        /// </summary>
        public static string EmailPwd => CommonHelper.SystemSettings["EmailPwd"];

        /// <summary>
        /// 收件人
        /// </summary>
        public static string ReceiveEmail => CommonHelper.SystemSettings["ReceiveEmail"];
    }
}