using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Helpers;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System.Linq;
using System;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class BaseOrchestrator
    {
        private readonly IMediator _mediator;

        public BaseOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IMediator Mediator => _mediator;

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
