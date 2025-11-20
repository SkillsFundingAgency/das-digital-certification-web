using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ModelStateExtensions
    {
        public static T GetAttemptedValueWhenInvalid<T>(this ModelStateDictionary modelState, string key, T defaultValue, T validValue)
        {
            if (modelState.IsValid || !modelState.TryGetValue(key, out ModelStateEntry entry))
            {
                return validValue;
            }

            if(entry.AttemptedValue == null)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(entry.AttemptedValue, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
