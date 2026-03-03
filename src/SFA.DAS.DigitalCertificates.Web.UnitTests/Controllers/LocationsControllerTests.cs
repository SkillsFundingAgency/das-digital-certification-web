using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    public class LocationsControllerTests
    {
        private Mock<ILocationsOrchestrator> _orchestrator;
        private LocationsController _sut;

        [SetUp]
        public void SetUp()
        {
            _orchestrator = new Mock<ILocationsOrchestrator>();
            _sut = new LocationsController(_orchestrator.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut?.Dispose();
        }

        [Test]
        public async Task Get_ReturnsJson_WithLocationsNames()
        {
            // Arrange
            var resultModel = new GetLocationsQueryResult
            {
                Locations = new[] { new LocationResult { Name = "Name 1" }, new LocationResult { Name = "Name 2" } }
            };

            _orchestrator.Setup(x => x.GetLocations("term")).ReturnsAsync(resultModel);

            // Act
            var actionResult = await _sut.Get("term");

            // Assert
            actionResult.Should().BeOfType<JsonResult>();
            var json = (JsonResult)actionResult;
            json.Value.Should().NotBeNull();

            var dict = json.Value as dynamic;
            var locations = ((System.Collections.IEnumerable)dict.locations).Cast<object>().ToList();
            locations.Should().HaveCount(2);
        }
    }
}
