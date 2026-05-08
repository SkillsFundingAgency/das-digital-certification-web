using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Configuration
{
    [ExcludeFromCodeCoverage]
    public class DigitalCertificatesWebConfiguration
    {
        public required string ServiceBaseUrl { get; set; }
        public required string OneLoginSettingsUrl { get; set; }
        public required string RedisConnectionString { get; set; }
        public required string DataProtectionKeysDatabase { get; set; }
        public int? SharingListLimit { get; set; }
        public int? SharingHistoryLimit { get; set; }       
        public required string ContainerName { get; set; }
        public required string AsposeLicenseContainerName { get; set; }
        public required string StandardTemplateBlobName { get; set; }
        public required string GreenStandardTemplateBlobName { get; set; }
        public required string FrameworkTemplateBlobName { get; set; }
        public required string LicenseBlobName { get; set; }
        public required string MasterPassword { get; set; }
        public required string StorageConnectionString { get; set; }
        public List<NotificationTemplate>? NotificationTemplates { get; set; }
        public DateTime? CutoverDate { get; set; }
        public int? MatchesCacheExpiryDays { get; set; }
        public int? FailedMatchesLimit { get; set; }
        public int? MinimumMasksForSelection { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class NotificationTemplate
    {
        public required string TemplateName { get; set; }
        public required string TemplateId { get; set; }
    }
}