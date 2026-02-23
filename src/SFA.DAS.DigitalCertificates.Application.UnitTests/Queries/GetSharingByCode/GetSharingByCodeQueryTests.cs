using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingByCode;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharingByCode
{
    [TestFixture]
    public class GetSharingByCodeQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var code = Guid.NewGuid();

            // Act
            var query = new GetSharingByCodeQuery { Code = code };

            // Assert
            query.Code.Should().Be(code);
        }
    }
}
