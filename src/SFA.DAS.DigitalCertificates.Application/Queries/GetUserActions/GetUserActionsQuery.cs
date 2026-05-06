using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions
{
    public class GetUserActionsQuery : IRequest<GetUserActionsQueryResult>
    {
        public required Guid UserId { get; set; }
    }
}
