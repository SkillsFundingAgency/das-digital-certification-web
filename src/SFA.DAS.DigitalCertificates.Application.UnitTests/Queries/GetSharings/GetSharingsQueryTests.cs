using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharings;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharings
{
    [TestFixture]
    public class GetSharingsQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var limit = 15;

            // Act
            var query = new GetSharingsQuery
            {
                UserId = userId,
                CertificateId = certificateId,
                Limit = limit
            };

            // Assert
            query.UserId.Should().Be(userId);
            query.CertificateId.Should().Be(certificateId);
            query.Limit.Should().Be(limit);
        }
    }
}