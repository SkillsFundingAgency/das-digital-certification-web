using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharedFrameworkCertificate
{
    [TestFixture]
    public class GetSharedFrameworkCertificateQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetSharedFrameworkCertificateQuery { Id = id };

            // Assert
            query.Id.Should().Be(id);
        }
    }
}
