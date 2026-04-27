using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Web.Models.Authorise
{
    public class SelectProviderViewModel
    {
        public string? SelectedProviderName { get; set; }
        public const string UnknownProviderSentinel = "UNKNOWN";
        public bool? SelectedProviderUnknown { get; set; }
        public List<ProviderOption>? Providers { get; set; }

        public class ProviderOption
        {
            public string? Ukprn { get; set; }
            public string? ProviderName { get; set; }
            public string DisplayName => ProviderName ?? string.Empty;
        }
    }
}
