using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.RequestPrintCertificate
{
    public class CreatePrintRequestCommand : IRequest
    {
        public Guid CertificateId { get; set; }
        public required CreatePrintRequest Request { get; set; }
    }
}
