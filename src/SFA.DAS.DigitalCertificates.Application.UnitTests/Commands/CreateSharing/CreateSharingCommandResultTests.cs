using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateSharing
{
    [TestFixture]
    public class CreateSharingCommandResultTests
    {
        [Test]
        public void Should_Convert_From_CreateSharingResponse_Successfully()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var linkCode = Guid.NewGuid();
            var createdAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified);
            var expiryTime = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Unspecified);

            var response = new CreateSharingResponse
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

            CreateSharingCommandResult? result = response;

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
        }
    }
}