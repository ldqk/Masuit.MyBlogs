using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Masuit.MyBlogs.Core.Common;

public sealed class EmailVerificationTicket(IDataProtectionProvider dataProtectionProvider)
{
    public const string CookieName = "ValidateKey";
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);
    private readonly ITimeLimitedDataProtector _protector = dataProtectionProvider
        .CreateProtector("Masuit.MyBlogs.EmailVerification.v1")
        .ToTimeLimitedDataProtector();

    public string GetEmail(HttpRequest request)
    {
        var ticket = request.Cookies[CookieName];
        if (string.IsNullOrEmpty(ticket))
        {
            return null;
        }

        try
        {
            return _protector.Unprotect(ticket);
        }
        catch (CryptographicException)
        {
            return null;
        }
    }

    public void Write(HttpResponse response, string email)
    {
        var expires = DateTimeOffset.UtcNow.Add(Lifetime);
        response.Cookies.Append(CookieName, _protector.Protect(email, expires), new CookieOptions
        {
            Expires = expires,
            SameSite = SameSiteMode.Lax,
            HttpOnly = true,
            Secure = true,
            Path = "/"
        });
    }

    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(CookieName, new CookieOptions { Path = "/", Secure = true, HttpOnly = true, SameSite = SameSiteMode.Lax });
    }
}
