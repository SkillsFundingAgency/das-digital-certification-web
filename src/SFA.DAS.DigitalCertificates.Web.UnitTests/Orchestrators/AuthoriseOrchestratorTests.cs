using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using MediatR;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Enums;
using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Application.Commands.SubmitMatch;
using SFA.DAS.DigitalCertificates.Application.Commands.AuthoriseUser;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction;
using SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions;
using System.Threading;
using SFA.DAS.DigitalCertificates.Infrastructure.Configuration;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class AuthoriseOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private DefaultHttpContext _httpContext;
        private Mock<IUserService> _userServiceMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IGovUkAuthenticationService> _govUkAuthenticationServiceMock;
        private Mock<IValidator<KnowYourUlnViewModel>> _knowUlnValidatorMock;
        private Mock<IValidator<KnowYearViewModel>> _knowYearValidatorMock;
        private Mock<IValidator<SelectCourseViewModel>> _selectCourseValidatorMock;
        private Mock<IValidator<SelectProviderViewModel>> _selectProviderValidatorMock;
        private AuthoriseOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _httpContext = new DefaultHttpContext();
            _httpContextAccessorMock.Setup(c => c.HttpContext).Returns(_httpContext);
            _userServiceMock = new Mock<IUserService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _sessionServiceMock = new Mock<ISessionService>();
            _govUkAuthenticationServiceMock = new Mock<IGovUkAuthenticationService>();
            _knowUlnValidatorMock = new Mock<IValidator<KnowYourUlnViewModel>>();
            _knowYearValidatorMock = new Mock<IValidator<KnowYearViewModel>>();
            _selectCourseValidatorMock = new Mock<IValidator<SelectCourseViewModel>>();
            _selectProviderValidatorMock = new Mock<IValidator<SelectProviderViewModel>>();

            _sut = new AuthoriseOrchestrator(
                _mediatorMock.Object,
                _httpContextAccessorMock.Object,
                _sessionServiceMock.Object,
                _userServiceMock.Object,
                _cacheServiceMock.Object,
                _govUkAuthenticationServiceMock.Object,
                _knowUlnValidatorMock.Object,
                _knowYearValidatorMock.Object,
                _selectCourseValidatorMock.Object,
                _selectProviderValidatorMock.Object,
                new DigitalCertificatesWebConfiguration
                {
                    ServiceBaseUrl = "https://test.local",
                    OneLoginSettingsUrl = "https://onelogin",
                    RedisConnectionString = "localhost",
                    DataProtectionKeysDatabase = "keys",
                    ContainerName = "container",
                    AsposeLicenseContainerName = "aspose-container",
                    StandardTemplateBlobName = "standard",
                    GreenStandardTemplateBlobName = "green",
                    FrameworkTemplateBlobName = "framework",
                    LicenseBlobName = "license",
                    MasterPassword = "master",
                    StorageConnectionString = "UseDevelopmentStorage=true"
                });
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
            _knowUlnValidatorMock = null;
            _knowYearValidatorMock = null;
            _selectCourseValidatorMock = null;
            _selectProviderValidatorMock = null;
            _sessionServiceMock = null;
            _govUkAuthenticationServiceMock = null;
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_GovUkIdentifier_Is_Null_Does_Not_Call_CacheService()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns((string)null);

            // Act
            await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            _cacheServiceMock.Verify(c => c.GetMatchesAsync(It.IsAny<string>()), Times.Never);
            _cacheServiceMock.Verify(c => c.CreateMatchesAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<GovUkCredentialSubject>()), Times.Never);
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_UserId_Is_Null_Does_Not_Call_CacheService()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns("gov-123");
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);

            // Act
            await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            _cacheServiceMock.Verify(c => c.GetMatchesAsync(It.IsAny<string>()), Times.Never);
            _cacheServiceMock.Verify(c => c.CreateMatchesAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<GovUkCredentialSubject>()), Times.Never);
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_Cached_Matches_Exist_Uses_Cached_Matches()
        {
            // Arrange
            var govUkId = "gov-123";
            var userId = Guid.NewGuid();

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 1234567890,
                UserIdentityId = Guid.NewGuid()
            });
            matches.Masks.Add(new Mask
            {
                CourseCode = "M1",
                CourseName = "Mask Course"
            });

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            // Act
            var result = await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            Assert.That(result, Is.True);

            _cacheServiceMock.Verify(c => c.GetMatchesAsync(govUkId), Times.Once);
            _cacheServiceMock.Verify(c => c.CreateMatchesAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<GovUkCredentialSubject>()), Times.Never);
        }

        [Test]
        public async Task GetSelectCourseViewModelAsync_Returns_Model_With_Courses_And_SelectedCode()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Masks.Add(new Mask
            {
                CourseCode = "M1",
                CourseName = "MaskName",
                CourseLevel = "3",
                CertificateType = CertificateType.Standard
            });
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 0,
                UserIdentityId = userIdentityId,
                CourseCode = "R1",
                CourseName = "RealName",
                CourseLevel = "4",
                CertificateType = CertificateType.Framework
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers { CourseCode = "R1" });

            // Act
            var result = await _sut.GetSelectCourseViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.SelectedCourseCode, Is.EqualTo("R1"));
            Assert.IsNotNull(result.Courses);
            Assert.That(result.Courses.Count, Is.EqualTo(2));
            Assert.IsTrue(result.Courses.Exists(c => c.CourseCode == "M1" && c.CourseName == "MaskName"));
            Assert.IsTrue(result.Courses.Exists(c => c.CourseCode == "R1" && c.CourseName == "RealName"));
        }

        [Test]
        public async Task SaveSelectedCourseAsync_Ignores_Empty_Selection()
        {
            // Act
            await _sut.SaveSelectedCourseAsync(new SelectCourseViewModel { SelectedCourseCode = "   " });

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>()), Times.Once);
        }

        [Test]
        public async Task SaveSelectedCourseAsync_Saves_CourseCode_And_Name_When_Valid()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 0,
                UserIdentityId = userIdentityId,
                CourseCode = "C1",
                CourseName = "Course One",
                CourseLevel = "2",
                CertificateType = CertificateType.Standard
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync((AuthorisationAnswers)null);

            var vm = new SelectCourseViewModel { SelectedCourseCode = "C1" };

            // Act
            await _sut.SaveSelectedCourseAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a =>
                a.CourseCode == "C1" &&
                a.CourseName == "Course One")), Times.Once);
        }

        [Test]
        public async Task GetKnowYourUlnViewModelAsync_Returns_New_Model_When_No_Answers()
        {
            // Arrange
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync((AuthorisationAnswers)null);

            // Act
            var result = await _sut.GetKnowYourUlnViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.KnowUln, Is.Null);
            Assert.That(result.Uln, Is.Null);
        }

        [Test]
        public async Task GetKnowYourUlnViewModelAsync_Returns_Model_From_Session()
        {
            // Arrange
            var answers = new AuthorisationAnswers { KnowUln = true, Uln = 1234567890L };
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(answers);

            // Act
            var result = await _sut.GetKnowYourUlnViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.KnowUln, Is.EqualTo(true));
            Assert.That(result.Uln, Is.EqualTo(1234567890L));
        }

        [Test]
        public async Task SaveKnowYourUlnAsync_Saves_KnowUln_True_And_Uln()
        {
            // Arrange
            var vm = new KnowYourUlnViewModel { KnowUln = true, Uln = 999999L };
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync((AuthorisationAnswers)null);

            // Act
            await _sut.SaveKnowYourUlnAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a =>
                a.KnowUln == true &&
                a.Uln == 999999L)), Times.Once);
        }

        [Test]
        public async Task SaveKnowYourUlnAsync_Sets_Uln_Null_When_KnowUln_False()
        {
            // Arrange
            var vm = new KnowYourUlnViewModel { KnowUln = false, Uln = 111111L };
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers { KnowUln = true, Uln = 222222L });

            // Act
            await _sut.SaveKnowYourUlnAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a =>
                a.KnowUln == false &&
                a.Uln == null)), Times.Once);
        }

        [Test]
        public async Task GetKnowYearViewModelAsync_Returns_Null_When_No_Answers()
        {
            // Arrange
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync((AuthorisationAnswers)null);

            // Act
            var result = await _sut.GetKnowYearViewModelAsync();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetKnowYearViewModelAsync_Returns_Model_From_Session()
        {
            // Arrange
            var answers = new AuthorisationAnswers { KnowYear = true, YearCompleted = 2020 };
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(answers);

            // Act
            var result = await _sut.GetKnowYearViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.KnowYear, Is.EqualTo(true));
            Assert.That(result.YearCompleted, Is.EqualTo(2020));
        }

        [Test]
        public async Task SaveKnowYearAsync_Saves_KnowYear_True_And_Year()
        {
            // Arrange
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = 2019 };
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync((AuthorisationAnswers)null);

            // Act
            await _sut.SaveKnowYearAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a =>
                a.KnowYear == true &&
                a.YearCompleted == 2019)), Times.Once);
        }

        [Test]
        public async Task SaveKnowYearAsync_Sets_YearCompleted_Null_When_KnowYear_False()
        {
            // Arrange
            var vm = new KnowYearViewModel { KnowYear = false, YearCompleted = 2000 };
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers { KnowYear = true, YearCompleted = 1999 });

            // Act
            await _sut.SaveKnowYearAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a =>
                a.KnowYear == false &&
                a.YearCompleted == null)), Times.Once);
        }

        [Test]
        public async Task GetSelectProviderViewModelAsync_Returns_Model_With_Providers_And_SessionValues()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 0,
                UserIdentityId = userIdentityId,
                ProviderName = "Provider A",
                Ukprn = 12345
            });
            matches.Masks.Add(new Mask { ProviderName = "Provider B" });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers { ProviderName = "Provider A", ProviderUnknown = false });

            // Act
            var result = await _sut.GetSelectProviderViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.SelectedProviderName, Is.EqualTo("Provider A"));
            Assert.That(result.SelectedProviderUnknown, Is.EqualTo(false));
            Assert.IsNotNull(result.Providers);
            Assert.IsTrue(result.Providers.Exists(p => p.ProviderName == "Provider A" && p.Ukprn == 12345));
            Assert.IsTrue(result.Providers.Exists(p => p.ProviderName == "Provider B" && p.Ukprn == null));
        }

        [Test]
        public async Task SaveSelectedProviderAsync_When_Unknown_Sets_Unknown_And_Clears_Other_Fields()
        {
            // Arrange
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers { ProviderName = "Old", ProviderUkprn = 999 });
            _sessionServiceMock
                .Setup(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>()))
                .Returns(Task.CompletedTask);

            var vm = new SelectProviderViewModel { SelectedProviderUnknown = true };

            // Act
            await _sut.SaveSelectedProviderAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a =>
                a.ProviderUnknown == true &&
                a.ProviderName == null &&
                a.ProviderUkprn == null)), Times.Once);
        }

        [Test]
        public async Task SaveSelectedProviderAsync_When_ValidProvider_Saves_Ukprn_And_Name()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 0,
                UserIdentityId = userIdentityId,
                ProviderName = "Provider A",
                Ukprn = 12345
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            var vm = new SelectProviderViewModel { SelectedProviderName = "Provider A" };

            // Act
            await _sut.SaveSelectedProviderAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a =>
                a.ProviderName == "Provider A" &&
                a.ProviderUkprn == 12345 &&
                a.ProviderUnknown == false)), Times.Once);
        }

        [TestCase(2022)]
        [TestCase(2023)]
        [TestCase(2024)]
        public async Task SubmitCheckAnswersAsync_LongJourney_Submits_When_YearIsWithinOneYearOfAwardedDate(int yearCompleted)
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 999999L,
                UserIdentityId = userIdentityId,
                ProviderName = "Provider A",
                Ukprn = 12345L,
                CertificateType = CertificateType.Standard,
                CourseCode = "C1",
                CourseName = "Course One",
                CourseLevel = "3",
                DateAwarded = new DateTime(2023, 1, 1)
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = 999999L,
                    CourseCode = null,
                    YearCompleted = yearCompleted,
                    ProviderUkprn = 12345L,
                    ProviderName = "Provider A"
                });

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.SingleMatch));

            _mediatorMock.Verify(m => m.Send(
                It.Is<SubmitMatchCommand>(c =>
                    c.UserId == userId &&
                    c.Uln == 999999L &&
                    c.UserIdentityId == userIdentityId &&
                    c.CertificateType == CertificateType.Standard.ToString() &&
                    c.CourseCode == "C1" &&
                    c.CourseName == "Course One" &&
                    c.CourseLevel == "3" &&
                    c.YearAwarded == 2023 &&
                    c.ProviderName == "Provider A" &&
                    c.Ukprn == 12345 &&
                    c.IsMatched &&
                    !c.IsFailed),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _mediatorMock.Verify(m => m.Send(
                It.Is<AuthoriseUserCommand>(c =>
                    c.UserId == userId &&
                    c.Uln == 999999L),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestCase(2021)]
        [TestCase(2025)]
        public async Task SubmitCheckAnswersAsync_LongJourney_DoesNotSubmit_When_YearIsMoreThanOneYearFromAwardedDate(int yearCompleted)
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 999999L,
                UserIdentityId = userIdentityId,
                ProviderName = "Provider A",
                Ukprn = 12345L,
                CertificateType = CertificateType.Standard,
                CourseCode = "C1",
                DateAwarded = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Unspecified)
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = 999999L,
                    CourseCode = null,
                    YearCompleted = yearCompleted,
                    ProviderUkprn = 12345L,
                    ProviderName = "Provider A"
                });

            _cacheServiceMock
                .Setup(c => c.IncrementMatchFailCountAsync(govUkId))
                .ReturnsAsync(1);

            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Surname, "Family"),
                new Claim(ClaimTypes.GivenName, "Given"),
                new Claim(ClaimTypes.DateOfBirth, "1990-01-01")
            }, "Test"));

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoMatch));
            _mediatorMock.Verify(m => m.Send(It.Is<SubmitMatchCommand>(c => !c.IsMatched), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(m => m.Send(It.IsAny<AuthoriseUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void CreateUserActionForCannotMatchAsync_Throws_When_UserId_Null()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);

            // Act / Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.CreateUserActionForCannotMatchAsync(ActionType.NotFound));

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<CreateUserActionCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateUserActionForCannotMatchAsync_Calls_Mediator_And_Returns_ActionCode()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Surname, "Smith"),
                new Claim(ClaimTypes.GivenName, "John")
            });
            _httpContext.User = new ClaimsPrincipal(identity);

            var commandResult = new CreateUserActionCommandResult { ActionCode = "REF-1" };
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(commandResult);

            // Act
            var result = await _sut.CreateUserActionForCannotMatchAsync(ActionType.NotFound);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo("REF-1"));
            _mediatorMock.Verify(m => m.Send(It.Is<CreateUserActionCommand>(c =>
                c.UserId == userId &&
                c.ActionType == ActionType.NotFound &&
                c.FamilyName == "Smith" &&
                c.GivenNames == "John"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetLatestUserActionReferenceAsync_Throws_When_UserId_Null()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);

            // Act / Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetLatestUserActionReferenceAsync(ActionType.NotFound));

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<GetUserActionsQuery>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetLatestUserActionReferenceAsync_Returns_Most_Recent_ActionCode_For_Specified_ActionType()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var older = new UserActionDetail
            {
                Id = 1,
                UserId = userId,
                ActionType = ActionType.NotFound,
                ActionTime = DateTime.UtcNow.AddHours(-2),
                ActionStatus = UserActionStatus.New,
                FamilyName = "Smith",
                GivenNames = "John",
                ActionCode = "OLD"
            };

            var newer = new UserActionDetail
            {
                Id = 2,
                UserId = userId,
                ActionType = ActionType.NotFound,
                ActionTime = DateTime.UtcNow,
                ActionStatus = UserActionStatus.New,
                FamilyName = "Smith",
                GivenNames = "John",
                ActionCode = "NEW"
            };

            var other = new UserActionDetail
            {
                Id = 3,
                UserId = userId,
                ActionType = ActionType.NotMatched,
                ActionTime = DateTime.UtcNow,
                ActionStatus = UserActionStatus.New,
                FamilyName = "Jones",
                GivenNames = "Anna",
                ActionCode = "OTHER"
            };

            var queryResult = new GetUserActionsQueryResult
            {
                UserActions = new List<UserActionDetail> { older, newer, other }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetUserActionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(queryResult);

            // Act
            var result = await _sut.GetLatestUserActionReferenceAsync(ActionType.NotFound);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo("NEW"));
            _mediatorMock.Verify(m => m.Send(
                It.Is<GetUserActionsQuery>(q => q.UserId == userId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetCheckAnswersViewModelAsync_Returns_Null_When_No_Answers()
        {
            // Arrange
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync((AuthorisationAnswers)null);

            // Act
            var result = await _sut.GetCheckAnswersViewModelAsync();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCheckAnswersViewModelAsync_Returns_Null_When_FirstTwoQuestions_Unanswered()
        {
            // Arrange
            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers());

            // Act
            var result = await _sut.GetCheckAnswersViewModelAsync();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCheckAnswersViewModelAsync_Sets_IsReturningToCheck_And_Updates_Session()
        {
            // Arrange
            var answers = new AuthorisationAnswers
            {
                KnowUln = true,
                Uln = 123L,
                KnowYear = true,
                YearCompleted = 2020,
                ProviderName = "P",
                ProviderUkprn = 1
            };

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(answers);
            _sessionServiceMock
                .Setup(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetCheckAnswersViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(
                It.Is<AuthorisationAnswers>(a => a.IsReturningToCheck == true)), Times.Once);
            Assert.That(result.UlnDisplay, Is.EqualTo("123"));
            Assert.That(result.YearDisplay, Is.EqualTo("2020"));
            Assert.That(result.ProviderDisplay, Is.EqualTo("P"));
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Returns_NoData_When_Matches_Null()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers());

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(new MatchesAndMasks
                {
                    Matches = null
                });

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoData));

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<SubmitMatchCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Returns_NoData_When_Matches_List_Is_Empty()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers());

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(new MatchesAndMasks
                {
                    Matches = new List<Domain.Models.Match>()
                });

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoData));

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<SubmitMatchCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Returns_Locked_When_FailedLimit_Reached()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers());

            _cacheServiceMock
                .Setup(c => c.GetMatchFailCountAsync(govUkId))
                .ReturnsAsync(10);

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.Locked));

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<SubmitMatchCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void SubmitCheckAnswersAsync_Throws_When_UserId_Is_Null()
        {
            // Arrange
            var govUkId = "gov-1";

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers());

            // Act / Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.SubmitCheckAnswersAsync());

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<SubmitMatchCommand>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Increments_FailedCount_And_Returns_NoMatch()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 111111L,
                UserIdentityId = userIdentityId,
                CourseCode = "OTHER",
                CourseName = "Other Course",
                ProviderName = "Provider A",
                Ukprn = 12345L,
                CertificateType = CertificateType.Standard,
                DateAwarded = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = null,
                    CourseCode = "C1",
                    CourseName = "Course One",
                    YearCompleted = 2019,
                    ProviderUkprn = 12345L,
                    ProviderName = "Provider A"
                });

            _cacheServiceMock
                .Setup(c => c.IncrementMatchFailCountAsync(govUkId))
                .ReturnsAsync(1);

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoMatch));

            _mediatorMock.Verify(m => m.Send(
                It.Is<SubmitMatchCommand>(c =>
                    c.UserId == userId &&
                    c.Uln == null &&
                    c.UserIdentityId == null &&
                    c.CertificateType == null &&
                    c.CourseCode == "C1" &&
                    c.CourseName == "Course One" &&
                    c.CourseLevel == null &&
                    c.YearAwarded == 2019 &&
                    c.ProviderName == "Provider A" &&
                    c.Ukprn == 12345 &&
                    !c.IsMatched &&
                    !c.IsFailed),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _mediatorMock.Verify(m => m.Send(
                It.IsAny<AuthoriseUserCommand>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Increments_FailedCount_And_Returns_Locked_When_FailedLimit_Reached()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 111111L,
                UserIdentityId = userIdentityId,
                CourseCode = "OTHER",
                CourseName = "Other Course",
                ProviderName = "Provider A",
                Ukprn = 12345L,
                CertificateType = CertificateType.Standard,
                DateAwarded = new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = null,
                    CourseCode = "C1",
                    CourseName = "Course One",
                    YearCompleted = 2019,
                    ProviderUkprn = 12345L,
                    ProviderName = "Provider A"
                });

            _cacheServiceMock
                .Setup(c => c.IncrementMatchFailCountAsync(govUkId))
                .ReturnsAsync(2);

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.Locked));

            _mediatorMock.Verify(m => m.Send(
                It.Is<SubmitMatchCommand>(c =>
                    c.UserId == userId &&
                    c.UserIdentityId == null &&
                    !c.IsMatched &&
                    c.IsFailed),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _cacheServiceMock.Verify(c => c.ClearUser(govUkId), Times.Once);
            _cacheServiceMock.Verify(c => c.ClearMatchFailCountAsync(govUkId), Times.Once);
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_Matches_Not_Cached_And_HttpContext_Is_Null_Does_Not_Create_Matches()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);

            _httpContextAccessorMock
                .Setup(x => x.HttpContext)
                .Returns((HttpContext)null);

            _cacheServiceMock
                .Setup(x => x.GetMatchesAsync(govUkId))
                .ReturnsAsync((MatchesAndMasks)null);

            // Act
            var result = await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            Assert.That(result, Is.False);

            _cacheServiceMock.Verify(x => x.GetMatchesAsync(govUkId), Times.Once);
            _cacheServiceMock.Verify(x => x.CreateMatchesAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<GovUkCredentialSubject>()), Times.Never);
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_When_SingleMatch_Found_Submits_UserIdentityId_From_Match()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 1234567890,
                UserIdentityId = userIdentityId,
                CertificateType = CertificateType.Standard,
                CourseCode = "C1",
                CourseName = "Course One",
                CourseLevel = "3",
                ProviderName = "Provider One",
                Ukprn = 12345678,
                DateAwarded = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            });

            _cacheServiceMock
                .Setup(x => x.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(x => x.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = 1234567890,
                    CourseCode = "C1",
                    YearCompleted = 2023,
                    ProviderUkprn = 12345678,
                    ProviderName = "Provider One"
                });

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.SingleMatch));

            _mediatorMock.Verify(x => x.Send(
                It.Is<SubmitMatchCommand>(command =>
                    command.UserId == userId &&
                    command.Uln == 1234567890 &&
                    command.UserIdentityId == userIdentityId &&
                    command.CertificateType == CertificateType.Standard.ToString() &&
                    command.CourseCode == "C1" &&
                    command.CourseName == "Course One" &&
                    command.CourseLevel == "3" &&
                    command.YearAwarded == 2023 &&
                    command.ProviderName == "Provider One" &&
                    command.Ukprn == 12345678 &&
                    command.IsMatched &&
                    !command.IsFailed),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _mediatorMock.Verify(x => x.Send(
                It.Is<AuthoriseUserCommand>(command =>
                    command.UserId == userId &&
                    command.Uln == 1234567890),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_When_NoMatch_Submits_Null_UserIdentityId()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(x => x.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(x => x.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 9999999999,
                UserIdentityId = Guid.NewGuid(),
                CertificateType = CertificateType.Standard,
                CourseCode = "OTHER",
                CourseName = "Other Course",
                ProviderName = "Other Provider",
                Ukprn = 87654321,
                DateAwarded = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified)
            });

            _cacheServiceMock
                .Setup(x => x.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _cacheServiceMock
                .Setup(x => x.IncrementMatchFailCountAsync(govUkId))
                .ReturnsAsync(1);

            _sessionServiceMock
                .Setup(x => x.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = 1234567890,
                    CourseCode = "C1",
                    CourseName = "Course One",
                    YearCompleted = 2023,
                    ProviderUkprn = 12345678,
                    ProviderName = "Provider One"
                });

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoMatch));

            _mediatorMock.Verify(x => x.Send(
                It.Is<SubmitMatchCommand>(command =>
                    command.UserId == userId &&
                    command.Uln == 1234567890 &&
                    command.UserIdentityId == null &&
                    command.CertificateType == null &&
                    command.CourseCode == "C1" &&
                    command.CourseName == "Course One" &&
                    command.CourseLevel == null &&
                    command.YearAwarded == 2023 &&
                    command.ProviderName == "Provider One" &&
                    command.Ukprn == 12345678 &&
                    !command.IsMatched &&
                    !command.IsFailed),
                It.IsAny<CancellationToken>()),
                Times.Once);

            _mediatorMock.Verify(x => x.Send(
                It.IsAny<AuthoriseUserCommand>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_NoData_When_ViewModel_Null()
        {
            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(null);

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoData));
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_SingleMatch_When_Match_Found()
        {
            // Arrange
            var govUkId = "gov-101";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 11111111L,
                UserIdentityId = userIdentityId,
                CourseCode = "C1",
                CourseName = "Course 1",
                DateAwarded = new DateTime(2019, 1, 1),
                Ukprn = 123
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    CourseCode = "C1",
                    YearCompleted = 2019,
                    ProviderUkprn = 123
                });

            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(new SelectCourseViewModel { SelectedCourseCode = "C1" });

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.SingleMatch));
        }

        [Test]
        public async Task GetUlnMatchOutcomeAsync_Returns_NoData_When_ViewModel_Null()
        {
            // Act
            var result = await _sut.GetUlnMatchOutcomeAsync(null);

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoData));
        }

        [Test]
        public async Task GetUlnMatchOutcomeAsync_Returns_SingleMatch_When_Match_Found()
        {
            // Arrange
            var govUkId = "gov-201";
            var userId = Guid.NewGuid();
            var userIdentityId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                Uln = 22222222L,
                UserIdentityId = userIdentityId,
                CourseCode = "C2",
                CourseName = "Course 2",
                DateAwarded = new DateTime(2018, 1, 1),
                Ukprn = 456
            });

            _cacheServiceMock
                .Setup(c => c.GetMatchesAsync(govUkId))
                .ReturnsAsync(matches);

            _sessionServiceMock
                .Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    CourseCode = "C2",
                    Uln = 22222222L,
                    IsShortJourney = true
                });

            // Act
            var result = await _sut.GetUlnMatchOutcomeAsync(new KnowYourUlnViewModel { Uln = 22222222L });

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.SingleMatch));
        }
    }
}