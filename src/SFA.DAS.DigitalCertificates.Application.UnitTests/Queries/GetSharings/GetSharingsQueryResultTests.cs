using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharings;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharings
{
    [TestFixture]
    public class GetSharingsQueryResultTests
    {
        [Test]
        public void Should_Convert_From_GetSharingsResponse_Successfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var linkCode = Guid.NewGuid();
            var sharingEmailId = Guid.NewGuid();
            var emailLinkCode = Guid.NewGuid();

            var response = new GetSharingsResponse
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = "Software Developer",
                Sharings = new List<SharingItem>
                {
                    new SharingItem
                    {
                        SharingId = sharingId,
                        SharingNumber = 1,
                        CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified),
                        LinkCode = linkCode,
                        ExpiryTime = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Unspecified),
                        SharingAccess = new List<DateTime>
                        {
                            new DateTime(2024, 1, 2, 9, 0, 0, DateTimeKind.Unspecified),
                            new DateTime(2024, 1, 3, 14, 30, 0, DateTimeKind.Unspecified)
                        },
                        SharingEmails = new List<SharingEmailItem>
                        {
                            new SharingEmailItem
                            {
                                SharingEmailId = sharingEmailId,
                                EmailAddress = "test@example.com",
                                EmailLinkCode = emailLinkCode,
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

            // Act
            GetSharingsQueryResult? result = response;

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.CertificateId.Should().Be(certificateId);
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.CourseName.Should().Be("Software Developer");

            result.Sharings.Should().HaveCount(1);
            var sharing = result.Sharings![0];
            sharing.SharingId.Should().Be(sharingId);
            sharing.SharingNumber.Should().Be(1);
            sharing.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Unspecified));
            sharing.LinkCode.Should().Be(linkCode);
            sharing.ExpiryTime.Should().Be(new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Unspecified));
            sharing.SharingAccess.Should().HaveCount(2);
            sharing.SharingAccess![0].Should().Be(new DateTime(2024, 1, 2, 9, 0, 0, DateTimeKind.Unspecified));
            sharing.SharingAccess[1].Should().Be(new DateTime(2024, 1, 3, 14, 30, 0, DateTimeKind.Unspecified));

            sharing.SharingEmails.Should().HaveCount(1);
            var email = sharing.SharingEmails![0];
            email.SharingEmailId.Should().Be(sharingEmailId);
            email.EmailAddress.Should().Be("test@example.com");
            email.EmailLinkCode.Should().Be(emailLinkCode);
            email.SentTime.Should().Be(new DateTime(2024, 1, 1, 11, 0, 0, DateTimeKind.Unspecified));
            email.SharingEmailAccess.Should().HaveCount(1);
            email.SharingEmailAccess![0].Should().Be(new DateTime(2024, 1, 2, 10, 0, 0, DateTimeKind.Unspecified));
        }
    }
}