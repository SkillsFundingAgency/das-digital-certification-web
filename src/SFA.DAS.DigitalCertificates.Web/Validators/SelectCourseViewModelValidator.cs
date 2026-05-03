using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class SelectCourseViewModelValidator : AbstractValidator<SelectCourseViewModel>
    {
        public const string SelectCourseErrorMessage = "Select your course or 'I don\'t know'";

        public SelectCourseViewModelValidator()
        {
            RuleFor(x => x.SelectedCourseCode)
                .NotEmpty()
                .When(x => x.SelectedCourseUnknown != true)
                .WithMessage(SelectCourseErrorMessage);
        }
    }
}
