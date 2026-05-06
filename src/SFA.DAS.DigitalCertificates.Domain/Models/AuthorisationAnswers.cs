namespace SFA.DAS.DigitalCertificates.Domain.Models
{
    public class AuthorisationAnswers
    {
        public bool? KnowUln { get; set; }
        public long? Uln { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseName { get; set; }
        public string? CourseLevel { get; set; }
        public CertificateType? CourseCertificateType { get; set; }
        public bool? KnowYear { get; set; }
        public int? YearCompleted { get; set; }
        public int FailedMatchCount { get; set; }
        public long? ProviderUkprn { get; set; }
        public string? ProviderName { get; set; }
        public bool? ProviderUnknown { get; set; }
        public bool? CourseUnknown { get; set; }
        
        // TODO: At present, we determine the long journey at this point.
        // This can be adjusted based on future requirements.

        private bool shortJourneyFromMatch = true;

        public bool IsShortJourney
        {
            get
            {
                if (ProviderUnknown != null
                    || !string.IsNullOrWhiteSpace(ProviderName)
                    || KnowYear != null
                    || YearCompleted != null)
                {
                    return false;
                }

                return shortJourneyFromMatch;
            }
            set => shortJourneyFromMatch = value;
        }

        public bool IsReturningToCheck { get; set; } = false;
    }
}
