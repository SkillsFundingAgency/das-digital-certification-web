using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateCertificateSharing;
using SFA.DAS.DigitalCertificates.Application.Queries.GetCertificateSharingDetails;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class SharingOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ISessionStorageService> _sessionStorageServiceMock;
        private CertificateSharingOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _userServiceMock = new Mock<IUserService>();
            _sessionStorageServiceMock = new Mock<ISessionStorageService>();
            _sut = new CertificateSharingOrchestrator(_mediatorMock.Object, _userServiceMock.Object, _sessionStorageServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
        }

        [Test]
        public async Task GetCertificateSharings_Sends_Query_With_Correct_Values_And_Returns_ViewModel()
        {
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

            var queryResult = new GetCertificateSharingDetailsQueryResult
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = courseName,
                Sharings = new List<CertificateSharingDetailsQueryResultItem>
                {
                    new CertificateSharingDetailsQueryResultItem
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
                .Setup(m => m.Send(It.IsAny<GetCertificateSharingDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            var result = await _sut.GetCertificateSharings(certificateId);

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
                It.Is<GetCertificateSharingDetailsQuery>(q =>
                    q.UserId == userId &&
                    q.CertificateId == certificateId &&
                    q.Limit == 10),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task GetCertificateSharings_When_Query_Returns_Null_Returns_Empty_ViewModel()
        {
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
                .Setup(m => m.Send(It.IsAny<GetCertificateSharingDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetCertificateSharingDetailsQueryResult)null);

            var result = await _sut.GetCertificateSharings(certificateId);

            result.Should().NotBeNull();
            result.CertificateId.Should().Be(certificateId);
            result.CourseName.Should().Be("Test Course");
            result.CertificateType.Should().Be(CertificateType.Standard);
            result.Sharings.Should().BeEmpty();
        }
        [Test]
        public async Task GetCertificateSharings_When_Multiple_Sharings_Exist_Returns_All_Sharings()
        {
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

            var queryResult = new GetCertificateSharingDetailsQueryResult
            {
                UserId = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = courseName,
                Sharings = new List<CertificateSharingDetailsQueryResultItem>
                {
                    new CertificateSharingDetailsQueryResultItem
                    {
                        SharingId = sharingId1,
                        SharingNumber = 111111,
                        CreatedAt = createdAt1,
                        LinkCode = Guid.NewGuid(),
                        ExpiryTime = expiryTime1
                    },
                    new CertificateSharingDetailsQueryResultItem
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
                .Setup(m => m.Send(It.IsAny<GetCertificateSharingDetailsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            var result = await _sut.GetCertificateSharings(certificateId);

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
        public void GetCertificateSharings_When_Certificate_Not_Found_In_Session_Throws_InvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate>());

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetCertificateSharings(certificateId));

            exception.Message.Should().Be($"Certificate {certificateId} not found for authenticated user");
        }

        [Test]
        public async Task CreateCertificateSharing_Sends_Command_With_Correct_Values_And_Returns_Success()
        {
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

            var commandResult = new CreateCertificateSharingCommandResult
            {
                Userid = userId,
                CertificateId = certificateId,
                CertificateType = "Standard",
                CourseName = courseName,
                SharingId = sharingId,
                SharingNumber = 123456,
                CreatedAt = DateTime.UtcNow,
                LinkCode = Guid.NewGuid(),
                ExpiryTime = DateTime.UtcNow.AddDays(30)
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateCertificateSharingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commandResult);

            var result = await _sut.CreateCertificateSharing(certificateId);

            result.Should().Be(sharingId);

            _mediatorMock.Verify(m => m.Send(
                It.Is<CreateCertificateSharingCommand>(c =>
                    c.UserId == userId &&
                    c.CertificateId == certificateId &&
                    c.CertificateType == "Standard" &&
                    c.CourseName == courseName),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task CreateCertificateSharing_When_Command_Returns_Null_Returns_Failed_Result()
        {
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
                .Setup(m => m.Send(It.IsAny<CreateCertificateSharingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateCertificateSharingCommandResult)null);

            var result = await _sut.CreateCertificateSharing(certificateId);

            result.Should().Be(Guid.Empty);
        }

        [Test]
        public void CreateCertificateSharing_When_Certificate_Not_Found_In_Session_Throws_InvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var certificateId = Guid.NewGuid();
            var govUkIdentifier = "test-gov-uk-id";

            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);
            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkIdentifier);

            _sessionStorageServiceMock.Setup(x => x.GetOwnedCertificatesAsync(govUkIdentifier))
                .ReturnsAsync(new List<Certificate>());

            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.CreateCertificateSharing(certificateId));

            exception.Message.Should().Be($"Certificate {certificateId} not found for authenticated user");
        }


    }
}