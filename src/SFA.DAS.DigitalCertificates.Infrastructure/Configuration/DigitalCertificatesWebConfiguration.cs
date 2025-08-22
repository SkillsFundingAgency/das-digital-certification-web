using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class DigitalCertificatesWebConfiguration
    {
        public string RedisConnectionString { get; set; }
        public string DataProtectionKeysDatabase { get; set; }
    }
}