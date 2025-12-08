using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateCertificateSharing;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Commands.CreateCertificateSharing
{
    [TestFixture]
    public class CreateCertificateSharingCommandTests
    {
        [Test]
        public void Should_Convert_To_CreateCertificateSharingRequest_Successfully()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();

            var command = new CreateCertificateSharingCommand
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = "Software Developer"
            };

            CreateCertificateSharingRequest request = command;

            request.Should().NotBeNull();
            request.Userid.Should().Be(userId);
            request.CertificateId.Should().Be(certificateId);
            request.CertificateType.Should().Be("Standard");
            request.CourseName.Should().Be("Software Developer");
        }
    }
}