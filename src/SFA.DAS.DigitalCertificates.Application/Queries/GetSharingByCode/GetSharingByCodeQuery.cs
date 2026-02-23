using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharingByCode
{
    public class GetSharingByCodeQuery : IRequest<GetSharingByCodeQueryResult?>
    {
        public required Guid Code { get; set; }
    }
}
