using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class DigitalCertificatesWebConfiguration
    {
        public required string ServiceBaseUrl { get; set; }
        public required string RedisConnectionString { get; set; }
        public required string DataProtectionKeysDatabase { get; set; }
        public int? SharingListLimit { get; set; }
        public int? SharingHistoryLimit { get; set; }
    }
}