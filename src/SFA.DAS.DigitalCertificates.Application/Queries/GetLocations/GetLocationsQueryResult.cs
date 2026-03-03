
namespace SFA.DAS.DigitalCertificates.Application.Queries.GetLocations
{
    public class GetLocationsQueryResult
    {
        public IEnumerable<LocationResult> Locations { get; set; } = new List<LocationResult>();
    }

    public class LocationResult
    {
        public string? Name { get; set; }
        public string? Postcode { get; set; }
    }
}
