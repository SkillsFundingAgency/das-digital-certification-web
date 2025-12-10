using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Web.HealthChecks
{
    [ExcludeFromCodeCoverage]
    public class ApiHealthCheck : IHealthCheck
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public ApiHealthCheck(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
        {
            var description = "Ping of Apprenticeship certificates outer API";

            try
            {
                await _outerApi.Ping();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(description, ex);
            }

            return HealthCheckResult.Healthy(description, new Dictionary<string, object>());
        }
    }
}