using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.AuthoriseUser
{
    public class AuthoriseUserCommandHandler : IRequestHandler<AuthoriseUserCommand, Unit>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public AuthoriseUserCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<Unit> Handle(AuthoriseUserCommand request, CancellationToken cancellationToken)
        {
            var req = (AuthoriseUserRequest)request;
            await _outerApi.AuthoriseUser(request.UserId, req);
            return Unit.Value;
        }
    }
}
