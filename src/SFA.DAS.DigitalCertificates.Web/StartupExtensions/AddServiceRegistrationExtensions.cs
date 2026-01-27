using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateOrUpdateUser;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Web.Attributes;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.Http.Configuration;
using SFA.DAS.DigitalCertificates.Domain.Extensions;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;

namespace SFA.DAS.DigitalCertificates.Web.StartupExtensions
{
    [ExcludeFromCodeCoverage]
    public static class AddServiceRegistrationExtensions
    {
        public static IServiceCollection AddServiceRegistrations(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrUpdateUserCommand).Assembly));

            services.AddSingleton<IAuthorizationHandler, AccountActiveAuthorizationHandler>();

            services.AddTransient<ICacheStorageService, CacheStorageService>();
            services.AddTransient<ICacheService, CacheService>();
            services.AddTransient<IUserService, UserService>();

            services.AddScoped<ISessionStorageService, SessionStorageService>();
            services.AddScoped<ISessionService, SessionService>();

            services.AddSingleton<IAuthorizationHandler, UlnAuthorisedAuthorizationHandler>();
            services.AddSingleton<IAuthorizationFailureHandler, UlnAuthorisedFailureHandler>();
            services.AddSingleton<IAuthorizationHandler, CertificateOwnerAuthorizationHandler>();
            services.AddSingleton<IAuthorizationFailureHandler, CertificateOwnerFailureHandler>();

            services.AddTransient<ValidateRequiredQueryParametersAttribute>();
            services.AddTransient<IHomeOrchestrator, HomeOrchestrator>();
            services.AddTransient<ICertificatesOrchestrator, CertificatesOrchestrator>();
            services.AddTransient<ISharingOrchestrator, SharingOrchestrator>();

            services.AddTransient<IClaimsTransformation, DigitalCertificatesClaimsTransformer>();

            services.AddScoped<IDateTimeHelper, UtcTimeProvider>();

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