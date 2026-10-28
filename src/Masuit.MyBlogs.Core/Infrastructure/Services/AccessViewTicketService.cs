using System.Security.Cryptography;
using System.Text;
using FreeRedis;
using Masuit.MyBlogs.Core.Infrastructure.Services.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Services;

public sealed class AccessViewTicketService(IRedisClient redisClient, IUserInfoService userInfoService) : IAccessViewTicketService
{
    private const int TicketLifetimeSeconds = 24 * 60 * 60*30;
    private const string TicketKeyPrefix = "access:view:ticket:";
    private const string VersionKeyPrefix = "access:view:version:";

    public string CreateTicket(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !userInfoService.Any(user => user.Email == email))
        {
            return null;
        }

        var version = redisClient.Get<string>(GetVersionKey(email)) ?? string.Empty;
        var ticket = CreateRandomToken();
        redisClient.Set(TicketKeyPrefix + ticket, new AccessViewTicket(email, version, DateTimeOffset.UtcNow.AddSeconds(TicketLifetimeSeconds)), TicketLifetimeSeconds);
        return ticket;
    }

    public bool IsValid(string ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return false;
        }

        var grant = redisClient.Get<AccessViewTicket>(TicketKeyPrefix + ticket);
        return grant != null
            && grant.ExpiresAtUtc > DateTimeOffset.UtcNow
            && string.Equals(redisClient.Get<string>(GetVersionKey(grant.Email)) ?? string.Empty, grant.Version, StringComparison.Ordinal)
            && userInfoService.Any(user => user.Email == grant.Email);
    }

    public void Revoke(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            redisClient.Set(GetVersionKey(email), CreateRandomToken());
        }
    }

    private static string CreateRandomToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    private static string GetVersionKey(string email)
    {
        var normalizedEmail = email.Trim().ToUpperInvariant();
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedEmail)));
        return VersionKeyPrefix + hash;
    }

    private sealed record AccessViewTicket(string Email, string Version, DateTimeOffset ExpiresAtUtc);
}
