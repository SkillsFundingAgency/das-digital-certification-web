using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class SessionStartupExtensions
    {
        public static IServiceCollection AddSession(this IServiceCollection services, DigitalCertificatesWebConfiguration configWeb)
        {
            services.AddSession(opt =>
            {
                opt.IdleTimeout = TimeSpan.FromMinutes(20);
                opt.Cookie = new CookieBuilder()
                {
                    Name = ".DigitalCertificates.Session",
                    HttpOnly = true
                };
            });

            return services;
        }
    }
}
