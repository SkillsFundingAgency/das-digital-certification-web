using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Enums;

namespace SFA.DAS.DigitalCertificates.Web.UnitTests.Controllers
{
    public class AuthoriseControllerTests
    {
        private Mock<IHttpContextAccessor> _contextAccessorMock;
        private Mock<ISessionService> _sessionServiceMock;
        private Mock<IAuthoriseOrchestrator> _orchestratorMock;
        private AuthoriseController _sut;

        [SetUp]
        public void SetUp()
        {
            _contextAccessorMock = new Mock<IHttpContextAccessor>();
            _sessionServiceMock = new Mock<ISessionService>();
            _orchestratorMock = new Mock<IAuthoriseOrchestrator>();

            _sut = new AuthoriseController(_contextAccessorMock.Object, _sessionServiceMock.Object, _orchestratorMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sut.Dispose();
        }

        [Test]
        public async Task NeedMoreInformation_When_NotAuthorised_Prepares_And_Returns_View()
        {
            // Act
            var result = await _sut.NeedMoreInformation();

            // Assert
            _orchestratorMock.Verify(o => o.PrepareNeedMoreInformationAsync(), Times.Once);
            result.Should().BeOfType<ViewResult>();
        }

        [Test]
        public async Task KnowYourUln_Get_Populates_Model_From_Session()
        {
            // Arrange
            var vm = new KnowYourUlnViewModel { KnowUln = true, Uln = 1234567890L };
            _orchestratorMock.Setup(o => o.GetKnowYourUlnViewModelAsync()).ReturnsAsync(vm);

            // Act
            var result = await _sut.KnowYourUln();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view.Model.Should().BeOfType<KnowYourUlnViewModel>();
            var model = view.Model as KnowYourUlnViewModel;
            model.KnowUln.Should().BeTrue();
            model.Uln.Should().Be(1234567890L);
        }

        [Test]
        public async Task KnowYourUln_Post_Invalid_Redirects_To_Get()
        {
            // Arrange
            var vm = new KnowYourUlnViewModel { KnowUln = true, Uln = 1234567890L };
            _orchestratorMock.Setup(o => o.ValidateKnowYourUlnViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(false);

            // Act
            var result = await _sut.KnowYourUln(vm);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.KnowYourUlnRouteGet);
        }

        [Test]
        public async Task KnowYourUln_Post_Valid_Saves_To_Session_And_Returns_View()
        {
            // Arrange
            var vm = new KnowYourUlnViewModel { KnowUln = true, Uln = 1234567890L };
            _orchestratorMock.Setup(o => o.ValidateKnowYourUlnViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveKnowYourUlnAsync(vm)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.KnowYourUln(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveKnowYourUlnAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.SelectCourseRouteGet);
        }

        [Test]
        public async Task KnowYear_Get_Populates_Model_From_Orchestrator()
        {
            // Arrange
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = 2018 };
            _orchestratorMock.Setup(o => o.GetKnowYearViewModelAsync()).ReturnsAsync(vm);

            // Act
            var result = await _sut.KnowYear();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view.Model.Should().BeOfType<KnowYearViewModel>();
            var model = view.Model as KnowYearViewModel;
            model.KnowYear.Should().BeTrue();
            model.YearCompleted.Should().Be(2018);
        }

        [Test]
        public async Task KnowYear_Post_Invalid_Redirects_To_Get()
        {
            // Arrange
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = 2018 };
            _orchestratorMock.Setup(o => o.ValidateKnowYearViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(false);

            // Act
            var result = await _sut.KnowYear(vm);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.KnowYearRouteGet);
        }

        [Test]
        public async Task KnowYear_Post_Valid_Saves_And_Returns_View()
        {
            // Arrange
            var vm = new KnowYearViewModel { KnowYear = true, YearCompleted = 2018 };
            _orchestratorMock.Setup(o => o.ValidateKnowYearViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveKnowYearAsync(vm)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.KnowYear(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveKnowYearAsync(vm), Times.Once);
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view.Model.Should().BeOfType<KnowYearViewModel>();
            var model = view.Model as KnowYearViewModel;
            model.KnowYear.Should().BeTrue();
            model.YearCompleted.Should().Be(2018);
        }

        [Test]
        public async Task SelectCourse_Get_Returns_View_With_Model()
        {
            // Arrange
            var model = new SelectCourseViewModel
            {
                SelectedCourseCode = "ABC123"
            };

            _orchestratorMock.Setup(o => o.GetSelectCourseViewModelAsync()).ReturnsAsync(model);

            // Act
            var result = await _sut.SelectCourse();

            // Assert
            _orchestratorMock.Verify(o => o.GetSelectCourseViewModelAsync(), Times.Once);
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view.Model.Should().BeOfType<SelectCourseViewModel>();
            var vm = view.Model as SelectCourseViewModel;
            vm.SelectedCourseCode.Should().Be("ABC123");
        }

        [Test]
        public async Task SelectCourse_Post_Invalid_Redirects_To_Get()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = null };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(false);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.SelectCourseRouteGet);
        }

        [Test]
        public async Task SelectCourse_Post_NoData_Redirects_To_CannotMatch()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "ABC123" };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).Returns(Task.CompletedTask);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(CourseMatchOutcome.NoData);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CannotMatchRouteGet);
        }

        [Test]
        public async Task SelectCourse_Post_NoMatch_Redirects_To_KnowYear()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "ABC123" };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).Returns(Task.CompletedTask);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(CourseMatchOutcome.NoMatch);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.KnowYearRouteGet);
        }

        [Test]
        public async Task SelectCourse_Post_MultipleMatches_Redirects_To_KnowYear()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "ABC123" };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).Returns(Task.CompletedTask);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(CourseMatchOutcome.MultipleMatches);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.KnowYearRouteGet);
        }

        [Test]
        public async Task SelectCourse_Post_SingleMatch_Redirects_To_CheckAnswers()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "ABC123" };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).Returns(Task.CompletedTask);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(CourseMatchOutcome.SingleMatch);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CheckAnswersRouteGet);
        }
    }
}
