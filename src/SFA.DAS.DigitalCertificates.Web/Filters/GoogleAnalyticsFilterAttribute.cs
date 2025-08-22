using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Authorization;
using SFA.DAS.DigitalCertificates.Web.Models.Shared;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SFA.DAS.DigitalCertificates.Web.Filters
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class GoogleAnalyticsFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.Controller is not Controller controller)
            {
                return;
            }

            controller.ViewBag.GaData = PopulateGaData(context);

            base.OnActionExecuting(context);
        }

        private static GaData PopulateGaData(ActionExecutingContext context)
        {
            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Equals(DigitalCertificateClaimsTypes.UserId))?.Value;

            return new GaData
            {
                UserId = userId
            };
        }
    }
}