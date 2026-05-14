using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class SessionCookieOptionsExtensions
    {
        public static IServiceCollection AddSecureSessionCookie(this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AspNetCore.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;               
            });

            return services;
        }
    }
}