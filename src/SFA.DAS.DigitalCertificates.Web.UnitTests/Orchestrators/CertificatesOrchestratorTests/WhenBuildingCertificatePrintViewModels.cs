using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Queries.GetFrameworkCertificate;
using SFA.DAS.DigitalCertificates.Application.Queries.GetStandardCertificate;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Enums;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators.CertificatesOrchestratorTests
{
    [TestFixture]
    public class WhenBuildingCertificatePrintViewModels
    {
        private static readonly DateTime RequestedAt =
            new(2026, 10, 1, 9, 0, 0, DateTimeKind.Utc);

        private static readonly DateTime BatchStatusAt =
            new(2026, 10, 2, 10, 0, 0, DateTimeKind.Utc);

        private const string RequestedMessage =
            "A certificate was requested on 01 October 2026. It can take up to 3 weeks to be delivered.";

        private const string DeliveredMessage =
            "A certificate was delivered on 02 October 2026.";

        private Guid _certificateId;
        private Mock<IMediator> _mediator;
        private DigitalCertificatesWebConfiguration _configuration;
        private CertificatesOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _certificateId = Guid.NewGuid();
            _mediator = new Mock<IMediator>();

            var sessionService = new Mock<ISessionService>();
            sessionService
                .Setup(x => x.GetOwnedCertificatesAsync())
                .ReturnsAsync(new List<Certificate>());

            _configuration = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.local",
                OneLoginBaseUrl = "http://onelogin/",
                OneLoginSettingsPath = "settings",
                RedisConnectionString = "UseDevelopmentStorage=true",
                DataProtectionKeysDatabase = "0",
                StandardTemplateBlobName = "standard-template",
                GreenStandardTemplateBlobName = "green-standard-template",
                FrameworkTemplateBlobName = "framework-template",
                MasterPassword = "master-password",
                StorageConnectionString = "UseDevelopmentStorage=true",
                ContainerName = "test-container",
                AsposeLicenseContainerName = "test-aspose-container",
                LicenseBlobName = "license-blob"
            };

            _sut = new CertificatesOrchestrator(
                _mediator.Object,
                new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                sessionService.Object,
                Mock.Of<IUserService>(),
                Mock.Of<IValidator<SelectAddressViewModel>>(),
                Mock.Of<IValidator<AddAddressManualViewModel>>(),
                Mock.Of<IBlobService>(),
                Mock.Of<IAsposeLicenseService>(),
                _configuration,
                Mock.Of<IDownloadCertificateService>(),
                Mock.Of<ILogger<CertificatesOrchestrator>>());
        }

        [TestCase("PrintRequested", null, true, PrintStatus.Requested, "Print requested")]
        [TestCase("Reprint", null, true, PrintStatus.Requested, "Print requested")]
        [TestCase("SentToPrinter", "SentToPrinter", true, PrintStatus.Requested, "Print requested")]
        [TestCase("Printed", "Printed", true, PrintStatus.Printed, "Printed")]
        [TestCase("NotDelivered", "NotDelivered", true, PrintStatus.Printed, "Printed")]
        [TestCase("Delivered", "Delivered", true, PrintStatus.Delivered, "Delivered")]
        [TestCase("Delivered", null, true, PrintStatus.None, null)]
        [TestCase("Delivered", "Delivered", false, PrintStatus.None, null)]
        public async Task Then_Standard_Shows_The_Status_Of_The_Current_Print_Request(
            string status,
            string? batchStatus,
            bool hasRequest,
            PrintStatus expectedStatus,
            string? expectedLabel)
        {
            // Arrange
            SetUpStandardCertificate(new GetStandardCertificateQueryResult
            {
                CertificateType = "Standard",
                Status = status,
                PrintRequestedAt = hasRequest ? RequestedAt : null,
                PrintStatus = batchStatus,
                PrintStatusAt = batchStatus is null ? null : BatchStatusAt
            });

            // Act
            var result = await _sut.GetCertificateStandardViewModel(_certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.ShowPrintHeader.Should().Be(hasRequest);
            result.ShowRequestPrint.Should().BeFalse();
            result.PrintStatusDisplay.Should().Be(expectedLabel);
            result.PrintStatusDate.Should().Be(ExpectedDisplayDate(expectedStatus));
            result.PrintStatusMessage.Should().Be(ExpectedMessage(expectedStatus));
        }

        [TestCase(null, null, false, PrintStatus.None, null)]
        [TestCase("Reprint", null, true, PrintStatus.Requested, "Print requested")]
        [TestCase("SentToPrinter", "SentToPrinter", true, PrintStatus.Requested, "Print requested")]
        [TestCase("Printed", "Printed", true, PrintStatus.Printed, "Printed")]
        [TestCase("NotDelivered", "NotDelivered", true, PrintStatus.Printed, "Printed")]
        [TestCase("Delivered", "Delivered", true, PrintStatus.Delivered, "Delivered")]
        [TestCase("Delivered", null, true, PrintStatus.None, null)]
        [TestCase("Delivered", "Delivered", false, PrintStatus.None, null)]
        [TestCase("PrintRequested", null, true, PrintStatus.None, null)]
        public async Task Then_Framework_Shows_The_Status_Of_The_Current_Print_Request(
            string? status,
            string? batchStatus,
            bool hasRequest,
            PrintStatus expectedStatus,
            string? expectedLabel)
        {
            // Arrange
            var source = new GetFrameworkCertificateQueryResult
            {
                CertificateType = "Framework",
                Status = status,
                PrintRequestedAt = hasRequest ? RequestedAt : null,
                PrintStatus = batchStatus,
                PrintStatusAt = batchStatus is null ? null : BatchStatusAt
            };

            _mediator
                .Setup(x => x.Send(
                    It.Is<GetFrameworkCertificateQuery>(q => q.CertificateId == _certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);

            // Act
            var result = await _sut.GetCertificateFrameworkViewModel(_certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.ShowPrintHeader.Should().Be(hasRequest);
            result.PrintStatusDisplay.Should().Be(expectedLabel);
            result.PrintStatusDate.Should().Be(ExpectedDisplayDate(expectedStatus));
            result.PrintStatusMessage.Should().Be(ExpectedMessage(expectedStatus));
        }

        [TestCase(true, "Submitted", "Pass", false, null, true)]
        [TestCase(true, "submitted", "pass", false, null, true)]
        [TestCase(false, "Submitted", "Pass", false, null, false)]
        [TestCase(true, "Submitted", "Fail", false, null, false)]
        [TestCase(true, "Submitted", null, false, null, false)]
        [TestCase(true, "Delivered", "Pass", false, null, false)]
        [TestCase(true, "Submitted", "Pass", true, null, false)]
        [TestCase(true, "Submitted", "Pass", false, "SentToPrinter", false)]
        public async Task Then_Standard_Offers_First_Print_Only_When_Enabled_And_Eligible(
            bool enabled,
            string status,
            string? outcome,
            bool hasRequest,
            string? batchStatus,
            bool expected)
        {
            // Arrange
            _configuration.ApprenticeFirstPrintEnabled = enabled;

            SetUpStandardCertificate(new GetStandardCertificateQueryResult
            {
                CertificateType = "Standard",
                Status = status,
                LatestEPAOutcome = outcome,
                PrintRequestedAt = hasRequest ? RequestedAt : null,
                PrintStatus = batchStatus
            });

            // Act
            var result = await _sut.GetCertificateStandardViewModel(_certificateId);

            // Assert
            result.Should().NotBeNull();
            result!.ShowRequestPrint.Should().Be(expected);
        }

        private void SetUpStandardCertificate(GetStandardCertificateQueryResult source)
        {
            _mediator
                .Setup(x => x.Send(
                    It.Is<GetStandardCertificateQuery>(q => q.CertificateId == _certificateId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(source);
        }

        private static DateTime? ExpectedDisplayDate(PrintStatus status)
        {
            return status switch
            {
                PrintStatus.Requested or PrintStatus.Printed => RequestedAt,
                PrintStatus.Delivered => BatchStatusAt,
                _ => null
            };
        }

        private static string? ExpectedMessage(PrintStatus status)
        {
            return status switch
            {
                PrintStatus.Requested or PrintStatus.Printed => RequestedMessage,
                PrintStatus.Delivered => DeliveredMessage,
                _ => null
            };
        }
    }
}