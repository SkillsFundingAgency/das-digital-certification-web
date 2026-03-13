using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class ConfigurationExtensions
    {
        public static IConfiguration BuildDasConfiguration(this IConfiguration configuration)
        {
            var config = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
#endif
                .AddEnvironmentVariables();

            if (!configuration.IsRunningInDev())
            {
                config.AddAzureTableStorage(options =>
                    {
                        options.ConfigurationKeys = configuration["ConfigNames"]?.Split(",");
                        options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                        options.EnvironmentName = configuration["EnvironmentName"];
                        options.PreFixConfigurationKeys = false;
                    }
                );
            }

            return config.Build();
        }

        public static T? GetSection<T>(this IConfiguration configuration)
        {
            return configuration
                .GetSection(typeof(T).Name)
                .Get<T?>();
        }

        public static bool IsRunningInDev(this IConfiguration configuration)
        {
            return configuration["EnvironmentName"]?.Equals("DEV", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        public static bool IsRunningInProd(this IConfiguration configuration)
        {
            return configuration["EnvironmentName"]?.Equals("PROD", StringComparison.CurrentCultureIgnoreCase) ?? false;
        }
    }
}