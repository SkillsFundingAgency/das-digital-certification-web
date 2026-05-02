namespace SFA.DAS.DigitalCertificates.Web.Models.Authorise
{
    public class KnowYearViewModel : AuthoriseViewModelBase
    {
        public bool? KnowYear { get; set; }

        public int? YearCompleted { get; set; }
        public bool ProviderSelected { get; set; }
        
    }
}
