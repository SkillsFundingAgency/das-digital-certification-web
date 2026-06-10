using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Helpers;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class BaseOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseOrchestrator(IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public IMediator Mediator => _mediator;

        protected string GetUserDisplayName()
        {
            return _httpContextAccessor?
                .HttpContext?
                .User?
                .Identity?
                .Name ?? string.Empty;
        }

        protected string GetUserGivenNames()
        {
            return _httpContextAccessor?
                .HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.GivenName) ?? string.Empty;
        }

        protected string GetUserSurname()
        {
            // get matches gets a certifcate based on each family name and date of brth
            // lets say the user was called jones and now they call called smith
            // they have a certifcyate for jones and then get the questions correct - but the match would be against smith - thats not correct is it?
            // The surname claim contains ypur latest name
            
            return _httpContextAccessor?
                .HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.Surname) ?? string.Empty;
        }

        protected string GetUserEmail()
        {
            return _httpContextAccessor?
                .HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        protected static async Task<bool> ValidateViewModel<T>(IValidator<T> validator, T viewModel, ModelStateDictionary modelState)
        {
            await validator.ValidateAndAddModelErrorsAsync(viewModel, modelState);
            return modelState.IsValid;
        }

        protected static string GetTemplateId(DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration, string templateName)
        {
            if (digitalCertificatesWebConfiguration == null)
                throw new ArgumentNullException(nameof(digitalCertificatesWebConfiguration));

            var template = digitalCertificatesWebConfiguration.NotificationTemplates?
                .FirstOrDefault(t => string.Equals(t.TemplateName, templateName, StringComparison.OrdinalIgnoreCase));

            if (template != null && !string.IsNullOrEmpty(template.TemplateId))
                return template.TemplateId;

            throw new InvalidOperationException($"Notification template '{templateName}' is not configured in DigitalCertificatesWebConfiguration.NotificationTemplates.");
        }
    }
}
