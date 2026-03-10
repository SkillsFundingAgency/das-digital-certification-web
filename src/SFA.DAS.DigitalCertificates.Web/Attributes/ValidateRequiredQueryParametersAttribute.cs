using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Domain.Extensions;

namespace SFA.DAS.DigitalCertificates.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ValidateRequiredQueryParametersAttribute : ActionFilterAttribute
    {
        public const string MissingRequireQueryParameterMessage = "Missing required query parameter: ";
        private readonly ILogger _logger;

        public ValidateRequiredQueryParametersAttribute(ILogger<ValidateRequiredQueryParametersAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var modelState = context.ModelState;
            var requiredParameterErrors = modelState.Values
                .SelectMany(v => v.Errors)
                .Where(e => e.ErrorMessage.StartsWith(MissingRequireQueryParameterMessage))
                .Select(e => e.ErrorMessage.Replace(MissingRequireQueryParameterMessage, string.Empty))
                .ToList();

            if (requiredParameterErrors.Any())
            {
                var errorMessage = $"Missing required query parameters: {string.Join(", ", requiredParameterErrors)}";
                _logger.LogError(errorMessage.SanitizeLogData());

                throw new ArgumentException(errorMessage);
            }
        }
    }
}
