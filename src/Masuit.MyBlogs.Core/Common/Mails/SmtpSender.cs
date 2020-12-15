using Masuit.Tools;
using Masuit.Tools.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Masuit.MyBlogs.Core.Common.Mails
{
    public class SmtpSender : IMailSender
    {
        public void Send(string title, string content, string tos)
        {
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
        }

        public List<string> GetBounces()
        {
            return File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "email-bounces.txt"), Encoding.UTF8).Split(',').ToList();
        }

        public string AddRecipient(string email)
        {
            var bounces = GetBounces();
            bounces.Add(email);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "email-bounces.txt"), bounces.Join(","));
            return "添加成功";
        }

        public bool HasBounced(string address)
        {
            return GetBounces().Contains(address);
        }
    }
}