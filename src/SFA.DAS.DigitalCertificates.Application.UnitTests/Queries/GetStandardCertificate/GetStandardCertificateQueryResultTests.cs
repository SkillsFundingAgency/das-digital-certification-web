using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.UnitTests.Queries.GetStandardCertificate
{
    public class GetStandardCertificateQueryResultTests
    {
        [Test]
        public void When_SourceIsNull_Then_ResultIsNull()
        {
            // Arrange
            GetStandardCertificateResponse? source = null;

            // Act
            GetStandardCertificateQueryResult? result = source;

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_SourceHasValues_Then_MapsAllFields()
        {
            // Arrange
            var requestedAt = new DateTime(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);
            var printStatusAt = new DateTime(2026, 10, 2, 10, 0, 0, DateTimeKind.Utc);

            var source = new GetStandardCertificateResponse
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
                OverallGrade = "Distinction",
                ProviderName = "Provider",
                Ukprn = "100000",
                EmployerName = "Employer",
                AssessorName = "Assessor",
                StartDate = DateTime.UtcNow.AddYears(-1),
                LatestEPAOutcome = "Pass",
                Status = "NotDelivered",
                PrintRequestedAt = requestedAt,
                PrintRequestedBy = "Requester",
                PrintStatus = "NotDelivered",
                PrintStatusAt = printStatusAt,
                CoronationEmblem = true
            };

            // Act
            GetStandardCertificateQueryResult? result = source;

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
            result.LatestEPAOutcome.Should().Be("Pass");
            result.Status.Should().Be("NotDelivered");
            result.PrintRequestedAt.Should().Be(requestedAt);
            result.PrintRequestedBy.Should().Be(source.PrintRequestedBy);
            result.PrintStatus.Should().Be("NotDelivered");
            result.PrintStatusAt.Should().Be(printStatusAt);
            result.CoronationEmblem.Should().BeTrue();
        }
    }
}