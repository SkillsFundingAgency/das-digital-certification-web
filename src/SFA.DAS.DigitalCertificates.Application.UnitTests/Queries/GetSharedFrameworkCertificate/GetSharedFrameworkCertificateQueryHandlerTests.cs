using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharedFrameworkCertificate
{
    [TestFixture]
    public class GetSharedFrameworkCertificateQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetSharedFrameworkCertificateQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetSharedFrameworkCertificateQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var id = Guid.NewGuid();

            var response = new GetSharedFrameworkCertificateResponse
            {
                FamilyName = "Family",
                GivenNames = "Given",
                CertificateType = "Framework",
                CertificateReference = "REF123",
                FrameworkCertificateNumber = "FW-1",
                CourseName = "Course",
                CourseOption = "Opt",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow.Date,
                ProviderName = "Provider",
                EmployerName = "Employer",
                StartDate = DateTime.UtcNow.AddYears(-1),
                QualificationsAndAwardingBodies = new List<QualificationDetailsResponse>
                {
                    new QualificationDetailsResponse { Name = "Q1", AwardingBody = "A1" }
                }
            };

            _outerApiMock
                .Setup(x => x.GetSharedFrameworkCertificate(id))
                .ReturnsAsync(response);

            var request = new GetSharedFrameworkCertificateQuery { Id = id };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(response.FamilyName);
            result.GivenNames.Should().Be(response.GivenNames);
            result.CertificateType.Should().Be(response.CertificateType);
            result.CertificateReference.Should().Be(response.CertificateReference);
            result.FrameworkCertificateNumber.Should().Be(response.FrameworkCertificateNumber);
            result.CourseName.Should().Be(response.CourseName);
            result.CourseOption.Should().Be(response.CourseOption);
            result.CourseLevel.Should().Be(response.CourseLevel);
            result.DateAwarded.Should().Be(response.DateAwarded);
            result.ProviderName.Should().Be(response.ProviderName);
            result.EmployerName.Should().Be(response.EmployerName);
            result.StartDate.Should().Be(response.StartDate);
            result.QualificationsAndAwardingBodies.Should().Contain("Q1, A1");

            _outerApiMock.Verify(x => x.GetSharedFrameworkCertificate(id), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var id = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetSharedFrameworkCertificate(id))
                .Returns(() => Task.FromResult<GetSharedFrameworkCertificateResponse>(null!));

            var request = new GetSharedFrameworkCertificateQuery { Id = id };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _outerApiMock.Verify(x => x.GetSharedFrameworkCertificate(id), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var id = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetSharedFrameworkCertificate(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("API failure"));

            var request = new GetSharedFrameworkCertificateQuery { Id = id };

            // Act
            Func<System.Threading.Tasks.Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
