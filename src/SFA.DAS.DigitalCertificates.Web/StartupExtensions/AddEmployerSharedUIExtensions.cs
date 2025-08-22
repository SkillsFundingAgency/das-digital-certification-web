using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.DigitalCertificates.Web.Models.Shared;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AddEmployerSharedUiExtensions
    {
        public static IServiceCollection AddEmployerSharedUi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMaMenuConfiguration("signout", configuration["ResourceEnvironmentName"]);

            services.AddSingleton<ICookieBannerViewModel>(provider =>
            {
                var maLinkGenerator = provider.GetService<UrlBuilder>();
                return new CookieBannerViewModel
                {
                    CookieDetailsUrl = maLinkGenerator.AccountsLink("Cookies") + "/details",
                    CookieConsentUrl = maLinkGenerator.AccountsLink("Cookies"),
                };
            });

            return services;
        }
    }
}