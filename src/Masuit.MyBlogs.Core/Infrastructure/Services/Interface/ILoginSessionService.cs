namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface;

public interface ILoginSessionService
{
    void AuthenticateRequest(HttpContext context);

    void SignIn(HttpContext context, UserInfoDto user, bool remember);

    void SignOut(HttpContext context);

    void RevokeUser(int userId);
}
