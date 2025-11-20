using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Models.Shared
{
    public interface ICookieBannerViewModel
    {
        string CookieConsentUrl { get; }
        string CookieDetailsUrl { get; }
    }

    [ExcludeFromCodeCoverage]
    public class CookieBannerViewModel : ICookieBannerViewModel
    {
        public string CookieDetailsUrl { get; set; }
        public string CookieConsentUrl { get; set; }
    }
}
