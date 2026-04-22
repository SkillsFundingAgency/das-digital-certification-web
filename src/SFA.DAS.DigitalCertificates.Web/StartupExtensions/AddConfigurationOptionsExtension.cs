using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.Encoding;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AddConfigurationOptionsExtension
    {
        public static void AddConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();            
            services.Configure<DigitalCertificatesWebConfiguration>(options =>
            {
                configuration.GetSection(nameof(DigitalCertificatesWebConfiguration)).Bind(options);
                options.StorageConnectionString = configuration["StorageConnectionString"]
                ?? throw new InvalidOperationException("StorageConnectionString is not configured."); ;
            });

            services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<DigitalCertificatesWebConfiguration>>().Value);
            services.Configure<DigitalCertificatesOuterApiConfiguration>(configuration.GetSection(nameof(DigitalCertificatesOuterApiConfiguration)));
            services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<DigitalCertificatesOuterApiConfiguration>>().Value);            
            services.Configure<EncodingConfig>(configuration.GetSection(nameof(EncodingConfig)));
            services.AddSingleton(cfg => cfg.GetRequiredService<IOptions<EncodingConfig>>().Value);
        }
    }
}