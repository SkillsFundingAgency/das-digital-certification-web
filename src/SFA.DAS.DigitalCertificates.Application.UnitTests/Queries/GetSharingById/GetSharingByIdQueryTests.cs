using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharingById
{
    [TestFixture]
    public class GetSharingByIdQueryTests
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            // Arrange
            var sharingId = Guid.NewGuid();
            var limit = 10;

            // Act
            var query = new GetSharingByIdQuery
            {
                SharingId = sharingId,
                Limit = limit
            };

            // Assert
            query.SharingId.Should().Be(sharingId);
            query.Limit.Should().Be(limit);
        }
    }
}
