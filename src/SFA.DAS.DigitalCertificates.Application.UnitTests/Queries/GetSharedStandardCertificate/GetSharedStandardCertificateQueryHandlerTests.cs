using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharedStandardCertificate
{
    [TestFixture]
    public class GetSharedStandardCertificateQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetSharedStandardCertificateQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetSharedStandardCertificateQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var id = Guid.NewGuid();

            var response = new GetSharedStandardCertificateResponse
            {
                FamilyName = "Family",
                GivenNames = "Given",
                CertificateType = "Standard",
                CertificateReference = "REF123",
                CourseName = "Course",
                CourseOption = "Opt",
                CourseLevel = 3,
                DateAwarded = DateTime.UtcNow.Date,
                OverallGrade = "Pass",
                ProviderName = "Provider",
                StartDate = DateTime.UtcNow.AddYears(-1)
            };

            _outerApiMock
                .Setup(x => x.GetSharedStandardCertificate(id))
                .ReturnsAsync(response);

            var request = new GetSharedStandardCertificateQuery { Id = id };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(response.FamilyName);
            result.GivenNames.Should().Be(response.GivenNames);
            result.CertificateType.Should().Be(response.CertificateType);
            result.CertificateReference.Should().Be(response.CertificateReference);
            result.CourseName.Should().Be(response.CourseName);
            result.CourseOption.Should().Be(response.CourseOption);
            result.CourseLevel.Should().Be(response.CourseLevel);
            result.DateAwarded.Should().Be(response.DateAwarded);
            result.OverallGrade.Should().Be(response.OverallGrade);
            result.ProviderName.Should().Be(response.ProviderName);
            result.StartDate.Should().Be(response.StartDate);

            _outerApiMock.Verify(x => x.GetSharedStandardCertificate(id), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var id = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetSharedStandardCertificate(id))
                .ReturnsAsync((GetSharedStandardCertificateResponse)null!);

            var request = new GetSharedStandardCertificateQuery { Id = id };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _outerApiMock.Verify(x => x.GetSharedStandardCertificate(id), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var id = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetSharedStandardCertificate(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("API failure"));

            var request = new GetSharedStandardCertificateQuery { Id = id };

            // Act
            Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
