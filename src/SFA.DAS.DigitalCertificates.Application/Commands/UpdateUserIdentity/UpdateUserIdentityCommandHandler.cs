using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.UpdateUserIdentity
{
    public class UpdateUserIdentityCommandHandler : IRequestHandler<UpdateUserIdentityCommand, Unit>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public UpdateUserIdentityCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<Unit> Handle(UpdateUserIdentityCommand command, CancellationToken cancellationToken)
        {
            await _outerApi.UpdateUserIdentity(command.UserId, new UpdateUserIdentityRequest
            {
                Names = command.Names?.Select(name =>
                    new Infrastructure.Api.Types.Name
                    {
                        ValidSince = name.ValidSince,
                        ValidUntil = name.ValidUntil,
                        FamilyName = name.FamilyName,
                        GivenNames = name.GivenNames
                    })
                    .ToList(),
                DateOfBirth = command.DateOfBirth
            });

            return Unit.Value;
        }
    }
}
