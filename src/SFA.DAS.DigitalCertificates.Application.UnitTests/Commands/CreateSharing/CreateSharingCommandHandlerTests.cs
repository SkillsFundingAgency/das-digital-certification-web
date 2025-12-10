using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands
{
    [TestFixture]
    public class CreateSharingCommandHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private CreateSharingCommandHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new CreateSharingCommandHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Request_And_Returns_Result()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var linkCode = Guid.NewGuid();
            var createdAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);
            var expiryTime = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Unspecified);

            var command = new CreateSharingCommand
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = "Software Developer"
            };

            var expectedResponse = new CreateSharingResponse
            {
                Userid = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = "Software Developer",
                SharingId = sharingId,
                SharingNumber = 1,
                CreatedAt = createdAt,
                LinkCode = linkCode,
                ExpiryTime = expiryTime
            };

            _outerApiMock
                .Setup(x => x.CreateSharing(It.IsAny<CreateSharingRequest>()))
                .ReturnsAsync(expectedResponse);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Userid.Should().Be(userId);
            result.CertificateId.Should().Be(certificateId);
            result.CertificateType.Should().Be("Standard");
            result.CourseName.Should().Be("Software Developer");
            result.SharingId.Should().Be(sharingId);
            result.SharingNumber.Should().Be(1);
            result.CreatedAt.Should().Be(createdAt);
            result.LinkCode.Should().Be(linkCode);
            result.ExpiryTime.Should().Be(expiryTime);

            _outerApiMock.Verify(x => x.CreateSharing(
                It.Is<CreateSharingRequest>(r =>
                    r.Userid == command.UserId &&
                    r.CertificateId == command.CertificateId &&
                    r.CertificateType == command.CertificateType &&
                    r.CourseName == command.CourseName
                )), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            var command = new CreateSharingCommand
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                CertificateType = "Framework",
                CourseName = "Business Administration"
            };

            _outerApiMock
                .Setup(x => x.CreateSharing(It.IsAny<CreateSharingRequest>()))
                .ReturnsAsync((CreateSharingResponse)null!);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.Should().BeNull();
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            var command = new CreateSharingCommand
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                CertificateType = "Standard",
                CourseName = "Data Science"
            };

            _outerApiMock
                .Setup(x => x.CreateSharing(It.IsAny<CreateSharingRequest>()))
                .ThrowsAsync(new Exception("API failure"));

            Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }

        [Test]
        public async Task Handle_Uses_Implicit_Conversion_From_Command_To_Request()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();

            var command = new CreateSharingCommand
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Framework",
                CourseName = "Digital Marketing"
            };

            var expectedResponse = new CreateSharingResponse
            {
                Userid = userId,
                CertificateId = certificateId,
                CertificateType = "Framework",
                CourseName = "Digital Marketing",
                SharingId = Guid.NewGuid(),
                SharingNumber = 2,
                CreatedAt = DateTime.Now,
                LinkCode = Guid.NewGuid(),
                ExpiryTime = DateTime.Now.AddDays(30)
            };

            _outerApiMock
                .Setup(x => x.CreateSharing(It.IsAny<CreateSharingRequest>()))
                .ReturnsAsync(expectedResponse);

            var result = await _sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();

            _outerApiMock.Verify(x => x.CreateSharing(
                It.Is<CreateSharingRequest>(r =>
                    r.Userid == userId &&
                    r.CertificateId == certificateId &&
                    r.CertificateType == "Framework" &&
                    r.CourseName == "Digital Marketing"
                )), Times.Once);
        }
    }
}