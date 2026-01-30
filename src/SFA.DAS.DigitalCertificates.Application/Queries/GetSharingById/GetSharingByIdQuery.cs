using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById
{
    public class GetSharingByIdQuery : IRequest<GetSharingByIdQueryResult?>
    {
        public required Guid SharingId { get; set; }
        public int? Limit { get; set; }
    }
}
