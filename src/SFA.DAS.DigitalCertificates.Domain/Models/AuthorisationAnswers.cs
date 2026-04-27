
namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class AuthorisationAnswers
    {
        //ToDo:Neeed to remove the unused fields at the end of journey
        public bool? KnowUln { get; set; }
        public long? Uln { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public bool? KnowYear { get; set; }
        public int? YearCompleted { get; set; }
        public int FailedMatchCount { get; set; }
        public string? ProviderUkprn { get; set; }
        public string? ProviderName { get; set; }
        public bool? ProviderUnknown { get; set; }
    }
}
    