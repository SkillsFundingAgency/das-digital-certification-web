using FluentValidation;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetUser
{
    public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
    {
        public GetUserQueryValidator()
        {
            RuleFor(x => x.GovUkIdentifier)
                .NotEmpty();
        }
    }
}
