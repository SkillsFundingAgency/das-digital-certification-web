using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetStandardCertificate
{
    [TestFixture]
    public class GetStandardCertificateQueryHandlerTests
    {
        private Mock<IDigitalCertificatesOuterApi> _outerApiMock;
        private GetStandardCertificateQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _outerApiMock = new Mock<IDigitalCertificatesOuterApi>();
            _sut = new GetStandardCertificateQueryHandler(_outerApiMock.Object);
        }

        [Test]
        public async Task Handle_Calls_OuterApi_With_Correct_Parameters_And_Returns_Result()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var response = new GetStandardCertificateResponse
            {
                FamilyName = "Family",
                GivenNames = "Given",
                Uln = 1234567890,
                CertificateType = "Standard",
                CertificateReference = "REF123",
                CourseCode = "C1",
                CourseName = "Course",
                CourseOption = "Opt",
                CourseLevel = 2,
                DateAwarded = DateTime.UtcNow.Date,
                OverallGrade = "Pass",
                ProviderName = "Provider",
                Ukprn = "100000",
                EmployerName = "Employer",
                AssessorName = "Assessor",
                StartDate = DateTime.UtcNow.AddYears(-1),
                PrintRequestedAt = DateTime.UtcNow.AddDays(-2),
                PrintRequestedBy = "Requester"
            };

            _outerApiMock
                .Setup(x => x.GetStandardCertificate(certificateId))
                .ReturnsAsync(response);

            var request = new GetStandardCertificateQuery { CertificateId = certificateId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(response.FamilyName);
            result.GivenNames.Should().Be(response.GivenNames);
            result.Uln.Should().Be(response.Uln);
            result.CertificateType.Should().Be(response.CertificateType);
            result.CertificateReference.Should().Be(response.CertificateReference);
            result.CourseCode.Should().Be(response.CourseCode);
            result.CourseName.Should().Be(response.CourseName);
            result.CourseOption.Should().Be(response.CourseOption);
            result.CourseLevel.Should().Be(response.CourseLevel);
            result.DateAwarded.Should().Be(response.DateAwarded);
            result.OverallGrade.Should().Be(response.OverallGrade);
            result.ProviderName.Should().Be(response.ProviderName);
            result.Ukprn.Should().Be(response.Ukprn);
            result.EmployerName.Should().Be(response.EmployerName);
            result.AssessorName.Should().Be(response.AssessorName);
            result.StartDate.Should().Be(response.StartDate);
            result.PrintRequestedAt.Should().Be(response.PrintRequestedAt);
            result.PrintRequestedBy.Should().Be(response.PrintRequestedBy);

            _outerApiMock.Verify(x => x.GetStandardCertificate(certificateId), Times.Once);
        }

        [Test]
        public async Task Handle_Returns_Null_When_OuterApi_Returns_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            _outerApiMock
                .Setup(x => x.GetStandardCertificate(certificateId))
                .Returns(() => Task.FromResult<GetStandardCertificateResponse>(null!));

            var request = new GetStandardCertificateQuery { CertificateId = certificateId };

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.Should().BeNull();
            _outerApiMock.Verify(x => x.GetStandardCertificate(certificateId), Times.Once);
        }

        [Test]
        public void Handle_Throws_If_OuterApi_Fails()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _outerApiMock
                .Setup(x => x.GetStandardCertificate(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception("API failure"));

            var request = new GetStandardCertificateQuery { CertificateId = certificateId };

            // Act
            Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<Exception>().WithMessage("API failure");
        }
    }
}
