namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public static class FirewallServiceCollectionExt
{
    public static IServiceCollection AddFirewallReporter(this IServiceCollection services, IConfiguration configuration)
    {
        switch (configuration["FirewallService:type"])
        {
            case "Cloudflare":
            case "cloudflare":
            case "cf":
                services.AddHttpClient<IFirewallReporter, CloudflareReporter>().ConfigureHttpClient(c =>
                {
                    c.DefaultRequestHeaders.Add("X-Auth-Email", configuration["FirewallService:Cloudflare:AuthEmail"]);
                    c.DefaultRequestHeaders.Add("X-Auth-Key", configuration["FirewallService:Cloudflare:AuthKey"]);
                });
                break;

            default:
                services.AddSingleton<IFirewallReporter, DefaultFirewallReporter>();
                break;
        }

        return services;
    }
}