using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class DictionaryExtensions
    {
        public static void AddIfNotPresent<TKey, TValue>(this IDictionary<TKey, TValue> headers, TKey key, TValue values)
        {
            if (headers.ContainsKey(key))
                return;

            headers.Add(key, values);
        }
    }
}