using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using MediatR;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Enums;
using FluentValidation;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Orchestrators
{
    [TestFixture]
    public class AuthoriseOrchestratorTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<ICacheService> _cacheServiceMock;
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IValidator<KnowYourUlnViewModel>> _knowUlnValidatorMock;
        private Mock<IValidator<KnowYearViewModel>> _knowYearValidatorMock;
        private Mock<IValidator<SelectCourseViewModel>> _selectCourseValidatorMock;
        private AuthoriseOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _userServiceMock = new Mock<IUserService>();
            _cacheServiceMock = new Mock<ICacheService>();
            _sessionServiceMock = new Mock<ISessionService>();
            _knowUlnValidatorMock = new Mock<IValidator<KnowYourUlnViewModel>>();
            _knowYearValidatorMock = new Mock<IValidator<KnowYearViewModel>>();
            _selectCourseValidatorMock = new Mock<IValidator<SelectCourseViewModel>>();

            _sut = new AuthoriseOrchestrator(
                _mediatorMock.Object,
                _sessionServiceMock.Object,
                _userServiceMock.Object,
                _cacheServiceMock.Object,
                _knowUlnValidatorMock.Object,
                _knowYearValidatorMock.Object,
                _selectCourseValidatorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
            _knowUlnValidatorMock = null;
            _knowYearValidatorMock = null;
            _selectCourseValidatorMock = null;
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
            matches.Matches.Add(new Domain.Models.Match { CourseCode = "R1", CourseName = "RealName", CourseLevel = "4", CertificateType = CertificateType.Framework });

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
        public async Task SaveSelectedCourseAsync_Ignores_Null_Or_Empty_Selection()
        {
            // Arrange
            // Act
            await _sut.SaveSelectedCourseAsync(null);
            await _sut.SaveSelectedCourseAsync(new SelectCourseViewModel { SelectedCourseCode = "   " });

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>()), Times.Never);
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
            matches.Matches.Add(new Domain.Models.Match { CourseCode = "C1", CourseName = "Course One", CourseLevel = "2", CertificateType = CertificateType.Standard });

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
        public async Task SaveKnowYourUlnAsync_Does_Nothing_When_ViewModel_Null()
        {
            // Act
            await _sut.SaveKnowYourUlnAsync(null);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>()), Times.Never);
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
        public async Task GetKnowYearViewModelAsync_Returns_New_Model_When_No_Answers()
        {
            // Arrange
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

            // Act
            var result = await _sut.GetKnowYearViewModelAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.KnowYear, Is.Null);
            Assert.That(result.YearCompleted, Is.Null);
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
        public async Task SaveKnowYearAsync_Does_Nothing_When_ViewModel_Null()
        {
            // Act
            await _sut.SaveKnowYearAsync(null);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.IsAny<AuthorisationAnswers>()), Times.Never);
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
        public async Task GetCourseMatchOutcomeAsync_Returns_NoData_When_ViewModel_Null()
        {
            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(null);

            // Assert
            Assert.That(result, Is.EqualTo(CourseMatchOutcome.NoData));
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_NoMatch_When_Selected_Empty()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "   " };

            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(vm);

            // Assert
            Assert.That(result, Is.EqualTo(CourseMatchOutcome.NoMatch));
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_NoData_When_Matches_Null()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync((MatchesAndMasks)null);

            var vm = new SelectCourseViewModel { SelectedCourseCode = "C1" };

            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(vm);

            // Assert
            Assert.That(result, Is.EqualTo(CourseMatchOutcome.NoData));
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_MultipleMatches_When_Multiple_Real_Matches()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match { CourseCode = "C1" });
            matches.Matches.Add(new Domain.Models.Match { CourseCode = "C1" });
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            var vm = new SelectCourseViewModel { SelectedCourseCode = "C1" };

            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(vm);

            // Assert
            Assert.That(result, Is.EqualTo(CourseMatchOutcome.MultipleMatches));
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_MultipleMatches_When_One_Real_And_Masks()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match { CourseCode = "C1" });
            matches.Masks.Add(new Mask { CourseCode = "C1" });
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            var vm = new SelectCourseViewModel { SelectedCourseCode = "C1" };

            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(vm);

            // Assert
            Assert.That(result, Is.EqualTo(CourseMatchOutcome.MultipleMatches));
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_SingleMatch_When_Exact_Single_Match()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Matches.Add(new Domain.Models.Match { CourseCode = "C1" });
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            var vm = new SelectCourseViewModel { SelectedCourseCode = "C1" };

            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(vm);

            // Assert
            Assert.That(result, Is.EqualTo(CourseMatchOutcome.SingleMatch));
        }

        [Test]
        public async Task GetCourseMatchOutcomeAsync_Returns_NoMatch_And_Increments_FailedCount_When_No_Real_Matches()
        {
            // Arrange
            var govUkId = "gov-1";
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetGovUkIdentifier()).Returns(govUkId);
            _userServiceMock.Setup(u => u.GetUserId()).Returns(userId);

            var matches = new MatchesAndMasks();
            matches.Masks.Add(new Mask { CourseCode = "C1" });
            _cacheServiceMock.Setup(c => c.GetOrCreateMatchesAsync(govUkId, userId)).ReturnsAsync(matches);

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(new AuthorisationAnswers { FailedMatchCount = 0 });

            var vm = new SelectCourseViewModel { SelectedCourseCode = "C1" };

            // Act
            var result = await _sut.GetCourseMatchOutcomeAsync(vm);

            // Assert
            Assert.That(result, Is.EqualTo(CourseMatchOutcome.NoMatch));
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.FailedMatchCount == 1)), Times.Once);
        }
    }
}
