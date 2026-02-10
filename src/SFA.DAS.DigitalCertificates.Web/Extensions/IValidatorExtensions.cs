using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SFA.DAS.DigitalCertificates.Web.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class IValidatorExtensions
    {
        public static async Task<bool> ModelStateIsValid<T>(this IValidator<T> validator, T model, ModelStateDictionary modelState)
        {
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    if (!modelState.ContainsKey(error.PropertyName) || modelState[error.PropertyName]?.Errors.Count == 0)
                        modelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
            }

            return modelState.IsValid;
        }
    }
}
