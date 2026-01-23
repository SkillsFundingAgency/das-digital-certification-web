using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetFrameworkCertificate
{
    [TestFixture]
    public class GetFrameworkCertificateQueryResultTests
    {
        [Test]
        public void Implicit_Conversion_Maps_Fields_And_Formats_Qualifications()
        {
            // Arrange
            var source = new GetFrameworkCertificateResponse
            {
                FamilyName = "Family",
                GivenNames = "Given",
                Uln = 123456,
                CertificateType = "Framework",
                FrameworkCertificateNumber = "FW-1",
                CourseName = "Course",
                CourseOption = "Opt",
                CourseLevel = "1",
                DateAwarded = DateTime.UtcNow.Date,
                ProviderName = "Provider",
                EmployerName = "Employer",
                StartDate = DateTime.UtcNow.AddYears(-1),
                PrintRequestedAt = DateTime.UtcNow.AddDays(-2),
                PrintRequestedBy = "Requester",
                QualificationsAndAwardingBodies = new List<QualificationDetailsResponse>
                {
                    new QualificationDetailsResponse { Name = "N1", AwardingBody = "B1" },
                    new QualificationDetailsResponse { Name = "", AwardingBody = "OnlyBody" },
                    new QualificationDetailsResponse { Name = "OnlyName", AwardingBody = "" }
                },
                DeliveryInformation = new List<string> { "D1", "D2" }
            };

            // Act
            GetFrameworkCertificateQueryResult? result = source;

            // Assert
            result.Should().NotBeNull();
            result!.FamilyName.Should().Be(source.FamilyName);
            result.GivenNames.Should().Be(source.GivenNames);
            result.Uln.Should().Be(source.Uln);
            result.CertificateType.Should().Be(source.CertificateType);
            result.FrameworkCertificateNumber.Should().Be(source.FrameworkCertificateNumber);
            result.CourseName.Should().Be(source.CourseName);
            result.CourseOption.Should().Be(source.CourseOption);
            result.CourseLevel.Should().Be(source.CourseLevel);
            result.DateAwarded.Should().Be(source.DateAwarded);
            result.ProviderName.Should().Be(source.ProviderName);
            result.EmployerName.Should().Be(source.EmployerName);
            result.StartDate.Should().Be(source.StartDate);
            result.PrintRequestedAt.Should().Be(source.PrintRequestedAt);
            result.PrintRequestedBy.Should().Be(source.PrintRequestedBy);
            result.QualificationsAndAwardingBodies.Should().Contain("N1, B1");
            result.QualificationsAndAwardingBodies.Should().Contain("OnlyBody");
            result.QualificationsAndAwardingBodies.Should().Contain("OnlyName");
            result.DeliveryInformation.Should().BeEquivalentTo(source.DeliveryInformation);
        }
    }
}
