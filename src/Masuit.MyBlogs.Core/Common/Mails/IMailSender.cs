namespace Masuit.MyBlogs.Core.Common.Mails;

public interface IMailSender
{
    Task Send(string title, string content, string tos, string clientip);

    Task<List<string>> GetBounces();

    Task<string> AddRecipient(string email);

    public Task<bool> HasBounced(string address);
}
