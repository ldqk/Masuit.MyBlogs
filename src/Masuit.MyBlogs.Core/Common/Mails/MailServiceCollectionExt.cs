using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Masuit.MyBlogs.Core.Common.Mails
{
    public static class MailServiceCollectionExt
    {
        public static IServiceCollection AddMailSender(this IServiceCollection services, IConfiguration configuration)
        {
            switch (configuration["MailSender"])
            {
                case "Mailgun":
                    services.AddHttpClient<IMailSender, MailgunSender>();
                    break;
                default:
                    services.AddSingleton<IMailSender, SmtpSender>();
                    break;
            }

            return services;
        }
    }
}