using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetSharedFrameworkCertificate
{
    [TestFixture]
    public class GetSharedFrameworkCertificateQueryResultTests
    {
        [Test]
        public void Implicit_Conversion_Maps_Properties_Correctly()
        {
            // Arrange
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
                DateAwarded = new DateTime(2023, 6, 1),
                ProviderName = "Provider",
                EmployerName = "Employer",
                StartDate = new DateTime(2020, 9, 1),
                QualificationsAndAwardingBodies = new List<QualificationDetailsResponse>
                {
                    new QualificationDetailsResponse { Name = "Q1", AwardingBody = "A1" },
                    new QualificationDetailsResponse { Name = "", AwardingBody = "OnlyBody" },
                    new QualificationDetailsResponse { Name = "OnlyName", AwardingBody = "" }
                }
            };

            // Act
            GetSharedFrameworkCertificateQueryResult? result = response;

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

            result.QualificationsAndAwardingBodies.Should().ContainInOrder(new[] { "Q1, A1", "OnlyBody", "OnlyName" });
        }

        [Test]
        public void Implicit_Conversion_Returns_Null_For_Null_Source()
        {
            // Arrange
            GetSharedFrameworkCertificateResponse? response = null;

            // Act
            GetSharedFrameworkCertificateQueryResult? result = response;

            // Assert
            result.Should().BeNull();
        }
    }
}
