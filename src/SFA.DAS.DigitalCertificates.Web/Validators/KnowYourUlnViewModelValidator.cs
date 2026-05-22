using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class KnowYourUlnViewModelValidator : AbstractValidator<KnowYourUlnViewModel>
    {
        public const string SelectYesIfYouKnowError = "Select 'Yes' if you know your unique learner number";
        public const string EnterUlnError = "Enter your unique learner number";
        public const string InvalidUlnFormatError = "Enter a unique learner number that is 10 digits and contains only numbers";

        public KnowYourUlnViewModelValidator()
        {
            RuleFor(x => x.KnowUln)
                .NotNull()
                .WithMessage(SelectYesIfYouKnowError);

            RuleFor(x => x.Uln)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(EnterUlnError)
                .Must(u => u.HasValue && u.Value.ToString().Length == 10).WithMessage(InvalidUlnFormatError)
                .When(x => x.KnowUln == true);
        }
    }
}
