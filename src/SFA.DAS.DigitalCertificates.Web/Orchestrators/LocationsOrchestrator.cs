using MediatR;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class LocationsOrchestrator : ILocationsOrchestrator
    {
        private readonly IMediator _mediator;

        public LocationsOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<GetLocationsQueryResult> GetLocations(string searchTerm)
        {
            return await _mediator.Send(new GetLocationsQuery { SearchTerm = searchTerm });
        }
    }
}
