namespace SFA.DAS.DigitalCertificates.Web.Models.Sharing
{
    public class CookiesViewModel
    {
        public required string PreviousPageUrl { get; set; }
        public bool ConsentFunctionalCookie { get; set; }
        public bool ConsentAnalyticsCookie { get; set; }
        public bool ShowBannerMessage { get; set; }
    }
}
