using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateById;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetCertificate
{
    public class GetCertificateByIdQueryResultTests
    {
        [Test]
        public void When_SourceIsNull_Then_ResultIsNull()
        {
            // Arrange
            GetCertificateByIdResponse? source = null;

            // Act
            GetCertificateByIdQueryResult? result = source;

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_SourceHasValues_Then_MapsAllFields()
        {
            // Arrange
            var source = new GetCertificateByIdResponse
            {
                FamilyName = "Family",
                GivenNames = "Given",
                Uln = "1234567890",
                CertificateType = "Standard",
                CertificateReference = "REF123",
                CourseCode = "C1",
                CourseName = "Course",
                CourseOption = "Opt",
                CourseLevel = "2",
                DateAwarded = System.DateTime.UtcNow.Date,
                OverallGrade = "Pass",
                ProviderName = "Provider",
                Ukprn = "100000",
                EmployerName = "Employer",
                AssessorName = "Assessor",
                StartDate = System.DateTime.UtcNow.AddYears(-1),
                PrintRequestedAt = System.DateTime.UtcNow.AddDays(-2),
                PrintRequestedBy = "Requester"
            };

            // Act
            GetCertificateByIdQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(source.FamilyName);
            result.GivenNames.Should().Be(source.GivenNames);
            result.Uln.Should().Be(source.Uln);
            result.CertificateType.Should().Be(source.CertificateType);
            result.CertificateReference.Should().Be(source.CertificateReference);
            result.CourseCode.Should().Be(source.CourseCode);
            result.CourseName.Should().Be(source.CourseName);
            result.CourseOption.Should().Be(source.CourseOption);
            result.CourseLevel.Should().Be(source.CourseLevel);
            result.DateAwarded.Should().Be(source.DateAwarded);
            result.OverallGrade.Should().Be(source.OverallGrade);
            result.ProviderName.Should().Be(source.ProviderName);
            result.Ukprn.Should().Be(source.Ukprn);
            result.EmployerName.Should().Be(source.EmployerName);
            result.AssessorName.Should().Be(source.AssessorName);
            result.StartDate.Should().Be(source.StartDate);
            result.PrintRequestedAt.Should().Be(source.PrintRequestedAt);
            result.PrintRequestedBy.Should().Be(source.PrintRequestedBy);
        }
    }
}
