using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Masuit.MyBlogs.Core.Common;

public sealed class ChallengeBypassTicket(IDataProtectionProvider dataProtectionProvider)
{
    private const string Payload = "challenge-passed";
    private static readonly TimeSpan MinimumLifetime = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan MaximumLifetime = TimeSpan.FromDays(1);
    private readonly ITimeLimitedDataProtector _protector = dataProtectionProvider.CreateProtector("Masuit.MyBlogs.ChallengeBypass.v1").ToTimeLimitedDataProtector();

    public bool IsValid(HttpRequest request)
    {
        var ticket = request.Cookies[SessionKey.ChallengeBypass];
        if (string.IsNullOrEmpty(ticket))
        {
            return false;
        }

        try
        {
            return string.Equals(_protector.Unprotect(ticket), Payload, StringComparison.Ordinal);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    public void Write(HttpResponse response)
    {
        var expires = DateTimeOffset.UtcNow.AddSeconds(RandomNumberGenerator.GetInt32((int)MinimumLifetime.TotalSeconds, (int)MaximumLifetime.TotalSeconds + 1));
        response.Cookies.Append(SessionKey.ChallengeBypass, _protector.Protect(Payload, expires), new CookieOptions
        {
            SameSite = SameSiteMode.Lax,
            Expires = expires,
            HttpOnly = true,
            Secure = true,
            Path = "/"
        });
    }

    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(SessionKey.ChallengeBypass, new CookieOptions
        {
            SameSite = SameSiteMode.Lax,
            HttpOnly = true,
            Secure = true,
            Path = "/"
        });
    }
}
