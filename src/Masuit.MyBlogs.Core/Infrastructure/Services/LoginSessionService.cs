using System.Security.Cryptography;
using System.Text;
using FreeRedis;
using Masuit.MyBlogs.Core.Models;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed class LoginSessionService(IRedisClient redis, IUserInfoService users) : ILoginSessionService
{
    private const string CookieName = "RememberLogin";
    private const string TicketPrefix = "auth:remember:";
    private const string VersionPrefix = "auth:user:version:";
    private const string SessionStampKey = "auth.login.stamp";
    private const string SessionTicketKey = "auth.login.ticket";
    private static readonly TimeSpan TicketLifetime = TimeSpan.FromDays(365);

    public void AuthenticateRequest(HttpContext context)
    {
        var sessionUser = context.Session.Get<UserInfoDto>(SessionKey.UserInfo);
        if (sessionUser != null)
        {
            var stamp = context.Session.Get<string>(SessionStampKey);
            var sessionTicketKey = context.Session.Get<string>(SessionTicketKey);
            var sessionTicket = sessionTicketKey == null ? null : redis.Get<RememberTicket>(sessionTicketKey);
            if (stamp == CredentialStamp(sessionUser) && (sessionTicketKey == null || IsValidTicket(sessionTicket, sessionUser)))
            {
                return;
            }

            context.Session.Remove(SessionKey.UserInfo);
            context.Session.Remove(SessionStampKey);
            context.Session.Remove(SessionTicketKey);
        }

        if (!context.Request.Cookies.TryGetValue(CookieName, out var token))
        {
            return;
        }

        if (!IsValidToken(token))
        {
            DeleteRememberCookie(context);
            return;
        }

        var key = TicketKey(token);
        var ticket = redis.Get<RememberTicket>(key);
        var rememberedUser = ticket == null ? null : users.GetById(ticket.UserId).ToDto();
        if (!IsValidTicket(ticket, rememberedUser))
        {
            redis.Del(key);
            DeleteRememberCookie(context);
            return;
        }

        SetSession(context, rememberedUser, key);
    }

    public void SignIn(HttpContext context, UserInfoDto authenticatedUser, bool remember)
    {
        var user = users.GetById(authenticatedUser.Id) ?? throw new InvalidOperationException("登录用户不存在。");
        RevokeCurrentTicket(context);
        if (!remember || !user.IsAdmin)
        {
            SetSession(context, authenticatedUser, null);
            DeleteRememberCookie(context);
            return;
        }

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var expiresAt = DateTimeOffset.UtcNow.Add(TicketLifetime);
        var key = TicketKey(token);
        redis.Set(key, new RememberTicket(user.Id, CredentialStamp(authenticatedUser), expiresAt), TicketLifetime);
        SetSession(context, authenticatedUser, key);
        context.Response.Cookies.Append(CookieName, token, CookieOptions(expiresAt));
    }

    public void SignOut(HttpContext context)
    {
        RevokeCurrentTicket(context);
        DeleteRememberCookie(context);
        context.Session.Remove(SessionKey.UserInfo);
        context.Session.Remove(SessionStampKey);
        context.Session.Remove(SessionTicketKey);
    }

    public void RevokeUser(int userId)
    {
        redis.Set(VersionPrefix + userId, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
    }

    private bool IsValidTicket(RememberTicket ticket, UserInfoDto user)
    {
        return ticket != null && ticket.ExpiresAtUtc > DateTimeOffset.UtcNow && user?.IsAdmin == true && ticket.UserId == user.Id && ticket.CredentialStamp == CredentialStamp(user);
    }

    private void SetSession(HttpContext context, UserInfoDto user, string ticketKey)
    {
        context.Session.Set(SessionKey.UserInfo, user);
        context.Session.Set(SessionStampKey, CredentialStamp(user));
        if (ticketKey == null)
        {
            context.Session.Remove(SessionTicketKey);
        }
        else
        {
            context.Session.Set(SessionTicketKey, ticketKey);
        }
    }

    private void RevokeCurrentTicket(HttpContext context)
    {
        var sessionTicketKey = context.Session.Get<string>(SessionTicketKey);
        if (sessionTicketKey != null)
        {
            redis.Del(sessionTicketKey);
        }

        if (context.Request.Cookies.TryGetValue(CookieName, out var token) && IsValidToken(token))
        {
            redis.Del(TicketKey(token));
        }
    }

    private string CredentialStamp(UserInfoDto user)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(user.Email + "\0" + user.IsAdmin + "\0" + redis.Get<string>(VersionPrefix + user.Id))));
    }

    private static string TicketKey(string token)
    {
        return TicketPrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private static bool IsValidToken(string token)
    {
        return token is {Length: 43} && token.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
    }

    private static CookieOptions CookieOptions(DateTimeOffset expiresAt)
    {
        return new CookieOptions
        {
            Expires = expiresAt,
            SameSite = SameSiteMode.Lax,
            HttpOnly = true,
            Secure = true,
            Path = "/"
        };
    }

    private static void DeleteRememberCookie(HttpContext context)
    {
        context.Response.Cookies.Delete(CookieName, CookieOptions(DateTimeOffset.UnixEpoch));
    }

    private sealed record RememberTicket(int UserId, string CredentialStamp, DateTimeOffset ExpiresAtUtc);
}