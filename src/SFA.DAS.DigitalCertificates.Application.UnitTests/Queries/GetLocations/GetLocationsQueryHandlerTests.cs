using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetLocations
{
	public class GetLocationsQueryHandlerTests
	{
		private Mock<IDigitalCertificatesOuterApi> _outerApi;
		private GetLocationsQueryHandler _sut;

		[SetUp]
		public void SetUp()
		{
			_outerApi = new Mock<IDigitalCertificatesOuterApi>();
			_sut = new GetLocationsQueryHandler(_outerApi.Object);
		}

		[Test]
		public async Task Handle_ReturnsMappedLocations_When_ApiReturnsAddresses()
		{
			// Arrange
			var apiResponse = new LocationsResponse
			{
				Addresses = new[]
				{
					new AddressResponse
					{
						Organisation = "Org",
						AddressLine1 = "Line1",
						AddressLine2 = "Line2",
						PostTown = "Town",
						Postcode = "AB1 2CD"
					}
				}
			};

			_outerApi.Setup(x => x.GetLocations(It.IsAny<string>())).ReturnsAsync(apiResponse);

			// Act
			var result = await _sut.Handle(new GetLocationsQuery { SearchTerm = "AB1" }, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			var locations = result.Locations.ToList();
			locations.Should().HaveCount(1);
			locations[0].Name.Should().Contain("Org").And.Contain("Line1");
			locations[0].Postcode.Should().Be("AB1 2CD");
		}

		[Test]
		public async Task Handle_ReturnsEmpty_When_ApiReturnsNull()
		{
			// Arrange
			_outerApi.Setup(x => x.GetLocations(It.IsAny<string>())).ReturnsAsync(new LocationsResponse { Addresses = null });

			// Act
			var result = await _sut.Handle(new GetLocationsQuery { SearchTerm = "none" }, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.Locations.Should().BeEmpty();
		}
	}
}

