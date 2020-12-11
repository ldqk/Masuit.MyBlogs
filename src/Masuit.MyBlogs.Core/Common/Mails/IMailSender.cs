using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Common.Mails
{
    public interface IMailSender
    {
        void Send(string title, string content, string tos);
        List<string> GetBounces();
        string AddBounces(string email);
    }
}