using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    public class LocationsOrchestratorTests
    {
        private Mock<IMediator> _mediator;
        private LocationsOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediator = new Mock<IMediator>();
            _sut = new LocationsOrchestrator(_mediator.Object);
        }

        [Test]
        public async Task GetLocations_ForwardsRequestToMediator()
        {
            // Arrange
            var expected = new GetLocationsQueryResult();
            _mediator.Setup(m => m.Send(It.IsAny<GetLocationsQuery>(), default)).ReturnsAsync(expected);

            // Act
            var result = await _sut.GetLocations("term");

            // Assert
            result.Should().BeSameAs(expected);
            _mediator.Verify(m => m.Send(It.Is<GetLocationsQuery>(q => q.SearchTerm == "term"), default), Times.Once);
        }
    }
}
