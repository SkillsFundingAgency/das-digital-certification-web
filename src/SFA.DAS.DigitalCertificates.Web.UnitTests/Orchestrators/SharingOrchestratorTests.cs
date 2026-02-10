using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateSharing;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharings;
using SFA.DAS.DigitalCertificates.Application.Queries.GetSharingById;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using SFA.DAS.DigitalCertificates.Domain.Extensions;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class SharingOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ISessionStorageService> _sessionStorageServiceMock;
        private Mock<IDateTimeHelper> _dateTimeHelperMock;
        private DigitalCertificatesWebConfiguration _digitalCertificatesWebConfiguration;
        private SharingOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _userServiceMock = new Mock<IUserService>();
            _sessionStorageServiceMock = new Mock<ISessionStorageService>();
            _dateTimeHelperMock = new Mock<IDateTimeHelper>();
            _dateTimeHelperMock.SetupGet(d => d.Now).Returns(DateTime.UtcNow);

            _digitalCertificatesWebConfiguration = new DigitalCertificatesWebConfiguration
            {
                ServiceBaseUrl = "https://test.com",
                RedisConnectionString = "test",
                DataProtectionKeysDatabase = "test",
                SharingListLimit = 10
            };
            _sut = new SharingOrchestrator(_mediatorMock.Object, _userServiceMock.Object, _sessionStorageServiceMock.Object, _digitalCertificatesWebConfiguration, _dateTimeHelperMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [Test]
        public async Task GetSharings_Sends_Query_With_Correct_Values_And_Returns_ViewModel()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            var expiryTime = DateTime.UtcNow.AddDays(30);
            var courseName = "Software Development";
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = courseName,
                CourseLevel = "Level 3",
                DateAwarded = DateTime.UtcNow
            };
            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            var queryResult = new GetSharingsQueryResult
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = courseName,
                Sharings = new List<Sharing>
                {
                    new Sharing
                    {
                        SharingId = sharingId,
                        SharingNumber = 123456,
                        CreatedAt = createdAt,
                        LinkCode = Guid.NewGuid(),
                        ExpiryTime = expiryTime
                    }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSharingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act
            var result = await _sut.GetSharings(certificateId);

            // Assert
            result.Should().NotBeNull();
            result.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be(courseName);
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.Sharings.Should().HaveCount(1);
            result.Sharings[0].SharingId.Should().Be(sharingId);
            result.Sharings[0].SharingNumber.Should().Be(123456);
            result.Sharings[0].CreatedAt.Should().Be(createdAt);
            result.Sharings[0].ExpiryTime.Should().Be(expiryTime);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetSharingsQuery>(q =>
                    q.UserId == userId &&
                    q.CertificateId == certificateId &&
                    q.Limit == 10),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetSharings_When_Query_Returns_Null_Returns_Empty_ViewModel()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Test Course",
                CourseLevel = "Level 3",
                DateAwarded = DateTime.UtcNow
            };
            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSharingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetSharingsQueryResult)null);

            // Act
            var result = await _sut.GetSharings(certificateId);

            // Assert
            result.Should().NotBeNull();
            result.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be("Test Course");
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.Sharings.Should().BeEmpty();
        }
        [Test]
        public async Task GetSharings_When_Multiple_Sharings_Exist_Returns_All_Sharings()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var sharingId1 = Guid.NewGuid();
            var sharingId2 = Guid.NewGuid();
            var createdAt1 = DateTime.UtcNow.AddDays(-5);
            var createdAt2 = DateTime.UtcNow.AddDays(-2);
            var expiryTime1 = DateTime.UtcNow.AddDays(25);
            var expiryTime2 = DateTime.UtcNow.AddDays(28);
            var courseName = "Data Science";
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = courseName,
                CourseLevel = "Level 4",
                DateAwarded = DateTime.UtcNow
            };
            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            var queryResult = new GetSharingsQueryResult
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = courseName,
                Sharings = new List<Sharing>
                {
                    new Sharing
                    {
                        SharingId = sharingId1,
                        SharingNumber = 111111,
                        CreatedAt = createdAt1,
                        LinkCode = Guid.NewGuid(),
                        ExpiryTime = expiryTime1
                    },
                    new Sharing
                    {
                        SharingId = sharingId2,
                        SharingNumber = 222222,
                        CreatedAt = createdAt2,
                        LinkCode = Guid.NewGuid(),
                        ExpiryTime = expiryTime2
                    }
                }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSharingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act
            var result = await _sut.GetSharings(certificateId);

            // Assert
            result.Should().NotBeNull();
            result.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be(courseName);
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.Sharings.Should().HaveCount(2);
            result.Sharings[0].SharingId.Should().Be(sharingId1);
            result.Sharings[0].SharingNumber.Should().Be(111111);
            result.Sharings[1].SharingId.Should().Be(sharingId2);
            result.Sharings[1].SharingNumber.Should().Be(222222);
        }
        [Test]
        public void GetSharings_When_Certificate_Not_Found_In_Session_Throws_InvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate>());

            // Act + Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetSharings(certificateId));

            exception.Message.Should().Be($"Certificate {certificateId} not found for authenticated user");
        }

        [Test]
        public async Task GetSharings_Uses_Configured_Limit_Value()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var govUkIdentifier = "test-gov-uk-id";
            var customLimit = 25;

            _digitalCertificatesWebConfiguration.SharingListLimit = customLimit;

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Test Course",
                CourseLevel = "Level 3",
                DateAwarded = DateTime.UtcNow
            };
            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            var queryResult = new GetSharingsQueryResult
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Test Course",
                Sharings = new List<Sharing>()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSharingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act
            await _sut.GetSharings(certificateId);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetSharingsQuery>(q =>
                    q.UserId == userId &&
                    q.CertificateId == certificateId &&
                    q.Limit == customLimit),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetSharings_Uses_Null_Limit_When_Config_Is_Zero()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var govUkIdentifier = "test-gov-uk-id";

            _digitalCertificatesWebConfiguration.SharingListLimit = 0;

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Test Course",
                CourseLevel = "Level 3",
                DateAwarded = DateTime.UtcNow
            };
            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            var queryResult = new GetSharingsQueryResult
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Test Course",
                Sharings = new List<Sharing>()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSharingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act
            await _sut.GetSharings(certificateId);

            // Assert
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetSharingsQuery>(q =>
                    q.UserId == userId &&
                    q.CertificateId == certificateId &&
                    q.Limit == null),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CreateSharing_Sends_Command_With_Correct_Values_And_Returns_Success()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var courseName = "Software Development";
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = courseName,
                CourseLevel = "Level 3",
                DateAwarded = DateTime.UtcNow
            };
            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            var commandResult = new CreateSharingCommandResult
            {
                Userid = userId,
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = courseName,
                SharingId = sharingId,
                SharingNumber = 123456,
                CreatedAt = DateTime.UtcNow,
                LinkCode = Guid.NewGuid(),
                ExpiryTime = DateTime.UtcNow.AddDays(30)
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateSharingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commandResult);

            // Act
            var result = await _sut.CreateSharing(certificateId);

            // Assert
            result.Should().Be(sharingId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateSharingCommand>(c =>
                    c.UserId == userId &&
                    c.CertificateId == certificateId &&
                    c.CertificateType == CertificateType.Standard &&
                    c.CourseName == courseName),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CreateSharing_When_Command_Returns_Null_Returns_Failed_Result()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var courseName = "Software Development";
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = courseName,
                CourseLevel = "Level 3",
                DateAwarded = DateTime.UtcNow
            };
            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateSharingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateSharingCommandResult)null);

            // Act
            var result = await _sut.CreateSharing(certificateId);

            // Assert
            result.Should().Be(Guid.Empty);
        }

        [Test]
        public void CreateSharing_When_Certificate_Not_Found_In_Session_Throws_InvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate>());

            // Act + Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateSharing(certificateId));

            exception.Message.Should().Be($"Certificate {certificateId} not found for authenticated user");
        }

        [Test]
        public async Task GetSharingById_Returns_ViewModel_With_Correct_Values()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var govUkIdentifier = "gov-123";

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Course Name",
                CourseLevel = "Level1",
                DateAwarded = DateTime.UtcNow
            };

            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            var createdAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var expiryTime = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc);
            var linkCode = Guid.NewGuid();

            var response = new GetSharingByIdQueryResult
            {
                CertificateId = certificateId,
                CourseName = "Course Name",
                CertificateType = CertificateType.Standard,
                SharingId = sharingId,
                SharingNumber = 999999,
                CreatedAt = createdAt,
                ExpiryTime = expiryTime,
                LinkCode = linkCode,
                SharingAccess = new List<DateTime> { createdAt },
                SharingEmails = new List<SharingEmail>()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSharingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            _dateTimeHelperMock.SetupGet(d => d.Now).Returns(expiryTime.AddDays(-1));

            // Act
            var result = await _sut.GetSharingById(certificateId, sharingId);

            // Assert
            result.Should().NotBeNull();
            result.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be("Course Name");
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.SharingId.Should().Be(sharingId);
            result.SharingNumber.Should().Be(999999);
            result.CreatedAt.Should().Be(createdAt);
            result.ExpiryTime.Should().Be(expiryTime);
            result.LinkCode.Should().Be(linkCode);
            result.FormattedExpiry.Should().Be(expiryTime.ToUkExpiryDateTimeString());
            result.FormattedCreated.Should().Be(createdAt.ToUkDateTimeString());
            result.FormattedAccessTimes.Should().HaveCount(1);
            result.SecureLink.Should().Be($"{_digitalCertificatesWebConfiguration.ServiceBaseUrl}/certificates/{linkCode}");

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetSharingByIdQuery>(q => q.SharingId == sharingId && q.Limit == null),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetSharingById_When_Response_Is_Null_Returns_Null()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var govUkIdentifier = "gov-123";

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            var certificate = new Certificate
            {
                CertificateId = certificateId,
                CertificateType = CertificateType.Standard,
                CourseName = "Course Name",
                CourseLevel = "Level1",
                DateAwarded = DateTime.UtcNow
            };

            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate> { certificate });

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetSharingByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetSharingByIdQueryResult)null);

            // Act
            var result = await _sut.GetSharingById(certificateId, sharingId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void GetSharingById_When_Certificate_Not_Found_Throws_InvalidOperationException()
        {
            // Arrange
            var certificateId = Guid.NewGuid();
            var sharingId = Guid.NewGuid();
            var govUkIdentifier = "gov-123";

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate>());

            // Act + Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetSharingById(certificateId, sharingId));

            exception.Message.Should().Be($"Certificate {certificateId} not found for authenticated user");
        }
    }
}