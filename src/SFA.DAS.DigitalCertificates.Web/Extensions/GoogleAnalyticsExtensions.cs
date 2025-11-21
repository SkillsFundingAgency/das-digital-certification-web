using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class GoogleAnalyticsExtensions
    {
        public static bool GoogleAnalyticsIsEnabled(this ViewDataDictionary viewData)
            => !string.IsNullOrWhiteSpace(GetConfiguration(viewData)?.GoogleTagManagerId);

        public static string? GetGoogleTagManagerId(this ViewDataDictionary viewData)
            => GetConfiguration(viewData)?.GoogleTagManagerId;

        private static GoogleAnalytics? GetConfiguration(ViewDataDictionary viewData)
            => viewData.TryGetValue(ViewDataKeys.ViewDataKeys.GoogleAnalyticsConfiguration, out var section)
                ? section as GoogleAnalytics
                : null;
    }

    public enum GoogleAnalyticsTag
    {
        Head,
        Body,
    }
}