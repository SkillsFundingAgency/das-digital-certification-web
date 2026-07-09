using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Commands.UpdateUserIdentity
{
    public class UpdateUserIdentityCommand : IRequest<Unit>
    {
        public Guid UserId { get; set; }
        public List<Name>? Names { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
