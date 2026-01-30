using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing
{
    public class CreateSharingCommand : IRequest<CreateSharingCommandResult?>
    {
        public required Guid UserId { get; set; }
        public required Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
        public required string CourseName { get; set; }

        public static implicit operator CreateSharingRequest(CreateSharingCommand command)
        {
            return new CreateSharingRequest
            {
                Userid = command.UserId,
                CertificateId = command.CertificateId,
                CertificateType = command.CertificateType.ToString(),
                CourseName = command.CourseName
            };
        }
    }
}