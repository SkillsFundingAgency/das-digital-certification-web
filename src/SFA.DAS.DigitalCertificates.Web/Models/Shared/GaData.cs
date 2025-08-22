using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Models.Shared
{
    [ExcludeFromCodeCoverage]
    public class GaData
    {
        public string DataLoaded { get; set; } = "dataLoaded";
        public string UserId { get; set; }
        public string Vpv { get; set; }
        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }
}
