using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Attributes;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Services.SessionStorage;
using SFA.DAS.DigitalCertificates.Web.Services.User;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AddServiceRegistrationExtensions
    {
        public static IServiceCollection AddServiceRegistrations(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetUserQuery).Assembly));

            services.AddSingleton<IAuthorizationHandler, AccountActiveAuthorizationHandler>();

            services.AddTransient<ISessionStorageService, SessionStorageService>();
            services.AddTransient<ICacheStorageService, CacheStorageService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserCacheService, UserCacheService>();

            services.AddSingleton<IAuthorizationHandler, UlnAuthorisedAuthorizationHandler>();
            services.AddSingleton<IAuthorizationFailureHandler, UlnAuthorisedFailureHandler>();
            services.AddSingleton<IAuthorizationHandler, CertificateOwnerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationFailureHandler, CertificateOwnerFailureHandler>();

            services.AddTransient<ValidateRequiredQueryParametersAttribute>();
            services.AddTransient<IHomeOrchestrator, HomeOrchestrator>();
            services.AddTransient<ICertificatesOrchestrator, CertificatesOrchestrator>();
            
            services.AddTransient<IClaimsTransformation, DigitalCertificatesClaimsTransformer>();

            return services;
        }

        public static IServiceCollection AddOuterApi(this IServiceCollection services, DigitalCertificatesOuterApiConfiguration configuration)
        {
            services.AddHealthChecks();
            services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
            services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
            services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

            services
                .AddRestEaseClient<IDigitalCertificatesOuterApi>(configuration.ApiBaseUrl)
                .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
                .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();

            services.AddTransient<IApimClientConfiguration>((_) => configuration);

            return services;
        }
    }
}