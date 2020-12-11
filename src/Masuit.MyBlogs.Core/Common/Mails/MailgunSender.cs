using CacheManager.Core;
using Masuit.Tools.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Masuit.MyBlogs.Core.Common.Mails
{
    public class MailgunSender : IMailSender
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ICacheManager<List<string>> _cacheManager;

        public MailgunSender(HttpClient httpClient, IConfiguration configuration, ICacheManager<List<string>> cacheManager)
        {
            _configuration = configuration;
            _cacheManager = cacheManager;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{_configuration["MailgunConfig:apikey"]}")));
        }

        public void Send(string title, string content, string tos)
        {
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

        public List<string> GetBounces()
        {
            EmailAddress email = _configuration["MailgunConfig:from"];
            return _cacheManager.GetOrAdd("emailbounces", _ => _httpClient.GetStringAsync($"https://api.mailgun.net/v3/{email.Domain}/bounces").ContinueWith(t =>
             {
                 return t.IsCompletedSuccessfully ? ((JArray)JObject.Parse(t.Result)["items"])?.Select(x => (string)x["address"]).ToList() : new List<string>();
             }).Result);
        }

        public string AddBounces(string email)
        {
            EmailAddress mail = _configuration["MailgunConfig:from"];
            return _httpClient.PostAsync($"https://api.mailgun.net/v3/{mail.Domain}/bounces", new MultipartFormDataContent
            {
                { new StringContent(email,Encoding.UTF8), "address" },
                { new StringContent("黑名单邮箱",Encoding.UTF8), "error" }
            }).ContinueWith(t =>
            {
                var resp = t.Result;
                if (resp.IsSuccessStatusCode)
                {
                    return (string)JObject.Parse(resp.Content.ReadAsStringAsync().Result)["message"];
                }
                return "添加失败";
            }).Result;
        }
    }
}