using System;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Web.Enums
{
    public static class GradeExtensions
    {
        private static readonly Dictionary<string, Grade> _map = new(StringComparer.OrdinalIgnoreCase)
        {
            { "CREDIT", Grade.Credit },
            { "DISTINCTION", Grade.Distinction },
            { "MERIT", Grade.Merit },
            { "OUTSTANDING", Grade.Outstanding },
            { "PASS", Grade.Pass },
            { "PASS WITH EXCELLENCE", Grade.PassWithExcellence },
            { "NO GRADE AWARDED", Grade.NoGradeAwarded },
            { "NO GRADE", Grade.NoGradeAwarded }
        };

        public static Grade ParseFromApi(this string? apiValue)
        {
            if (string.IsNullOrWhiteSpace(apiValue))
                return Grade.Unknown;

            return _map.TryGetValue(apiValue.Trim(), out var grade)
                ? grade
                : Grade.Unknown;
        }

        public static string ToDisplay(this Grade grade)
        {
            return grade switch
            {
                Grade.Credit => "Credit",
                Grade.Distinction => "Distinction",
                Grade.Merit => "Merit",
                Grade.Outstanding => "Outstanding",
                Grade.Pass => "Pass",
                Grade.PassWithExcellence => "Pass with excellence",
                Grade.NoGradeAwarded => "No grade awarded",
                _ => string.Empty,
            };
        }

        public static string ToBannerDisplay(this Grade grade)
        {
            return grade switch
            {
                Grade.Credit => "You have passed your apprenticeship with credit",
                Grade.Distinction => "You have passed your apprenticeship with distinction",
                Grade.Merit => "You have passed your apprenticeship with merit",
                Grade.Outstanding => "You have passed your apprenticeship with outstanding",
                Grade.Pass => "You have passed your apprenticeship",
                Grade.PassWithExcellence => "You have passed your apprenticeship with excellence",
                Grade.Unknown => "You have passed your apprenticeship",
                Grade.NoGradeAwarded => "You have passed your apprenticeship",
                _ => "You have passed your apprenticeship",
            };
        }

        public static string ToResultDisplay(this Grade grade)
        {
            return grade switch
            {
                Grade.Credit => "Credit",
                Grade.Distinction => "Distinction",
                Grade.Merit => "Merit",
                Grade.Outstanding => "Outstanding",
                Grade.Pass => "Pass",
                Grade.PassWithExcellence => "Pass with excellence",
                Grade.NoGradeAwarded => "No grade awarded",
                _ => string.Empty,
            };
        }

        public static bool IsAvailable(this Grade grade)
        {
            return grade != Grade.NoGradeAwarded && grade != Grade.Unknown && grade != Grade.Pass;
        }
    }
}
