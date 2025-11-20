using SFA.DAS.Http.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class DigitalCertificatesOuterApiConfiguration : IApimClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string SubscriptionKey { get; set; }

        public string ApiVersion { get; set; }
    }
}
