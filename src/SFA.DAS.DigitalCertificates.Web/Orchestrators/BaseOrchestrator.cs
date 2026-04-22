using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Helpers;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;

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
            return _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? string.Empty;
        }

        protected static async Task<bool> ValidateViewModel<T>(IValidator<T> validator, T viewModel, ModelStateDictionary modelState)
        {
            var result = await validator.ValidateAndAddModelErrorsAsync(viewModel, modelState);
            return modelState.IsValid;
        }

        protected string GetTemplateId(DigitalCertificatesWebConfiguration digitalCertificatesWebConfiguration, string templateName)
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
