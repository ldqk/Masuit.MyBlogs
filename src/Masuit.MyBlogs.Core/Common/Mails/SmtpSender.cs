using FreeRedis;
using Hangfire;

namespace Masuit.MyBlogs.Core.Common.Mails;

public sealed class SmtpSender(IRedisClient redisClient) : IMailSender
{
    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public Task Send(string title, string content, string tos, string clientip)
    {
#if !DEBUG
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
#endif

        redisClient.SAdd($"Email:{DateTime.Now:yyyyMMdd}", new { title, content, tos, time = DateTime.Now, clientip });
        redisClient.Expire($"Email:{DateTime.Now:yyyyMMdd}", 86400);
        return Task.CompletedTask;
    }
}