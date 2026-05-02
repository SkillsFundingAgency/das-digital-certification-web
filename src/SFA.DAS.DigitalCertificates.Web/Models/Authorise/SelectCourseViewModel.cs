using System.Collections.Generic;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Models.Authorise
{
    public class SelectCourseViewModel : AuthoriseViewModelBase
    {
        //ToDo:Neeed to remove the unused fields at the end of journey
        public string? SelectedCourseCode { get; set; }
        public List<CourseOption>? Courses { get; set; }

        public class CourseOption
        {
            public required string CourseCode { get; set; }
            public required string CourseName { get; set; }
            public string? CourseLevel { get; set; }
            public CertificateType CertificateType { get; set; }

            public string DisplayName
            {
                get
                {
                    var levelText = string.Empty;
                    if (!string.IsNullOrEmpty(CourseLevel))
                    {
                        if (CertificateType == CertificateType.Framework)
                        {
                            levelText = $" ({CourseLevel} Level)";
                        }
                        else
                        {
                            levelText = $" (Level {CourseLevel})";
                        }
                    }

                    return string.Concat(CourseName ?? string.Empty, levelText);
                }
            }
        }
    }
}
