using SFA.DAS.Http.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class DigitalCertificatesOuterApiConfiguration : IApimClientConfiguration
    {
        public required string ApiBaseUrl { get; set; }
        public required string SubscriptionKey { get; set; }

        public required string ApiVersion { get; set; }
    }
}
