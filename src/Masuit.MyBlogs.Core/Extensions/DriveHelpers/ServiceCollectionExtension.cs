using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.MyBlogs.Core.Infrastructure.Drive;
using Microsoft.Extensions.DependencyInjection;

namespace Masuit.MyBlogs.Core.Extensions.DriveHelpers
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddOneDrive(this IServiceCollection services)
        {
            services.AddDbContext<DriveContext>(ServiceLifetime.Scoped);
            //不要被 CG Token获取采用单一实例
            services.AddSingleton(new TokenService());
            services.AddTransient<IDriveAccountService, DriveAccountService>();
            services.AddTransient<IDriveService, DriveService>();
            services.AddScoped<SettingService>();
            return services;
        }
    }
}