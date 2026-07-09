using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetMatches;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetMatches
{
    [TestFixture]
    public class GetMatchesQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetMatchesQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetMatchesQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            var response = new MatchesResponse
            {
                Matches =
                {
                    new MatchResponse
                    {
                        Uln = 1234567890,
                        UserIdentityId = userIdentityId,
                        CertificateType = "Standard",
                        CourseCode = "C1",
                        CourseName = "Course",
                        CourseLevel = "2",
                        DateAwarded = DateTime.UtcNow.Date,
                        ProviderName = "Provider",
                        Ukprn = 100000
                    }
                },
                Masks =
                {
                    new MaskResponse
                    {
                        CertificateType = "Framework",
                        CourseCode = "F1",
                        CourseName = "FrameworkCourse",
                        CourseLevel = "3",
                        ProviderName = "Provider"
                    }
                }
            };

            _outerApiMock
                .Setup(x => x.GetMatches(userId))
                .ReturnsAsync(response);

            var request = new GetMatchesQuery { UserId = userId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Matches.Should().HaveCount(1);
            var match = result.Matches[0];
            match.Uln.Should().Be(response.Matches[0].Uln);
            match.CertificateType.Should().Be(CertificateType.Standard);
            match.UserIdentityId.Should().Be(response.Matches[0].UserIdentityId);
            match.CourseCode.Should().Be(response.Matches[0].CourseCode);
            match.CourseName.Should().Be(response.Matches[0].CourseName);
            match.CourseLevel.Should().Be(response.Matches[0].CourseLevel);
            match.DateAwarded.Should().Be(response.Matches[0].DateAwarded);
            match.ProviderName.Should().Be(response.Matches[0].ProviderName);
            match.Ukprn.Should().Be(response.Matches[0].Ukprn);

            result.Masks.Should().HaveCount(1);
            var mask = result.Masks[0];
            mask.CertificateType.Should().Be(CertificateType.Framework);
            mask.CourseCode.Should().Be(response.Masks[0].CourseCode);
            mask.CourseName.Should().Be(response.Masks[0].CourseName);

            _outerApiMock.Verify(x => x.GetMatches(userId), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetMatches(userId))
                .ReturnsAsync((MatchesResponse)null!);

            var request = new GetMatchesQuery { UserId = userId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _outerApiMock.Verify(x => x.GetMatches(userId), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetMatches(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("API failure"));

            var request = new GetMatchesQuery { UserId = userId };

            // Act
            Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
