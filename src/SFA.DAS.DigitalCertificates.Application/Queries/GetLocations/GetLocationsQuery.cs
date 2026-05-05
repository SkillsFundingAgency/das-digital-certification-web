using MediatR;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetLocations
{
    public class GetLocationsQuery : IRequest<GetLocationsQueryResult>
    {
        public required string SearchTerm { get; set; }
    }
}
