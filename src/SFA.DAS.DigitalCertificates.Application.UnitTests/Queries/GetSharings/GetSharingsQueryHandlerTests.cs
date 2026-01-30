using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharings;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharings
{
    [TestFixture]
    public class GetSharingsQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetSharingsQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetSharingsQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var limit = 15;

            var query = new GetSharingsQuery
            {
                UserId = userId,
                CertificateId = certificateId,
                Limit = limit
            };

            var expectedResponse = new GetSharingsResponse
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = "Software Developer",
                Sharings = new List<SharingItem>
                {
                    new SharingItem
                    {
                        SharingId = Guid.NewGuid(),
                        SharingNumber = 1,
                        CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified),
                        LinkCode = Guid.NewGuid(),
                        ExpiryTime = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Unspecified),
                        SharingAccess = new List<DateTime>
                        {
                            new DateTime(2024, 1, 2, 9, 0, 0, DateTimeKind.Unspecified)
                        },
                        SharingEmails = new List<SharingEmailItem>
                        {
                            new SharingEmailItem
                            {
                                SharingEmailId = Guid.NewGuid(),
                                EmailAddress = "test@example.com",
                                EmailLinkCode = Guid.NewGuid(),
                                SentTime = new DateTime(2024, 1, 1, 11, 0, 0, DateTimeKind.Unspecified),
                                SharingEmailAccess = new List<DateTime>
                                {
                                    new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Unspecified)
                                }
                            }
                        }
                    }
                }
            };

            _outerApiMock
                .Setup(x => x.GetSharings(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.CertificateId.Should().Be(certificateId);
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.CourseName.Should().Be("Software Developer");
            result.Sharings.Should().HaveCount(1);

            var sharing = result.Sharings[0];
            sharing.SharingNumber.Should().Be(1);
            sharing.SharingEmails.Should().HaveCount(1);
            sharing.SharingEmails[0].EmailAddress.Should().Be("test@example.com");

            _outerApiMock.Verify(x => x.GetSharings(
                userId.ToString(),
                certificateId,
                limit), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var query = new GetSharingsQuery
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                Limit = 5
            };

            _outerApiMock
                .Setup(x => x.GetSharings(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync((GetSharingsResponse)null!);

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var query = new GetSharingsQuery
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                Limit = 5
            };

            _outerApiMock
                .Setup(x => x.GetSharings(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("API failure"));

            // Act
            Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}