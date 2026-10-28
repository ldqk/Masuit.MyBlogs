namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface;

public interface IAccessViewTicketService
{
    string CreateTicket(string email);

    bool IsValid(string ticket);

    void Revoke(string email);
}
