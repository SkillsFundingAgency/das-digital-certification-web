using System;
using System.Text.RegularExpressions;
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
                .NotEmpty()
                .WithMessage(EnterUlnError)
                .Must(uln =>
                    uln != null &&
                    Regex.IsMatch(uln.Trim(), @"\A[0-9]{10}\z", RegexOptions.None, TimeSpan.FromMilliseconds(100)))
                .WithMessage(InvalidUlnFormatError)
                .When(x => x.KnowUln == true);
        }
    }
}
