namespace Masuit.MyBlogs.Core.Common.Mails;

public interface IMailSender
{
    Task Send(string title, string content, string tos, string clientip);
}