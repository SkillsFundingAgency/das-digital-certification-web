
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
        public string? Organisation { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? PostTown { get; set; }
        public string? County { get; set; }
    }
}
