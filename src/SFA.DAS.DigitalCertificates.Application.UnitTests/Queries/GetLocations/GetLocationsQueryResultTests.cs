using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetLocations
{
    public class GetLocationsQueryResultTests
    {
        [Test]
        public void LocationResult_Properties_AreSettable()
        {
            // Arrange
            var location = new LocationResult
            {
                Name = "Name",
                Postcode = "PC1 1PC",
                Organisation = "Org",
                AddressLine1 = "Line1",
                AddressLine2 = "Line2",
                PostTown = "Town",
                County = "County"
            };

            // Act
            var name = location.Name;
            var postcode = location.Postcode;
            var organisation = location.Organisation;
            var line1 = location.AddressLine1;
            var line2 = location.AddressLine2;
            var postTown = location.PostTown;
            var county = location.County;

            // Assert
            name.Should().Be("Name");
            postcode.Should().Be("PC1 1PC");
            organisation.Should().Be("Org");
            line1.Should().Be("Line1");
            line2.Should().Be("Line2");
            postTown.Should().Be("Town");
            county.Should().Be("County");
        }
    }
}
