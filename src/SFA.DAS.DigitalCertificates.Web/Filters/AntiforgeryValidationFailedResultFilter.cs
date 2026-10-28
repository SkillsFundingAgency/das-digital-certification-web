using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using SFA.DAS.DigitalCertificates.Web.Controllers;

namespace SFA.DAS.DigitalCertificates.Web.Filters
{
     [ExcludeFromCodeCoverage]
    public class AntiforgeryValidationFailedResultFilter : IAsyncAlwaysRunResultFilter
    {
        private readonly ILogger<AntiforgeryValidationFailedResultFilter> _logger;

        public AntiforgeryValidationFailedResultFilter(ILogger<AntiforgeryValidationFailedResultFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is AntiforgeryValidationFailedResult)
            {
                _logger.LogWarning("Antiforgery token validation failed for {Path}", context.HttpContext.Request.Path);

                context.Result = new RedirectToRouteResult(HomeController.AntiforgeryRouteGet, routeValues: null);
            }

            await next();
        }
    }
}
