using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetUser
{
    public class GetUserQuery : IRequest<UserResponse>
    {
        public required string GovUkIdentifier { get; set; }
    }
}
