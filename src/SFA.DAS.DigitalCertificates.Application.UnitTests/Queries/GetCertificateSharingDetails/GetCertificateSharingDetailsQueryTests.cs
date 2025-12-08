using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateSharingDetails;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetCertificateSharingDetails
{
    [TestFixture]
    public class GetCertificateSharingDetailsQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
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

            query.UserId.Should().Be(userId);
            query.CertificateId.Should().Be(certificateId);
            query.Limit.Should().Be(limit);
        }
    }
}