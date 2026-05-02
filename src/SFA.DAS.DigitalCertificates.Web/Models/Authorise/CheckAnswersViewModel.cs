namespace SFA.DAS.DigitalCertificates.Web.Models.Authorise
{
    public class CheckAnswersViewModel : AuthoriseViewModelBase
    {
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        
        public bool ShowNoMatchBanner { get; set; }
        public string? UlnDisplay { get; set; }
        public string? YearDisplay { get; set; }
        public string? ProviderDisplay { get; set; }
        public string? BackLinkRouteName { get; set; }
    }
}
