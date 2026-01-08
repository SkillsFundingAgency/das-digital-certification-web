using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateById;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetCertificate
{
    [TestFixture]
    public class GetCertificateByIdQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var id = System.Guid.NewGuid();

            // Act
            var query = new GetCertificateByIdQuery { CertificateId = id };

            // Assert
            query.CertificateId.Should().Be(id);
        }
    }
}
