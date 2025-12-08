using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateCertificateSharing
{
    public class CreateCertificateSharingCommand : IRequest<CreateCertificateSharingCommandResult?>
    {
        public required Guid UserId { get; set; }
        public required Guid CertificateId { get; set; }
        public required string CertificateType { get; set; }
        public required string CourseName { get; set; }

        public static implicit operator CreateCertificateSharingRequest(CreateCertificateSharingCommand command)
        {
            return new CreateCertificateSharingRequest
            {
                Userid = command.UserId,
                CertificateId = command.CertificateId,
                CertificateType = command.CertificateType,
                CourseName = command.CourseName
            };
        }
    }
}