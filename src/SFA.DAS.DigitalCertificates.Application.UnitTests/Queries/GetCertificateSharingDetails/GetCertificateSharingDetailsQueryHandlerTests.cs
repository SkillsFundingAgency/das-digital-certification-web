using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateSharingDetails;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetCertificateSharingDetails
{
    [TestFixture]
    public class GetCertificateSharingDetailsQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetCertificateSharingDetailsQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetCertificateSharingDetailsQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var limit = 15;

            var query = new GetCertificateSharingDetailsQuery
            {
                UserId = userId,
                CertificateId = certificateId,
                Limit = limit
            };

            var expectedResponse = new GetCertificateSharingDetailsResponse
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
                .Setup(x => x.GetCertificateSharings(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(expectedResponse);

            var result = await _sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.CertificateId.Should().Be(certificateId);
            result.CertificateType.Should().Be("Standard");
            result.CourseName.Should().Be("Software Developer");
            result.Sharings.Should().HaveCount(1);

            var sharing = result.Sharings[0];
            sharing.SharingNumber.Should().Be(1);
            sharing.SharingEmails.Should().HaveCount(1);
            sharing.SharingEmails[0].EmailAddress.Should().Be("test@example.com");

            _outerApiMock.Verify(x => x.GetCertificateSharings(
                userId.ToString(),
                certificateId,
                limit), Times.Once);
        }

        [Test]
        public async Task Handle_Uses_Default_Limit_When_Not_Specified()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();

            var query = new GetCertificateSharingDetailsQuery
            {
                UserId = userId,
                CertificateId = certificateId
            };

            var expectedResponse = new GetCertificateSharingDetailsResponse
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Framework",
                CourseName = "Business Administration"
            };

            _outerApiMock
                .Setup(x => x.GetCertificateSharings(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync(expectedResponse);

            var result = await _sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();

            _outerApiMock.Verify(x => x.GetCertificateSharings(
                userId.ToString(),
                certificateId,
                10), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            var query = new GetCertificateSharingDetailsQuery
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                Limit = 5
            };

            _outerApiMock
                .Setup(x => x.GetCertificateSharings(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ReturnsAsync((GetCertificateSharingDetailsResponse)null!);

            var result = await _sut.Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            var query = new GetCertificateSharingDetailsQuery
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                Limit = 5
            };

            _outerApiMock
                .Setup(x => x.GetCertificateSharings(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("API failure"));

            Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}