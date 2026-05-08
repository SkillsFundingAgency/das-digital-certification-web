using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetMatches;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetMatches
{
    [TestFixture]
    public class GetMatchesQueryTests
    {
        [Test]
        public void When_Set_UserId_Then_Property_Is_Set()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetMatchesQuery { UserId = id };

            // Assert
            query.UserId.Should().Be(id);
        }

    }
}
