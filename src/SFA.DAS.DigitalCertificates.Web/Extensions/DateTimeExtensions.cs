using System;
using System.Globalization;

namespace SFA.DAS.DigitalCertificates.Web.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo UkTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    private static readonly CultureInfo UkLowerCaseAmPmCulture = CreateUkLowerCaseCulture();

    private static CultureInfo CreateUkLowerCaseCulture()
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("en-GB").Clone();
        culture.DateTimeFormat.AMDesignator = "am";
        culture.DateTimeFormat.PMDesignator = "pm";
        return culture;
    }

    public static DateTime UtcToUkLocalTime(this DateTime date)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(date, UkTimeZone);
    }

    public static string ToUkDateTimeString(this DateTime date)
    {
        return date
            .UtcToUkLocalTime()
            .ToString("h:mmtt d MMMM yyyy", UkLowerCaseAmPmCulture);
    }

    public static string ToUkExpiryDateTimeString(this DateTime date)
    {
        return date
            .UtcToUkLocalTime()
            .ToString("h:mmtt 'on' d MMMM yyyy", UkLowerCaseAmPmCulture);
    }
}
