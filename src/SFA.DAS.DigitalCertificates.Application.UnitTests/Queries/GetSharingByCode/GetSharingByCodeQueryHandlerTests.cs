using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingByCode;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharingByCode
{
    [TestFixture]
    public class GetSharingByCodeQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetSharingByCodeQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetSharingByCodeQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var code = Guid.NewGuid();

            var response = new GetSharingCodeResponse
            {
                CertificateId = Guid.NewGuid(),
                CertificateType = "Standard",
                ExpiryTime = new DateTime(2024, 12, 31),
                SharingId = Guid.NewGuid(),
                SharingEmailId = Guid.NewGuid()
            };

            _outerApiMock
                .Setup(x => x.GetSharingByCode(code))
                .ReturnsAsync(response);

            var query = new GetSharingByCodeQuery { Code = code };

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.CertificateId.Should().Be(response.CertificateId);
            result.CertificateType.ToString().Should().Be(response.CertificateType);
            result.ExpiryTime.Should().Be(response.ExpiryTime);
            result.SharingId.Should().Be(response.SharingId);
            result.SharingEmailId.Should().Be(response.SharingEmailId);

            _outerApiMock.Verify(x => x.GetSharingByCode(code), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var code = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetSharingByCode(It.IsAny<Guid>()))
                .ReturnsAsync((GetSharingCodeResponse)null!);

            var query = new GetSharingByCodeQuery { Code = code };

            // Act
            var result = await _sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var code = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetSharingByCode(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("API failure"));

            var query = new GetSharingByCodeQuery { Code = code };

            // Act
            Func<Task> act = async () => await _sut.Handle(query, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
