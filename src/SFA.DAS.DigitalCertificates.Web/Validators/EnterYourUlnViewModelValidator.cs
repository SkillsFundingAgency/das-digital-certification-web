using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class EnterYourUlnViewModelValidator : AbstractValidator<EnterYourUlnViewModel>
    {
        public const string EnterUlnError = "Enter your unique learner number";
        public const string InvalidUlnFormatError = "Enter a unique learner number that is 10 digits and contains only numbers";

        public EnterYourUlnViewModelValidator()
        {
            RuleFor(x => x.Uln)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(EnterUlnError)
                .Must(u => u.HasValue && u.Value.ToString().Length == 10).WithMessage(InvalidUlnFormatError);
        }
    }
}
