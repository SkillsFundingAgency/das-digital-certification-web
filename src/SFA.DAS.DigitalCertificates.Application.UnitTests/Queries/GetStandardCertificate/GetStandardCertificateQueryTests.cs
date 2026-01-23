using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetStandardCertificate
{
    [TestFixture]
    public class GetStandardCertificateQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetStandardCertificateQuery { CertificateId = id };

            // Assert
            query.CertificateId.Should().Be(id);
        }
    }
}
