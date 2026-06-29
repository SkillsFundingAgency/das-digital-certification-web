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
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_GovUkIdentifier_Is_Null_Does_Not_Call_CacheService()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns((string)null);

            // Act
            await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            _cacheServiceMock.Verify(c => c.GetOrCreateMatchesAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
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
            _cacheServiceMock.Verify(c => c.GetOrCreateMatchesAsync(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task PrepareNeedMoreInformationAsync_When_Identifiers_Present_Calls_CacheService()
        {
            // Arrange
            var govUkId = "gov-123";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync((Domain.Models.MatchesAndMasks)null);

            // Act
            await _sut.PrepareNeedMoreInformationAsync();

            // Assert
            _cacheServiceMock.Verify(c => c.GetOrCreateMatchesAsync(govUkId, userId), Times.Once);
        }

        [Test]
        public async Task GetSelectCourseViewModelAsync_Returns_Model_With_Courses_And_SelectedCode()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Masks.Add(new Mask { CourseCode = "M1", CourseName = "MaskName", CourseLevel = "3", CertificateType = CertificateType.Standard });
            matches.Matches.Add(new Domain.Models.Match { Uln = 0, CourseCode = "R1", CourseName = "RealName", CourseLevel = "4", CertificateType = CertificateType.Framework });

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers { CourseCode = "R1" });

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
            // Arrange
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

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match { Uln = 0, CourseCode = "C1", CourseName = "Course One", CourseLevel = "2", CertificateType = CertificateType.Standard });

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

            var vm = new SelectCourseViewModel { SelectedCourseCode = "C1" };

            // Act
            await _sut.SaveSelectedCourseAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.CourseCode == "C1" && a.CourseName == "Course One")), Times.Once);
        }

        [Test]
        public async Task GetKnowYourUlnViewModelAsync_Returns_New_Model_When_No_Answers()
        {
            // Arrange
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

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
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(answers);

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
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

            // Act
            await _sut.SaveKnowYourUlnAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.KnowUln == true && a.Uln == 999999L)), Times.Once);
        }

        [Test]
        public async Task SaveKnowYourUlnAsync_Sets_Uln_Null_When_KnowUln_False()
        {
            // Arrange
            var vm = new KnowYourUlnViewModel { KnowUln = false, Uln = 111111L };
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers { KnowUln = true, Uln = 222222L });

            // Act
            await _sut.SaveKnowYourUlnAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.KnowUln == false && a.Uln == null)), Times.Once);
        }

        [Test]
        public async Task GetKnowYearViewModelAsync_Returns_Model_With_Null_Properties_When_No_Answers()
        {
            // Arrange
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

            // Act
            var result = await _sut.GetKnowYearViewModelAsync();

            // Assert
            Assert.That(result?.KnowYear, Is.Null);
            Assert.That(result?.YearCompleted, Is.Null);
        }

        [Test]
        public async Task GetKnowYearViewModelAsync_Returns_Model_From_Session()
        {
            // Arrange
            var answers = new AuthorisationAnswers { KnowYear = true, YearCompleted = 2020 };
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(answers);

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
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

            // Act
            await _sut.SaveKnowYearAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.KnowYear == true && a.YearCompleted == 2019)), Times.Once);
        }

        [Test]
        public async Task SaveKnowYearAsync_Sets_YearCompleted_Null_When_KnowYear_False()
        {
            // Arrange
            var vm = new KnowYearViewModel { KnowYear = false, YearCompleted = 2000 };
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers { KnowYear = true, YearCompleted = 1999 });

            // Act
            await _sut.SaveKnowYearAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.KnowYear == false && a.YearCompleted == null)), Times.Once);
        }

        [Test]
        public async Task GetSelectProviderViewModelAsync_Returns_Model_With_Providers_And_SessionValues()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match { Uln = 0, ProviderName = "Provider A", Ukprn = 12345 });
            matches.Masks.Add(new Mask { ProviderName = "Provider B" });

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers { ProviderName = "Provider A", ProviderUnknown = false });

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
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers { ProviderName = "Old", ProviderUkprn = 999 });
            _sessionServiceMock.Setup(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>())).Returns(Task.CompletedTask);

            var vm = new SelectProviderViewModel { SelectedProviderUnknown = true };

            // Act
            await _sut.SaveSelectedProviderAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.ProviderUnknown == true && a.ProviderName == null && a.ProviderUkprn == null)), Times.Once);
        }

        [Test]
        public async Task SaveSelectedProviderAsync_When_ValidProvider_Saves_Ukprn_And_Name()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match { Uln = 0, ProviderName = "Provider A", Ukprn = 12345 });
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);
            _sessionServiceMock.Setup(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>())).Returns(Task.CompletedTask);

            var vm = new SelectProviderViewModel { SelectedProviderName = "Provider A" };

            // Act
            await _sut.SaveSelectedProviderAsync(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.ProviderName == "Provider A" && a.ProviderUkprn == 12345 && a.ProviderUnknown == false)), Times.Once);
        }

        [TestCase(2022)]
        [TestCase(2023)]
        [TestCase(2024)]
        public async Task SubmitCheckAnswersAsync_LongJourney_Submits_When_YearIsWithinOneYearOfAwardedDate(int yearCompleted)
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                CourseCode = "C1",
                Uln = 999999L,
                DateAwarded = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Unspecified),
                ProviderName = "Provider A",
                Ukprn = 12345L,
                CertificateType = CertificateType.Standard
            });

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId))
                .ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = 999999L,
                    CourseCode = null,
                    YearCompleted = yearCompleted,
                    ProviderUkprn = 12345L,
                    ProviderName = "Provider A"
                });

            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Surname, "Family"),
                new Claim(ClaimTypes.GivenName, "Given"),
                new Claim(ClaimTypes.DateOfBirth, "1990-01-01")
            }, "Test"));

            _mediatorMock.Setup(m => m.Send(It.IsAny<MediatR.IRequest<bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.SingleMatch));
            _mediatorMock.Verify(m => m.Send(It.Is<SubmitMatchCommand>(c => c.IsMatched), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(m => m.Send(It.IsAny<AuthoriseUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase(2021)]
        [TestCase(2025)]
        public async Task SubmitCheckAnswersAsync_LongJourney_DoesNotSubmit_When_YearIsMoreThanOneYearFromAwardedDate(int yearCompleted)
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                CourseCode = "C1",
                Uln = 999999L,
                DateAwarded = new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Unspecified),
                ProviderName = "Provider A",
                Ukprn = 12345L,
                CertificateType = CertificateType.Standard
            });

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId))
                .ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync())
                .ReturnsAsync(new AuthorisationAnswers
                {
                    Uln = 999999L,
                    CourseCode = null,
                    YearCompleted = yearCompleted,
                    ProviderUkprn = 12345L,
                    ProviderName = "Provider A"
                });

            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Surname, "Family"),
                new Claim(ClaimTypes.GivenName, "Given"),
                new Claim(ClaimTypes.DateOfBirth, "1990-01-01")
            }, "Test"));

            _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

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
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.CreateUserActionForCannotMatchAsync(ActionType.NotFound));
            _mediatorMock.Verify(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateUserActionForCannotMatchAsync_Calls_Mediator_And_Returns_ActionCode()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            // add name claims
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Surname, "Smith"), new Claim(ClaimTypes.GivenName, "John") });
            _httpContext.User = new ClaimsPrincipal(identity);

            var commandResult = new CreateUserActionCommandResult { ActionCode = "REF-1" };
            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateUserActionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(commandResult);

            // Act
            var result = await _sut.CreateUserActionForCannotMatchAsync(ActionType.NotFound);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo("REF-1"));
            _mediatorMock.Verify(m => m.Send(It.Is<CreateUserActionCommand>(c => c.UserId == userId && c.ActionType == ActionType.NotFound && c.FamilyName == "Smith" && c.GivenNames == "John"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void GetLatestUserActionReferenceAsync_Throws_When_UserId_Null()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetUserId()).Returns((Guid?)null);

            // Act / Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.GetLatestUserActionReferenceAsync(ActionType.NotFound));
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetUserActionsQuery>(), It.IsAny<CancellationToken>()), Times.Never);
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

            var queryResult = new GetUserActionsQueryResult { UserActions = new System.Collections.Generic.List<UserActionDetail> { older, newer, other } };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserActionsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(queryResult);

            // Act
            var result = await _sut.GetLatestUserActionReferenceAsync(ActionType.NotFound);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Is.EqualTo("NEW"));
            _mediatorMock.Verify(m => m.Send(It.Is<GetUserActionsQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetCheckAnswersViewModelAsync_Returns_Null_When_No_Answers()
        {
            // Arrange
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

            // Act
            var result = await _sut.GetCheckAnswersViewModelAsync();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCheckAnswersViewModelAsync_Returns_Null_When_FirstTwoQuestions_Unanswered()
        {
            // Arrange 
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers());

            // Act
            var result = await _sut.GetCheckAnswersViewModelAsync();

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetCheckAnswersViewModelAsync_Sets_IsReturningToCheck_And_Updates_Session()
        {
            // Arrange
            var answers = new AuthorisationAnswers { KnowUln = true, Uln = 123L, KnowYear = true, YearCompleted = 2020, ProviderName = "P", ProviderUkprn = 1 };
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(answers);
            _sessionServiceMock.Setup(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>())).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.GetCheckAnswersViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.IsReturningToCheck == true)), Times.Once);
            Assert.That(result.UlnDisplay, Is.EqualTo("123"));
            Assert.That(result.YearDisplay, Is.EqualTo("2020"));
            Assert.That(result.ProviderDisplay, Is.EqualTo("P"));
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Returns_NoData_When_Matches_Null()
        {
            // Arrange
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers());
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync((MatchesAndMasks)null);

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.NoData));
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Returns_Locked_When_FailedLimit_Reached()
        {
            // Arrange
            var govUkId = "gov-1";
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers());
            _cacheServiceMock.Setup(c => c.GetMatchFailCountAsync(govUkId)).ReturnsAsync(10);

            // Act
            var result = await _sut.SubmitCheckAnswersAsync();

            // Assert
            Assert.That(result, Is.EqualTo(MatchOutcome.Locked));
        }

        [Test]
        public async Task SubmitCheckAnswersAsync_Increments_FailedCount_And_Returns_NoMatch_Or_Locked()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers { Uln = null, CourseCode = "C" });
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Surname, "F"),
                new Claim(ClaimTypes.DateOfBirth, "1990-01-01")
            }, "Test"));

            var matches = new MatchesAndMasks { Matches = new List<Domain.Models.Match>() };
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _cacheServiceMock.Setup(c => c.IncrementMatchFailCountAsync(govUkId)).ReturnsAsync(1);

            // Act
            var result1 = await _sut.SubmitCheckAnswersAsync();

            // Assert 
            Assert.That(result1, Is.EqualTo(MatchOutcome.NoMatch));

            _cacheServiceMock.Setup(c => c.IncrementMatchFailCountAsync(govUkId)).ReturnsAsync(99);
            var result2 = await _sut.SubmitCheckAnswersAsync();
            Assert.That(result2, Is.EqualTo(MatchOutcome.Locked));
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

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                CourseCode = "C1",
                CourseName = "Course 1",
                DateAwarded = new DateTime(2019, 1, 1),
                Ukprn = 123,
                Uln = 11111111L
            });

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers
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

            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match
            {
                CourseCode = "C2",
                CourseName = "Course 2",
                DateAwarded = new DateTime(2018, 1, 1),
                Ukprn = 456,
                Uln = 22222222L
            });

            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers
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
