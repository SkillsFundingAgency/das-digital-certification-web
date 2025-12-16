using System;
using System.Globalization;

namespace SFA.DAS.DigitalCertificates.Web.Extensions;

public static class DateTimeExtensions
{
    private static readonly TimeZoneInfo UkTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

    public static DateTime UtcToUkLocalTime(this DateTime date)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(date, UkTimeZone);
    }

    public static string ToUkDateTimeString(this DateTime date)
    {
        return date
            .UtcToUkLocalTime()
            .ToString("h:mmtt d MMMM yyyy", CultureInfo.GetCultureInfo("en-GB"))
            .ToLowerInvariant();
    }

    public static string ToUkExpiryDateTimeString(this DateTime date)
    {
        return date
            .UtcToUkLocalTime()
            .ToString("h:mmtt 'on' d MMMM yyyy", CultureInfo.GetCultureInfo("en-GB"))
            .ToLowerInvariant();
    }
}
