using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators.CertificatesOrchestratorTests
{
    public class DownloadCertificateTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private Mock<ISessionService> _sessionMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IBlobService> _blobServiceMock;
        private Mock<IAsposeLicenseService> _asposeLicenseServiceMock;
        private DigitalCertificatesWebConfiguration _digitalCertificatesWebConfig;

        private CertificatesOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _sessionMock = new Mock<ISessionService>();
            _userServiceMock = new Mock<IUserService>();
            _blobServiceMock = new Mock<IBlobService>();
            _asposeLicenseServiceMock = new Mock<IAsposeLicenseService>();
            _digitalCertificatesWebConfig = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                OneLoginSettingsUrl = "http://settings.com",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "TestDb",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                FrameworkTemplateBlobName = "framework-template",
                MasterPassword = "master-password",
                StorageConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "test-container",
                AsposeLicenseContainerName = "aspose-license-container",
                LicenseBlobName = "license-blob"
            };

            _sut = new CertificatesOrchestrator(
                _mediatorMock.Object,
                _httpContextAccessorMock.Object,
                _sessionMock.Object,
                _userServiceMock.Object,
                _blobServiceMock.Object,
                _asposeLicenseServiceMock.Object,
                _digitalCertificatesWebConfig);
        }

        [Test]
        public async Task GetDownloadCertificateViewModelAsync_ReturnsNull_When_MediatorReturnsNull()
        {
            // Arrange
            var certificateId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetStandardCertificateQueryResult)null);

            // Act
            var result = await _sut.GetDownloadCertificateViewModelAsync(certificateId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetDownloadCertificateViewModelAsync_ThrowsException_When_RequiredFileds_AreNotPresent()
        {
            // Arrange
            var certificateId = Guid.NewGuid();            

            var queryResult = new GetStandardCertificateQueryResult
            {
                CertificateType = "Standard",
                FamilyName = "",
                GivenNames = "",
                CourseName = "",
                CourseOption = "Frontend",
                CourseLevel = null,
                OverallGrade = null,
                DateAwarded = null,
                CertificateReference = null
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act & Assert
            Func<Task> act = async () => await _sut.GetDownloadCertificateViewModelAsync(certificateId);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage($"Certificate {certificateId} is missing required data.");
        }

        [Test]
        public async Task GetDownloadCertificateViewModelAsync_ReturnsMappedViewModel_When_MediatorReturnsResult()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var dateAwarded = new DateTime(2025, 3, 24);

            var queryResult = new GetStandardCertificateQueryResult
            {
                CertificateType = "Standard",
                FamilyName = "Smith",
                GivenNames = "John Andrew",
                CourseName = "Software Developer",
                CourseOption = "Frontend",
                CourseLevel = 3,
                OverallGrade = "Distinction",
                DateAwarded = dateAwarded,
                CertificateReference = "CERT-12345",
                CoronationEmblem = true
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act
            var result = await _sut.GetDownloadCertificateViewModelAsync(certificateId);

            // Assert
            result.Should().NotBeNull();
            result.FamilyName.Should().Be(queryResult.FamilyName);
            result.GivenNames.Should().Be(queryResult.GivenNames);
            result.FullName.Should().Be($"{queryResult.GivenNames} \n {queryResult.FamilyName}");
            result.StandardName.Should().Be(queryResult.CourseName);
            result.OptionName.Should().Be(queryResult.CourseOption);
            result.Level.Should().Be(queryResult.CourseLevel.ToString());
            result.Result.Should().Be(queryResult.OverallGrade);
            result.DateAwarded.Should().Be(queryResult.DateAwarded);
            result.CertificateNumber.Should().Be(queryResult.CertificateReference);
            result.CoronationEmblem.Should().Be(queryResult.CoronationEmblem);
        }                               
    }
}
