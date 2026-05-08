using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetUserActions
{
    [TestFixture]
    public class GetUserActionsQueryTests
    {
        [Test]
        public void When_Set_UserId_Then_Property_Is_Set()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var query = new GetUserActionsQuery { UserId = id };

            // Assert
            query.UserId.Should().Be(id);
        }
    }
}
