using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetLocations
{
    public class GetLocationsQueryHandler : IRequestHandler<GetLocationsQuery, GetLocationsQueryResult>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetLocationsQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<GetLocationsQueryResult> Handle(GetLocationsQuery request, CancellationToken cancellationToken)
        {
            var response = await _outerApi.GetLocations(request.SearchTerm);

            var result = new GetLocationsQueryResult
            {
                Locations = response?.Addresses?.Select(a => new LocationResult
                {
                    Name = BuildDisplayName(a),
                    Postcode = a.Postcode
                }) ?? Enumerable.Empty<LocationResult>()
            };

            return result;
        }

        private static string BuildDisplayName(SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses.AddressResponse a)
        {
            var parts = new List<string?>();

            if (!string.IsNullOrWhiteSpace(a.Organisation)) parts.Add(a.Organisation);
            if (!string.IsNullOrWhiteSpace(a.AddressLine1)) parts.Add(a.AddressLine1);
            if (!string.IsNullOrWhiteSpace(a.AddressLine2)) parts.Add(a.AddressLine2);
            if (!string.IsNullOrWhiteSpace(a.AddressLine3)) parts.Add(a.AddressLine3);
            if (!string.IsNullOrWhiteSpace(a.Locality)) parts.Add(a.Locality);
            if (!string.IsNullOrWhiteSpace(a.PostTown)) parts.Add(a.PostTown);
            if (!string.IsNullOrWhiteSpace(a.County)) parts.Add(a.County);
            if (!string.IsNullOrWhiteSpace(a.Postcode)) parts.Add(a.Postcode);

            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
    }
}
