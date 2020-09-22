using Masuit.Tools;
using Masuit.Tools.Models;

namespace Masuit.MyBlogs.Core.Common
{
    public class SmtpSender : IMailSender
    {
        public void Send(string title, string content, string tos)
        {
#if !DEBUG
            new Email()
            {
                EnableSsl = bool.Parse(CommonHelper.SystemSettings.GetOrAdd("EnableSsl", "true")),
                Body = content,
                SmtpServer = CommonHelper.SystemSettings["SMTP"],
                Username = CommonHelper.SystemSettings["EmailFrom"],
                Password = CommonHelper.SystemSettings["EmailPwd"],
                SmtpPort = CommonHelper.SystemSettings["SmtpPort"].ToInt32(),
                Subject = title,
                Tos = tos
            }.Send();
#endif
        }
    }
}