using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators.CertificatesOrchestratorTests
{
    public class DownloadCertificateTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ISessionService> _sessionMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IBlobService> _blobServiceMock;
        private Mock<IAsposeLicenseService> _asposeLicenseServiceMock;
        private DigitalCertificatesWebConfiguration _digitalCertificatesWebConfig;
        private Mock<IDownloadCertificateService> _downloadCertificateServiceMock;

        private CertificatesOrchestrator _sut;
        private Mock<IValidator<SelectAddressViewModel>> _selectAddressValidatorMock;
        private Mock<IValidator<AddAddressManualViewModel>> _addAddressValidatorMock;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _sessionMock = new Mock<ISessionService>();
            _userServiceMock = new Mock<IUserService>();
            _blobServiceMock = new Mock<IBlobService>();
            _asposeLicenseServiceMock = new Mock<IAsposeLicenseService>();
            _downloadCertificateServiceMock = new Mock<IDownloadCertificateService>();
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

            _selectAddressValidatorMock = new Mock<IValidator<SelectAddressViewModel>>();
            _addAddressValidatorMock = new Mock<IValidator<AddAddressManualViewModel>>();

            _sut = new CertificatesOrchestrator(
                _mediatorMock.Object,
                _contextAccessorMock.Object,
                _sessionMock.Object,
                _userServiceMock.Object,
                _selectAddressValidatorMock.Object,
                _addAddressValidatorMock.Object,
                _blobServiceMock.Object,
                _asposeLicenseServiceMock.Object,
                _digitalCertificatesWebConfig,
                _downloadCertificateServiceMock.Object);
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
        public async Task GetDownloadCertificateViewModelAsync_ThrowsException_When_RequiredFields_AreNotPresent()
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

            _downloadCertificateServiceMock
                .Setup(s => s.CreateDownloadCertificateViewModel(
                    It.IsAny<DownloadCertificateRequestViewModel>()))
                .Throws(new InvalidOperationException(
                    $"Certificate {certificateId} is missing required data."));

            // Act
            Func<Task> act = async () =>
                await _sut.GetDownloadCertificateViewModelAsync(certificateId);

            // Assert
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage($"Certificate {certificateId} is missing required data.");

            _downloadCertificateServiceMock.Verify(s => s.CreateDownloadCertificateViewModel(
                It.Is<DownloadCertificateRequestViewModel>(m =>
                    m.CertificateId == certificateId
                    && m.CertificateType == CertificateType.Standard
                    && m.FamilyName == ""
                    && m.GivenNames == ""
                    && m.CourseName == ""
                    && m.CourseOption == "Frontend"
                    && m.CourseLevel == null
                    && m.OverallGrade == null
                    && m.DateAwarded == null
                    && m.CertificateNumber == null)),
                Times.Once);
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

            var expectedViewModel = new DownloadCertificateViewModel
            {
                FamilyName = "Smith",
                GivenNames = "John Andrew",
                CourseName = "Software Developer",
                CourseOption = "Frontend",
                CourseLevel = "3",
                OverallGrade = "Distinction",
                DateAwarded = dateAwarded,
                CertificateNumber = "CERT-12345",
                CoronationEmblem = true,
                CertificateType = CertificateType.Standard
            };

            _mediatorMock
                .Setup(m => m.Send(
                    It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            _downloadCertificateServiceMock
                .Setup(s => s.CreateDownloadCertificateViewModel(
                    It.Is<DownloadCertificateRequestViewModel>(m =>
                        m.CertificateId == certificateId
                        && m.CertificateType == CertificateType.Standard
                        && m.FamilyName == queryResult.FamilyName
                        && m.GivenNames == queryResult.GivenNames
                        && m.CourseName == queryResult.CourseName
                        && m.CourseOption == queryResult.CourseOption
                        && m.CourseLevel == queryResult.CourseLevel.ToString()
                        && m.OverallGrade == queryResult.OverallGrade
                        && m.DateAwarded == queryResult.DateAwarded
                        && m.CertificateNumber == queryResult.CertificateReference
                        && m.CoronationEmblem == queryResult.CoronationEmblem)))
                .Returns(expectedViewModel);

            // Act
            var result = await _sut.GetDownloadCertificateViewModelAsync(certificateId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeSameAs(expectedViewModel);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetStandardCertificateQuery>(q => q.CertificateId == certificateId),
                It.IsAny<CancellationToken>()), Times.Once);

            _downloadCertificateServiceMock.Verify(s => s.CreateDownloadCertificateViewModel(
                It.IsAny<DownloadCertificateRequestViewModel>()), Times.Once);
        }
    }
}
