using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class CacheStartupExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services, DigitalCertificatesWebConfiguration config, IHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(options => { options.Configuration = config.RedisConnectionString; });
            }

            return services;
        }
    }
}