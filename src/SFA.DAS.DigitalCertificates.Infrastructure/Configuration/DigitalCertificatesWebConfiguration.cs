using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

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

        public List<NotificationTemplate>? NotificationTemplates { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class NotificationTemplate
    {
        public required string TemplateName { get; set; }
        public required string TemplateId { get; set; }
    }
}