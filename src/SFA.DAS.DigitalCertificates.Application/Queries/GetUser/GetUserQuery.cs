using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetUser
{
    public class GetUserQuery : IRequest<User>
    {
        public required string GovUkIdentifier { get; set; }
    }
}
