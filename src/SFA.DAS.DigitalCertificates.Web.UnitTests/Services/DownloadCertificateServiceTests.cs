using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Services
{
    public class DownloadCertificateServiceTests
    {
        
            private DownloadCertificateService _sut = null!;

            [SetUp]
            public void SetUp()
            {
                _sut = new DownloadCertificateService();
            }

            [Test]
            public void CreateDownloadCertificateViewModel_WhenStandardRequestIsValid_ReturnsMappedViewModel()
            {
                // Arrange
                var certificateId = Guid.NewGuid();
                var dateAwarded = new DateTime(2025, 3, 24);

                var request = new DownloadCertificateRequestViewModel
                {
                    CertificateId = certificateId,
                    CertificateType = CertificateType.Standard,
                    FamilyName = "Smith",
                    GivenNames = "John Andrew",
                    CourseName = "Software Developer",
                    CourseOption = "Frontend",
                    CourseLevel = "3",
                    OverallGrade = "Distinction",
                    DateAwarded = dateAwarded,
                    CertificateNumber = "CERT-12345",
                    CoronationEmblem = true
                };

                // Act
                var result = _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                result.Should().NotBeNull();
                result.FamilyName.Should().Be(request.FamilyName);
                result.GivenNames.Should().Be(request.GivenNames);               
                result.CourseName.Should().Be(request.CourseName);
                result.CourseOption.Should().Be(request.CourseOption);
                result.CourseLevel.Should().Be(request.CourseLevel);
                result.OverallGrade.Should().Be(request.OverallGrade);
                result.DateAwarded.Should().Be(dateAwarded);
                result.CertificateNumber.Should().Be(request.CertificateNumber);
                result.CoronationEmblem.Should().BeTrue();
                result.CertificateType.Should().Be(CertificateType.Standard);
            }

            [Test]
            public void CreateDownloadCertificateViewModel_WhenFrameworkRequestIsValid_ReturnsMappedViewModel()
            {
                // Arrange
                var certificateId = Guid.NewGuid();
                var dateAwarded = new DateTime(2025, 4, 10);

                var request = new DownloadCertificateRequestViewModel
                {
                    CertificateId = certificateId,
                    CertificateType = CertificateType.Framework,
                    FamilyName = "Brown",
                    GivenNames = "Sarah",
                    CourseName = "Engineering",
                    CourseOption = "Mechanical",
                    CourseLevel = "Level 3",
                    OverallGrade = null,
                    DateAwarded = dateAwarded,
                    CertificateNumber = "FW-98765",
                    CoronationEmblem = false
                };

                // Act
                var result = _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                result.Should().NotBeNull();
                result.FamilyName.Should().Be(request.FamilyName);
                result.GivenNames.Should().Be(request.GivenNames);                
                result.CourseName.Should().Be(request.CourseName);
                result.CourseOption.Should().Be(request.CourseOption);
                result.CourseLevel.Should().Be(request.CourseLevel);
                result.OverallGrade.Should().BeNull();
                result.DateAwarded.Should().Be(dateAwarded);
                result.CertificateNumber.Should().Be(request.CertificateNumber);
                result.CoronationEmblem.Should().BeFalse();
                result.CertificateType.Should().Be(CertificateType.Framework);
            }

            [Test]
            [TestCase(null)]
            [TestCase("")]
            [TestCase("   ")]
            public void CreateDownloadCertificateViewModel_WhenFamilyNameIsMissing_ThrowsInvalidOperationException(string familyName)
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.FamilyName = familyName;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            [TestCase(null)]
            [TestCase("")]
            [TestCase("   ")]
            public void CreateDownloadCertificateViewModel_WhenGivenNamesIsMissing_ThrowsInvalidOperationException(string givenNames)
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.GivenNames = givenNames;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            [TestCase(null)]
            [TestCase("")]
            [TestCase("   ")]
            public void CreateDownloadCertificateViewModel_WhenCourseNameIsMissing_ThrowsInvalidOperationException(string courseName)
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.CourseName = courseName;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            [TestCase(null)]
            [TestCase("")]
            [TestCase("   ")]
            public void CreateDownloadCertificateViewModel_WhenCourseLevelIsMissing_ThrowsInvalidOperationException(string courseLevel)
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.CourseLevel = courseLevel;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            public void CreateDownloadCertificateViewModel_WhenDateAwardedIsMissing_ThrowsInvalidOperationException()
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.DateAwarded = null;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            [TestCase(null)]
            [TestCase("")]
            [TestCase("   ")]
            public void CreateDownloadCertificateViewModel_WhenStandardOverallGradeIsMissing_ThrowsInvalidOperationException(string overallGrade)
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.OverallGrade = overallGrade;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            [TestCase(null)]
            [TestCase("")]
            [TestCase("   ")]
            public void CreateDownloadCertificateViewModel_WhenStandardCertificateNumberIsMissing_ThrowsInvalidOperationException(string certificateNumber)
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.CertificateNumber = certificateNumber;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            [TestCase(null)]
            [TestCase("")]
            [TestCase("   ")]
            public void CreateDownloadCertificateViewModel_WhenFrameworkCertificateNumberIsMissing_ThrowsInvalidOperationException(string certificateNumber)
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidFrameworkRequest(certificateId);
                request.CertificateNumber = certificateNumber;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} is missing required data.");
            }

            [Test]
            public void CreateDownloadCertificateViewModel_WhenCertificateTypeIsUnknown_ThrowsInvalidOperationException()
            {
                // Arrange
                var certificateId = Guid.NewGuid();

                var request = CreateValidStandardRequest(certificateId);
                request.CertificateType = CertificateType.Unknown;

                // Act
                Action act = () => _sut.CreateDownloadCertificateViewModel(request);

                // Assert
                act.Should()
                    .Throw<InvalidOperationException>()
                    .WithMessage($"Certificate {certificateId} has unsupported certificate type.");
            }

            private static DownloadCertificateRequestViewModel CreateValidStandardRequest(Guid certificateId)
            {
                return new DownloadCertificateRequestViewModel
                {
                    CertificateId = certificateId,
                    CertificateType = CertificateType.Standard,
                    FamilyName = "Smith",
                    GivenNames = "John Andrew",
                    CourseName = "Software Developer",
                    CourseOption = "Frontend",
                    CourseLevel = "3",
                    OverallGrade = "Distinction",
                    DateAwarded = new DateTime(2025, 3, 24),
                    CertificateNumber = "CERT-12345",
                    CoronationEmblem = true
                };
            }

            private static DownloadCertificateRequestViewModel CreateValidFrameworkRequest(Guid certificateId)
            {
                return new DownloadCertificateRequestViewModel
                {
                    CertificateId = certificateId,
                    CertificateType = CertificateType.Framework,
                    FamilyName = "Brown",
                    GivenNames = "Sarah",
                    CourseName = "Engineering",
                    CourseOption = "Mechanical",
                    CourseLevel = "Level 3",
                    OverallGrade = null,
                    DateAwarded = new DateTime(2025, 4, 10),
                    CertificateNumber = "FW-98765",
                    CoronationEmblem = false
                };
            }
        
    }
}
