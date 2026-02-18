using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharedStandardCertificate
{
    [TestFixture]
    public class GetSharedStandardCertificateQueryResultTests
    {
        [Test]
        public void Implicit_Conversion_Maps_Properties_Correctly()
        {
            // Arrange
            var response = new GetSharedStandardCertificateResponse
            {
                FamilyName = "Family",
                GivenNames = "Given",
                CertificateType = "Standard",
                CertificateReference = "REF123",
                CourseName = "Course",
                CourseOption = "Opt",
                CourseLevel = 2,
                DateAwarded = new DateTime(2023, 6, 1),
                OverallGrade = "Pass",
                ProviderName = "Provider",
                StartDate = new DateTime(2020, 9, 1)
            };

            // Act
            GetSharedStandardCertificateQueryResult? result = response;

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
        }

        [Test]
        public void Implicit_Conversion_Returns_Null_For_Null_Source()
        {
            // Arrange
            GetSharedStandardCertificateResponse? response = null;

            // Act
            GetSharedStandardCertificateQueryResult? result = response;

            // Assert
            result.Should().BeNull();
        }
    }
}
