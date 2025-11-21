using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class StringExtensions
    {
        public static DateTime ParseEnGbDateTime(this string dateTime)
        {
            return DateTime.Parse(
                    dateTime,
                    CultureInfo.GetCultureInfo("en-GB"),
                    DateTimeStyles.None);
        }
    }
}