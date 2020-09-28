using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Masuit.Tools.Models;
using Microsoft.Extensions.Configuration;

namespace Masuit.MyBlogs.Core.Common.Mails
{
    public class MailgunSender : IMailSender
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public MailgunSender(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public void Send(string title, string content, string tos)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{_configuration["MailgunConfig:apikey"]}")));
            EmailAddress email = _configuration["MailgunConfig:from"];
            var form = new MultipartFormDataContent
            {
                { new StringContent(email,Encoding.UTF8), "from" },
                { new StringContent(tos,Encoding.UTF8), "to" },
                { new StringContent(title,Encoding.UTF8), "subject" },
                { new StringContent(content,Encoding.UTF8), "html" }
            };
            _httpClient.PostAsync($"https://api.mailgun.net/v3/{email.Domain}/messages", form).Wait();
        }
    }
}