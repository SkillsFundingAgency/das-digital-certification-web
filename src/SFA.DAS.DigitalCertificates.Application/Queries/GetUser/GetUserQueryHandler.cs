using FluentValidation;
using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetUser
{
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, User?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;
        private readonly IValidator<GetUserQuery> _validator;

        public GetUserQueryHandler(IDigitalCertificatesOuterApi outerApi, IValidator<GetUserQuery> validator)
        {
            _outerApi = outerApi;
            _validator = validator;
        }

        public async Task<User?> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            await _validator.ValidateAsync(request, cancellationToken);

            User user = await _outerApi.GetUser(request.GovUkIdentifier);

            return user;
        }
    }
}
