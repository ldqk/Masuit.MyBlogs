namespace Masuit.MyBlogs.Core.Common
{
    public interface IMailSender
    {
        void Send(string title, string content, string tos);
    }
}