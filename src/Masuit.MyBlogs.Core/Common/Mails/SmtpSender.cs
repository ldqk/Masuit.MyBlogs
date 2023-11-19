using FreeRedis;
using Hangfire;
using Masuit.Tools.Models;
using System.Text;

namespace Masuit.MyBlogs.Core.Common.Mails;

public sealed class SmtpSender : IMailSender
{
    private readonly IRedisClient _redisClient;

    public SmtpSender(IRedisClient redisClient)
    {
        _redisClient = redisClient;
    }

    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public Task Send(string title, string content, string tos, string clientip)
    {
        new Email()
        {
            EnableSsl = bool.Parse(CommonHelper.SystemSettings.GetOrAdd("EnableSsl", "true")),
            Body = content,
            SmtpServer = CommonHelper.SystemSettings["SMTP"],
            Username = CommonHelper.SystemSettings["EmailFrom"],
            Password = CommonHelper.SystemSettings["EmailPwd"],
            SmtpPort = CommonHelper.SystemSettings["SmtpPort"].ToInt32(),
            Subject = title,
            Tos = tos
        }.Send();
        _redisClient.SAdd($"Email:{DateTime.Now:yyyyMMdd}", new { title, content, tos, time = DateTime.Now, clientip });
        _redisClient.Expire($"Email:{DateTime.Now:yyyyMMdd}", 86400);
        return Task.CompletedTask;
    }

    public Task<List<string>> GetBounces()
    {
        return Task.FromResult(File.ReadAllText(Path.Combine(AppContext.BaseDirectory + "App_Data", "email-bounces.txt"), Encoding.UTF8).Split(',').ToList());
    }

    public async Task<string> AddRecipient(string email)
    {
        var bounces = await GetBounces();
        bounces.Add(email);
        await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory + "App_Data", "email-bounces.txt"), bounces.Join(","));
        return "添加成功";
    }

    public Task<bool> HasBounced(string address)
    {
        return GetBounces().ContinueWith(list => list.Result.Contains(address));
    }
}
