using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharingById
{
    public class GetSharingByIdQueryResultTests
    {
        [Test]
        public void When_SourceIsNull_Then_ResultIsNull()
        {
            // Arrange
            GetSharingByIdResponse? source = null;

            // Act
            GetSharingByIdQueryResult? result = source;

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_SourceHasValues_Then_MapsAllFields()
        {
            // Arrange
            var sharingEmailId = Guid.NewGuid();
            var linkCode = Guid.NewGuid();
            var source = new GetSharingByIdResponse
            {
                UserId = Guid.NewGuid(),
                CertificateId = Guid.NewGuid(),
                CertificateType = "Standard",
                CourseName = "Course",
                SharingId = Guid.NewGuid(),
                SharingNumber = 42,
                CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                LinkCode = linkCode,
                ExpiryTime = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc),
                SharingAccess = new List<DateTime> { new DateTime(2024, 1, 2, 9, 0, 0, DateTimeKind.Utc) },
                SharingEmails = new List<SharingEmailItem>
                {
                    new SharingEmailItem
                    {
                        SharingEmailId = sharingEmailId,
                        EmailAddress = "test@example.com",
                        EmailLinkCode = Guid.NewGuid(),
                        SentTime = new DateTime(2024,1,1,11,0,0, DateTimeKind.Utc),
                        SharingEmailAccess = new List<DateTime> { new DateTime(2024,1,2,10,0,0, DateTimeKind.Utc) }
                    }
                }
            };

            // Act
            GetSharingByIdQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.CertificateId.Should().Be(source.CertificateId);
            result.SharingId.Should().Be(source.SharingId);
            result.SharingNumber.Should().Be(source.SharingNumber);
            result.CreatedAt.Should().Be(source.CreatedAt);
            result.LinkCode.Should().Be(source.LinkCode);
            result.ExpiryTime.Should().Be(source.ExpiryTime);
            result.SharingAccess.Should().HaveCount(1);
            result.SharingEmails.Should().HaveCount(1);
            result.SharingEmails![0].SharingEmailId.Should().Be(sharingEmailId);
            result.CertificateType.ToString().Should().Be(source.CertificateType);
        }
    }
}