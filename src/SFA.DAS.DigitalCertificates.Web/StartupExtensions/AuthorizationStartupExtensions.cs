using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.GovUK.Auth.Authentication;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AuthorizationStartupExtensions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    PolicyNames.IsAuthenticated, policy =>
                    {
                        policy.RequireAuthenticatedUser();
                    });
                options.AddPolicy(
                    PolicyNames.IsActiveAccount, policy =>
                    {
                        policy.Requirements.Add(new AccountActiveRequirement());
                        policy.RequireAuthenticatedUser();
                    });
                options.AddPolicy(
                    PolicyNames.IsVerified, policy =>
                    {
                        policy.Requirements.Add(new VerifiedIdentityRequirement());
                        policy.RequireAuthenticatedUser();
                    });
            });

            return services;
        }
    }
}