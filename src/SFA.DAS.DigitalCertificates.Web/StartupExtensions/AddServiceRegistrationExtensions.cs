using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using RestEase.HttpClientFactory;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.CacheStorage;
using SFA.DAS.DigitalCertificates.Infrastructure.Services.SessionStorage;
using SFA.DAS.DigitalCertificates.Web.Attributes;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.Http.Configuration;
using System.Diagnostics.CodeAnalysis;

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

            services.AddTransient<ValidateRequiredQueryParametersAttribute>();

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