using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Web.Models.Authorise
{
    public class SelectProviderViewModel : AuthoriseViewModelBase
    {
        public string? SelectedProviderName { get; set; }
        public const string UnknownProviderSentinel = "UNKNOWN";
        public bool? SelectedProviderUnknown { get; set; }
        public List<ProviderOption>? Providers { get; set; }

        public class ProviderOption
        {
            public long? Ukprn { get; set; }
            public required string ProviderName { get; set; }
        }
    }
}
