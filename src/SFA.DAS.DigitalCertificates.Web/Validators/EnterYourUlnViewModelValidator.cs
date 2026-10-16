using System.Text.RegularExpressions;
using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class EnterYourUlnViewModelValidator
        : AbstractValidator<EnterYourUlnViewModel>
    {
        public const string EnterUlnError = "Enter your unique learner number";

        public const string InvalidUlnFormatError = "Enter a unique learner number that is 10 digits and contains only numbers";

        public EnterYourUlnViewModelValidator()
        {
            RuleFor(x => x.Uln)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(EnterUlnError)
                .Must(uln =>
                    uln != null &&
                    Regex.IsMatch(uln.Trim(), @"\A[0-9]{10}\z"))
                .WithMessage(InvalidUlnFormatError);
        }
    }
}