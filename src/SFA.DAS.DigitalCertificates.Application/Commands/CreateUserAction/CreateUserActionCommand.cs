using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction
{
    public class CreateUserActionCommand : IRequest<CreateUserActionCommandResult?>
    {
        public Guid UserId { get; set; }
        public required ActionType ActionType { get; set; }
        public required string FamilyName { get; set; }
        public required string GivenNames { get; set; }
        public Guid? CertificateId { get; set; }
        public CertificateType? CertificateType { get; set; }
        public string? CourseName { get; set; }

        public static implicit operator CreateUserActionRequest(CreateUserActionCommand command)
        {
            return new CreateUserActionRequest
            {
                ActionType = command.ActionType.ToString(),
                FamilyName = command.FamilyName,
                GivenNames = command.GivenNames,
                CertificateId = command.CertificateId,
                CertificateType = command.CertificateType?.ToString(),
                CourseName = command.CourseName
            };
        }
    }
}
