using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Configuration;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddEmployerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IStubAuthenticationService, StubAuthenticationService>();
            services.Configure<GovUkOidcConfiguration>(configuration.GetSection("GovUkOidcConfiguration"));
            services.AddAndConfigureGovUkAuthentication(configuration,
                new AuthRedirects
                {
                    SignedOutRedirectUrl = "/user-signed-out",
                    SuspendedRedirectUrl = "/locked",
                    LocalStubLoginPath = "/stub/sign-in-Stub"
                },
                typeof(DigitalCertificateCustomClaims),
                null
                );

            return services;
        }
    }
}