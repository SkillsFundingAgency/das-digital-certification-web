using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Http.Configuration;

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
