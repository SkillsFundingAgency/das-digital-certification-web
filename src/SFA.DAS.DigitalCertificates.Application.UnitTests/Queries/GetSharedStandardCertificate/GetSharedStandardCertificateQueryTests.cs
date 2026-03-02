using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharedStandardCertificate
{
    [TestFixture]
    public class GetSharedStandardCertificateQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetSharedStandardCertificateQuery { Id = id };

            // Assert
            query.Id.Should().Be(id);
        }
    }
}
