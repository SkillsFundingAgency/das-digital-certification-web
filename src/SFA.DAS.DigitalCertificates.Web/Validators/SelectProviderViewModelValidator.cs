using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class SelectProviderViewModelValidator : AbstractValidator<SelectProviderViewModel>
    {
        public const string SelectProviderErrorMessage = "Select your training provider or 'I don't know'";

        public SelectProviderViewModelValidator()
        {
            RuleFor(x => x.SelectedProviderName)
                .NotEmpty()
                .When(x => x.SelectedProviderUnknown != true)
                .WithMessage(SelectProviderErrorMessage);
        }
    }
}
