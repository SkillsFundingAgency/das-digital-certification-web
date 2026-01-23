using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetFrameworkCertificate
{
    [TestFixture]
    public class GetFrameworkCertificateQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetFrameworkCertificateQuery { CertificateId = id };

            // Assert
            query.CertificateId.Should().Be(id);
        }
    }
}
