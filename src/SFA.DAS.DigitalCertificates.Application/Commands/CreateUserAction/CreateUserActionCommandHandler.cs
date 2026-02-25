using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction
{
    public class CreateUserActionCommandHandler : IRequestHandler<CreateUserActionCommand, CreateUserActionCommandResult?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public CreateUserActionCommandHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<CreateUserActionCommandResult?> Handle(CreateUserActionCommand request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.CreateUserAction(request.UserId, request);

            if (response == null)
                return null;

            return response;
        }
    }
}
