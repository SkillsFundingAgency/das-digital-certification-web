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
            var location = new LocationResult { Name = "Name", Postcode = "PC1 1PC" };

            // Act
            var name = location.Name;
            var postcode = location.Postcode;

            // Assert
            name.Should().Be("Name");
            postcode.Should().Be("PC1 1PC");
        }
    }
}
