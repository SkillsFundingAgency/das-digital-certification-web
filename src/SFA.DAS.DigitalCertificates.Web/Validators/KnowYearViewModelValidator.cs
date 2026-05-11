using System;
using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class KnowYearViewModelValidator : AbstractValidator<KnowYearViewModel>
    {
        public const string SelectYesIfYouKnowError = "Select 'Yes' if you know the year you completed your apprenticeship";
        public const string EnterYearError = "Enter the year you completed your apprenticeship";
        public const string InvalidYearFormatError = "Enter a year that is 4 digits and contains only numbers";
        public const string YearLaterThanCurrentError = "The date you completed your apprenticeship must be no later than the current year";
        public const string YearEarlierThan2012Error = "The date you completed your apprenticeship must no earlier than 2012";

        public KnowYearViewModelValidator()
        {
            RuleFor(x => x.KnowYear)
                .NotNull()
                .WithMessage(SelectYesIfYouKnowError);

            RuleFor(x => x.YearCompleted)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage(EnterYearError)
                .Must(y => y.HasValue && y.Value.ToString().Length == 4).WithMessage(InvalidYearFormatError)
                .Must(y => y.HasValue && y.Value <= DateTime.UtcNow.Year).WithMessage(YearLaterThanCurrentError)
                .Must(y => y.HasValue && y.Value >= 2012).WithMessage(YearEarlierThan2012Error)
                .When(x => x.KnowYear == true);
        }
    }
}
