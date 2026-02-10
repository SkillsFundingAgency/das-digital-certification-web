using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using StackExchange.Redis;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class DataProtectionStartupExtensions
    {
        public static IServiceCollection AddDasDataProtection(this IServiceCollection services, DigitalCertificatesWebConfiguration configWeb, IHostEnvironment environment)
        {
            if (!environment.IsDevelopment())
            {
                var redisConnectionString = configWeb.RedisConnectionString;
                var dataProtectionKeysDatabase = configWeb.DataProtectionKeysDatabase;

                var redis = ConnectionMultiplexer.Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

                services.AddDataProtection()
                    .SetApplicationName("das-digital-certificates")
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }

            return services;
        }
    }
}