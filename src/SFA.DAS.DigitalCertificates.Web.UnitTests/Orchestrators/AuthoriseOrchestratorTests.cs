using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using MediatR;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
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
            _selectCourseValidatorMock = new Mock<IValidator<SelectCourseViewModel>>();

            _sut = new AuthoriseOrchestrator(_mediatorMock.Object, _sessionServiceMock.Object, _userServiceMock.Object, _cacheServiceMock.Object, _knowUlnValidatorMock.Object, _selectCourseValidatorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut = null;
            _knowUlnValidatorMock = null;
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
    }
}
