using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingByCode;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharingByCode
{
    [TestFixture]
    public class GetSharingByCodeQueryResultTests
    {
        [Test]
        public void When_SourceHasValues_Then_MapsAllFields()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var sharingEmailId = Guid.NewGuid();
            var source = new GetSharingCodeResponse
            {
                CertificateId = Guid.NewGuid(),
                CertificateType = "Standard",
                ExpiryTime = new DateTime(2024, 12, 31),
                SharingId = sharingId,
                SharingEmailId = sharingEmailId
            };

            // Act
            GetSharingByCodeQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.CertificateId.Should().Be(source.CertificateId);
            result.CertificateType.Should().Be(Enum.Parse<CertificateType>(source.CertificateType));
            result.ExpiryTime.Should().Be(source.ExpiryTime);
            result.SharingId.Should().Be(sharingId);
            result.SharingEmailId.Should().Be(sharingEmailId);
        }
    }
}
