namespace SFA.DAS.DigitalCertificates.Web.Models.Authorise
{
    public class CheckAnswersViewModel : AuthoriseViewModelBase
    {
        public string? CourseDisplay { get; set; }
        
        
        public string? UlnDisplay { get; set; }
        public string? YearDisplay { get; set; }
        public string? ProviderDisplay { get; set; }
        public string? BackLinkRouteName { get; set; }
    }
}
