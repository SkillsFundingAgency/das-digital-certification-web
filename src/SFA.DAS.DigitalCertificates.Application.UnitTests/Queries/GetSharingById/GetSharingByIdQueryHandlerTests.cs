using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharingById
{
    [TestFixture]
    public class GetSharingByIdQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetSharingByIdQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetSharingByIdQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var limit = 5;

            var response = new GetSharingByIdResponse
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                CertificateType = "Standard",
                CourseName = "Course",
                SharingId = sharingId,
                SharingNumber = 1,
                CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified),
                LinkCode = Guid.NewGuid(),
                ExpiryTime = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Unspecified),
                SharingAccess = new List<DateTime> { new DateTime(2024, 1, 2, 9, 0, 0) },
                SharingEmails = new List<SharingEmailItem>
                {
                    new SharingEmailItem
                    {
                        SharingEmailId = Guid.NewGuid(),
                        EmailAddress = "test@example.com",
                        EmailLinkCode = Guid.NewGuid(),
                        SentTime = new DateTime(2024,1,1,11,0,0),
                        SharingEmailAccess = new List<DateTime> { new DateTime(2024,1,2,10,0,0) }
                    }
                }
            };

            _outerApiMock
                .Setup(x => x.GetSharingById(sharingId, limit))
                .ReturnsAsync(response);

            var query = new GetSharingByIdQuery { SharingId = sharingId, Limit = limit };

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.SharingId.Should().Be(sharingId);
            result.CertificateId.Should().Be(response.CertificateId);
            result.CertificateType.ToString().Should().Be(response.CertificateType);
            result.SharingEmails.Should().HaveCount(1);

            _outerApiMock.Verify(x => x.GetSharingById(sharingId, limit), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var sharingId = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetSharingById(It.IsAny<Guid>(), It.IsAny<int?>()))
                .ReturnsAsync((GetSharingByIdResponse)null!);

            var query = new GetSharingByIdQuery { SharingId = sharingId, Limit = null };

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var sharingId = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetSharingById(It.IsAny<Guid>(), It.IsAny<int?>()))
                .ThrowsAsync(new Exception("API failure"));

            var query = new GetSharingByIdQuery { SharingId = sharingId, Limit = null };

            // Act
            Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}