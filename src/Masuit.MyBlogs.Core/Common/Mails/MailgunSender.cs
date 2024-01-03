using FreeRedis;
using Hangfire;
using System.Net.Http.Headers;
using System.Text;

namespace Masuit.MyBlogs.Core.Common.Mails;

public sealed class MailgunSender : IMailSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IRedisClient _redisClient;

    public MailgunSender(HttpClient httpClient, IConfiguration configuration, IRedisClient redisClient)
    {
        _configuration = configuration;
        _redisClient = redisClient;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{_configuration["MailgunConfig:apikey"]}")));
    }

    [AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task Send(string title, string content, string tos, string clientip)
    {
        EmailAddress email = _configuration["MailgunConfig:from"];
        using var form = new MultipartFormDataContent
        {
            { new StringContent(email,Encoding.UTF8), "from" },
            { new StringContent(tos,Encoding.UTF8), "to" },
            { new StringContent(title,Encoding.UTF8), "subject" },
            { new StringContent(content,Encoding.UTF8), "html" }
        };
        await _httpClient.PostAsync($"https://api.mailgun.net/v3/{email.Domain}/messages", form);
        await _redisClient.SAddAsync($"Email:{DateTime.Now:yyyyMMdd}", new { title, content, tos, time = DateTime.Now, clientip });
        await _redisClient.ExpireAsync($"Email:{DateTime.Now:yyyyMMdd}", 86400);
    }
}
