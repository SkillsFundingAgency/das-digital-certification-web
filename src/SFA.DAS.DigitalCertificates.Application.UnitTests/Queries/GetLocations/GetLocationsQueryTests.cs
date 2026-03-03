using FluentAssertions;
using MediatR;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetLocations;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetLocations
{
    public class GetLocationsQueryTests
    {
        [Test]
        public void GetLocationsQuery_Is_IRequest_For_Result()
        {
            // Arrange
            var query = new GetLocationsQuery { SearchTerm = "abc" };

            // Act
            var searchTerm = query.SearchTerm;
            var isRequest = query is IRequest<GetLocationsQueryResult>;

            // Assert
            searchTerm.Should().Be("abc");
            isRequest.Should().BeTrue();
        }
    }
}
