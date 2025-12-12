using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class GoogleAnalytics
    {
        public required string GoogleTagManagerId { get; set; }
    }
}