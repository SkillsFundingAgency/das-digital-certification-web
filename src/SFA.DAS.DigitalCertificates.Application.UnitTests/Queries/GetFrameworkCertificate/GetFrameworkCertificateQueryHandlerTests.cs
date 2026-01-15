using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetFrameworkCertificate
{
    [TestFixture]
    public class GetFrameworkCertificateQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetFrameworkCertificateQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetFrameworkCertificateQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var response = new GetFrameworkCertificateResponse
            {
                FamilyName = "Family",
                GivenNames = "Given",
                Uln = 1234567890,
                CertificateType = "Framework",
                CertificateReference = "REF123",
                FrameworkCertificateNumber = "FW-1",
                CourseName = "Course",
                CourseOption = "Opt",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow.Date,
                ProviderName = "Provider",
                Ukprn = 100000,
                EmployerName = "Employer",
                StartDate = DateTime.UtcNow.AddYears(-1),
                PrintRequestedAt = DateTime.UtcNow.AddDays(-2),
                PrintRequestedBy = "Requester",
                QualificationsAndAwardingBodies = new List<QualificationDetailsResponse>
                {
                    new QualificationDetailsResponse { Name = "Q1", AwardingBody = "A1" }
                },
                DeliveryInformation = new List<string> { "Del1" }
            };

            _outerApiMock
                .Setup(x => x.GetFrameworkCertificate(certificateId))
                .ReturnsAsync(response);

            var request = new GetFrameworkCertificateQuery { CertificateId = certificateId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(response.FamilyName);
            result.GivenNames.Should().Be(response.GivenNames);
            result.Uln.Should().Be(response.Uln);
            result.CertificateType.Should().Be(response.CertificateType);
            result.FrameworkCertificateNumber.Should().Be(response.FrameworkCertificateNumber);
            result.CourseName.Should().Be(response.CourseName);
            result.CourseOption.Should().Be(response.CourseOption);
            result.CourseLevel.Should().Be(response.CourseLevel);
            result.DateAwarded.Should().Be(response.DateAwarded);
            result.ProviderName.Should().Be(response.ProviderName);
            result.Ukprn.Should().Be(response.Ukprn);
            result.EmployerName.Should().Be(response.EmployerName);
            result.StartDate.Should().Be(response.StartDate);
            result.PrintRequestedAt.Should().Be(response.PrintRequestedAt);
            result.PrintRequestedBy.Should().Be(response.PrintRequestedBy);
            result.QualificationsAndAwardingBodies.Should().Contain("Q1, A1");
            result.DeliveryInformation.Should().Contain("Del1");

            _outerApiMock.Verify(x => x.GetFrameworkCertificate(certificateId), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetFrameworkCertificate(certificateId))
                .Returns(() => Task.FromResult<GetFrameworkCertificateResponse>(null!));

            var request = new GetFrameworkCertificateQuery { CertificateId = certificateId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _outerApiMock.Verify(x => x.GetFrameworkCertificate(certificateId), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetFrameworkCertificate(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("API failure"));

            var request = new GetFrameworkCertificateQuery { CertificateId = certificateId };

            // Act
            Func<System.Threading.Tasks.Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
