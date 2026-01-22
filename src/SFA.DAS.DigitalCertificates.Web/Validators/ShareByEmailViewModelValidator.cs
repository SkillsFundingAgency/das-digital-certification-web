using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Sharing;

namespace SFA.DAS.DigitalCertificates.Web.Validators
{
    public class ShareByEmailViewModelValidator : AbstractValidator<ShareByEmailViewModel>
    {
        public const string InvalidDomainErrorMessage = "Enter an email address with a valid domain";
        public const string NoEmailErrorMessage = "Enter a valid email address";
        public const string InvalidEmailErrorMessage = "Enter an email address";

        public ShareByEmailViewModelValidator()
        {
            RuleFor(x => x.EmailAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(NoEmailErrorMessage)
                .Matches(RegularExpressions.EmailRegex)
                .WithMessage(InvalidEmailErrorMessage)
                .MustAsync(async (email, cancellationToken) => await EmailCheckingService.IsValidDomain(email))
                .WithMessage(InvalidDomainErrorMessage);
        }
    }
}
