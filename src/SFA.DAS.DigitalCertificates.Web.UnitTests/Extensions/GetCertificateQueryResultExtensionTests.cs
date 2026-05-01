using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharedStandardCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using FluentAssertions;
using System;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Extensions
{
    public class GetCertificateQueryResultExtensionTests
    {
        [Test]
        public void ToDownloadCertificateRequest_WhenStandardCertificateResult_ShouldMapProperties()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var dateAwarded = new DateTime(2024, 5, 10);

            var result = new GetStandardCertificateQueryResult
            {
                FamilyName = "Smith",
                GivenNames = "John",
                CertificateType = "Standard",
                CertificateReference = "STD-123",
                CourseName = "Software Developer",
                CourseOption = "C#",
                CourseLevel = 4,
                DateAwarded = dateAwarded,
                OverallGrade = "Distinction",
                CoronationEmblem = true
            };

            // Act
            var actual = result.ToDownloadCertificateRequest(certificateId);

            // Assert
            actual.CertificateId.Should().Be(certificateId);
            actual.CertificateType.Should().Be(CertificateType.Standard);
            actual.FamilyName.Should().Be("Smith");
            actual.GivenNames.Should().Be("John");
            actual.CourseName.Should().Be("Software Developer");
            actual.CourseOption.Should().Be("C#");
            actual.CourseLevel.Should().Be("4");
            actual.DateAwarded.Should().Be(dateAwarded);
            actual.OverallGrade.Should().Be("Distinction");
            actual.CertificateNumber.Should().Be("STD-123");
            actual.CoronationEmblem.Should().BeTrue();
        }

        [Test]
        public void ToDownloadCertificateRequest_WhenSharedStandardCertificateResult_ShouldMapProperties()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var dateAwarded = new DateTime(2024, 6, 15);

            var result = new GetSharedStandardCertificateQueryResult
            {
                FamilyName = "Jones",
                GivenNames = "Sarah",
                CertificateType = "Standard",
                CertificateReference = "SH-STD-456",
                CourseName = "Business Administrator",
                CourseOption = "Admin",
                CourseLevel = 3,
                DateAwarded = dateAwarded,
                OverallGrade = "Pass",
                CoronationEmblem = false
            };

            // Act
            var actual = result.ToDownloadCertificateRequest(certificateId);

            // Assert
            actual.CertificateId.Should().Be(certificateId);
            actual.CertificateType.Should().Be(CertificateType.Standard);
            actual.FamilyName.Should().Be("Jones");
            actual.GivenNames.Should().Be("Sarah");
            actual.CourseName.Should().Be("Business Administrator");
            actual.CourseOption.Should().Be("Admin");
            actual.CourseLevel.Should().Be("3");
            actual.DateAwarded.Should().Be(dateAwarded);
            actual.OverallGrade.Should().Be("Pass");
            actual.CertificateNumber.Should().Be("SH-STD-456");
            actual.CoronationEmblem.Should().BeFalse();
        }

        [Test]
        public void ToDownloadCertificateRequest_WhenFrameworkCertificateResult_ShouldMapProperties()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var dateAwarded = new DateTime(2024, 7, 20);

            var result = new GetFrameworkCertificateQueryResult
            {
                FamilyName = "Brown",
                GivenNames = "David",
                CertificateType = "Framework",
                FrameworkCertificateNumber = "FW-789",
                CourseName = "Engineering",
                CourseOption = "Mechanical",
                CourseLevel = "Level 3",
                DateAwarded = dateAwarded
            };

            // Act
            var actual = result.ToDownloadCertificateRequest(certificateId);

            // Assert
            actual.CertificateId.Should().Be(certificateId);
            actual.CertificateType.Should().Be(CertificateType.Framework);
            actual.FamilyName.Should().Be("Brown");
            actual.GivenNames.Should().Be("David");
            actual.CourseName.Should().Be("Engineering");
            actual.CourseOption.Should().Be("Mechanical");
            actual.CourseLevel.Should().Be("Level 3");
            actual.DateAwarded.Should().Be(dateAwarded);
            actual.CertificateNumber.Should().Be("FW-789");
            actual.OverallGrade.Should().BeNull();
            actual.CoronationEmblem.Should().BeFalse();
        }

        [Test]
        public void ToDownloadCertificateRequest_WhenSharedFrameworkCertificateResult_ShouldMapProperties()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var dateAwarded = new DateTime(2024, 8, 25);

            var result = new GetSharedFrameworkCertificateQueryResult
            {
                FamilyName = "Taylor",
                GivenNames = "Emma",
                CertificateType = "Framework",
                FrameworkCertificateNumber = "SH-FW-101",
                CourseName = "Construction",
                CourseOption = "Site Management",
                CourseLevel = "Level 2",
                DateAwarded = dateAwarded
            };

            // Act
            var actual = result.ToDownloadCertificateRequest(certificateId);

            // Assert
            actual.CertificateId.Should().Be(certificateId);
            actual.CertificateType.Should().Be(CertificateType.Framework);
            actual.FamilyName.Should().Be("Taylor");
            actual.GivenNames.Should().Be("Emma");
            actual.CourseName.Should().Be("Construction");
            actual.CourseOption.Should().Be("Site Management");
            actual.CourseLevel.Should().Be("Level 2");
            actual.DateAwarded.Should().Be(dateAwarded);
            actual.CertificateNumber.Should().Be("SH-FW-101");
            actual.OverallGrade.Should().BeNull();
            actual.CoronationEmblem.Should().BeFalse();
        }

        [Test]
        public void ToDownloadCertificateRequest_WhenStandardCourseLevelIsNull_ShouldMapCourseLevelAsNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var result = new GetStandardCertificateQueryResult
            {
                CertificateType = "Standard",
                CourseLevel = null
            };

            // Act
            var actual = result.ToDownloadCertificateRequest(certificateId);

            // Assert
            actual.CourseLevel.Should().BeNull();
        }

        [Test]
        public void ToDownloadCertificateRequest_WhenSharedStandardCourseLevelIsNull_ShouldMapCourseLevelAsNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            var result = new GetSharedStandardCertificateQueryResult
            {
                CertificateType = "Standard",
                CourseLevel = null
            };

            // Act
            var actual = result.ToDownloadCertificateRequest(certificateId);

            // Assert
            actual.CourseLevel.Should().BeNull();
        }
    }

}
