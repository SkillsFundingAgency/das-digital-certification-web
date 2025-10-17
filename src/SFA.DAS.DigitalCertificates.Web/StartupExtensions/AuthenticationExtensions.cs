using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddGovUkOneLoginAuthentication(this IServiceCollection services, DigitalCertificatesWebConfiguration webConfiguration,
            IConfiguration configuration)
        {
            services.AddTransient<IStubAuthenticationService, StubAuthenticationService>();
            services.AddAndConfigureGovUkAuthentication(configuration,
                new AuthRedirects
                {
                    SuspendedRedirectUrl = "/locked",
                    SignedOutRedirectUrl = "/user-signed-out",
                    LoginRedirect = webConfiguration.ServiceBaseUrl + "/stub/sign-in-stub",
                    LocalStubLoginPath = "/stub/sign-in-Stub"
                },
                typeof(DigitalCertificateCustomClaims),
                null
                );

            return services;
        }
    }
}