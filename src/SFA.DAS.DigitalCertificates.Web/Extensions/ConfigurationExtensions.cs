using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ConfigurationExtensions
    {
        public static bool TryGetSection<T>(
            this IConfiguration configuration,
            out T? value,
            string? sectionName = null)
            where T : class
        {
            var key = sectionName ?? typeof(T).Name;
            var section = configuration.GetSection(key);

            if (!section.Exists())
            {
                value = null;
                return false;
            }

            value = section.Get<T>();

            return value != null;
        }
    }

}
