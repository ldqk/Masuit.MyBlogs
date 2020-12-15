using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Common.Mails
{
    public interface IMailSender
    {
        void Send(string title, string content, string tos);
        List<string> GetBounces();
        string AddRecipient(string email);
        public bool HasBounced(string address);
    }
}