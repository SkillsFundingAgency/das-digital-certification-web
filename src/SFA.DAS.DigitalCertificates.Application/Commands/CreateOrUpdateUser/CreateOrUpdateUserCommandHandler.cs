using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateOrUpdateUser
{
    public class CreateOrUpdateUserCommandHandler : IRequestHandler<CreateOrUpdateUserCommand, Guid>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public CreateOrUpdateUserCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<Guid> Handle(CreateOrUpdateUserCommand command, CancellationToken cancellationToken)
        {
            var userId = await _outerApi.CreateOrUpdateUser(new CreateOrUpdateUserRequest
            {
                GovUkIdentifier = command.GovUkIdentifier,
                EmailAddress = command.EmailAddress,
                PhoneNumber = command.PhoneNumber,
                Names = command.Names,
                DateOfBirth = command.DateOfBirth
            });

            return userId;
        }
    }
}
