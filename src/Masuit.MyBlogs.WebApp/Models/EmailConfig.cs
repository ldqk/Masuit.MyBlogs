using Common;

namespace Masuit.MyBlogs.WebApp.Models
{
    /// <summary>
    /// 邮箱配置
    /// </summary>
    public static class EmailConfig
    {
        /// <summary>
        /// smtp服务器地址
        /// </summary>
        public static string Smtp { get; set; } = CommonHelper.GetSettings("smtp");

        /// <summary>
        /// 发送邮箱用户名
        /// </summary>
        public static string SendFrom { get; set; } = CommonHelper.GetSettings("EmailFrom");

        /// <summary>
        /// 发送邮箱密码
        /// </summary>
        public static string EmailPwd { get; set; } = CommonHelper.GetSettings("EmailPwd");

        /// <summary>
        /// 收件人
        /// </summary>
        public static string ReceiveEmail { get; set; } = CommonHelper.GetSettings("ReceiveEmail");
    }
}