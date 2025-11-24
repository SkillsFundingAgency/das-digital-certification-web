using System.Net.Quic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUser;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetUser
{
    public class GetUserQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetUserQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetUserQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task When_UserReturnedFromApi_Then_ReturnsMappedUser()
        {
            // Arrange
            var query = new GetUserQuery { GovUkIdentifier = "gov-123" };

            var apiResponse = new UserResponse
            {
                GovUkIdentifier = query.GovUkIdentifier,
                EmailAddress = "user@test.com"
            };

            _outerApiMock
                .Setup(x => x.GetUser(query.GovUkIdentifier))
                .ReturnsAsync(apiResponse);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.GovUkIdentifier.Should().Be(apiResponse.GovUkIdentifier);
            result.EmailAddress.Should().Be(apiResponse.EmailAddress);

            _outerApiMock.Verify(x => x.GetUser(query.GovUkIdentifier), Times.Once);
        }
    }
}