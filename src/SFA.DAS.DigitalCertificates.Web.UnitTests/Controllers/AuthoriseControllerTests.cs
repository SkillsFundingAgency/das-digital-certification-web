using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Extensions;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Enums;
using System.Collections.Generic;

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
            // Arrange
            _orchestratorMock.Setup(o => o.PrepareNeedMoreInformationAsync()).ReturnsAsync(true);

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
            _orchestratorMock.Setup(o => o.SaveKnowYourUlnAsync(vm)).ReturnsAsync(vm);

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
            _orchestratorMock.Setup(o => o.SaveKnowYearAsync(vm)).ReturnsAsync(vm);
            // Act
            var result = await _sut.KnowYear(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveKnowYearAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.SelectProviderRouteGet);
        }

        [Test]
        public async Task SelectCourse_Get_Returns_View_With_Model()
        {
            // Arrange
            var model = new SelectCourseViewModel
            {
                SelectedCourseCode = "ABC123"
            };

            model.Courses = new List<SelectCourseViewModel.CourseOption>
            {
                new SelectCourseViewModel.CourseOption { CourseCode = "ABC123", CourseName = "Course 1" }
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
        public async Task SelectCourse_Post_NoData_Redirects_To_NotFound()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "ABC123" };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).ReturnsAsync(vm);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(MatchOutcome.NoData);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.NotFoundRouteGet);
        }

        [Test]
        public async Task SelectCourse_Post_NoMatch_Redirects_To_KnowYear()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "ABC123" };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).ReturnsAsync(vm);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(MatchOutcome.NoMatch);

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
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).ReturnsAsync(vm);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(MatchOutcome.MultipleMatches);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CheckAnswersRouteGet);
        }

        [Test]
        public async Task SelectCourse_Post_SingleMatch_Redirects_To_CheckAnswers()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = "ABC123" };
            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(vm, It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(vm)).ReturnsAsync(vm);
            _orchestratorMock.Setup(o => o.GetCourseMatchOutcomeAsync(vm)).ReturnsAsync(MatchOutcome.SingleMatch);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(vm), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CheckAnswersRouteGet);
        }

        [Test]
        public async Task SelectCourse_Post_Unknown_Sets_Unknown_And_Saves()
        {
            // Arrange
            var vm = new SelectCourseViewModel { SelectedCourseCode = SelectCourseViewModel.UnknownCourseSentinel };

            _orchestratorMock.Setup(o => o.ValidateSelectCourseViewModel(It.IsAny<SelectCourseViewModel>(), It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedCourseAsync(It.IsAny<SelectCourseViewModel>())).ReturnsAsync((SelectCourseViewModel m) => m);

            // Act
            var result = await _sut.SelectCourse(vm);

            // Assert 
            _orchestratorMock.Verify(o => o.SaveSelectedCourseAsync(It.Is<SelectCourseViewModel>(m => m.SelectedCourseUnknown == true && m.SelectedCourseCode == null)), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
        }

        [Test]
        public async Task SelectProvider_Get_Returns_View_With_Model()
        {
            // Arrange
            var model = new SelectProviderViewModel
            {
                SelectedProviderName = "Provider A"
            };

            model.Providers = new List<SelectProviderViewModel.ProviderOption>
            {
                new SelectProviderViewModel.ProviderOption { ProviderName = "Provider A" }
            };

            _orchestratorMock.Setup(o => o.GetSelectProviderViewModelAsync()).ReturnsAsync(model);

            // Act
            var result = await _sut.SelectProvider();

            // Assert
            _orchestratorMock.Verify(o => o.GetSelectProviderViewModelAsync(), Times.Once);
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            view.Model.Should().BeOfType<SelectProviderViewModel>();
            var vm = view.Model as SelectProviderViewModel;
            vm.SelectedProviderName.Should().Be("Provider A");
        }

        [Test]
        public async Task SelectProvider_Post_Invalid_Redirects_To_Get()
        {
            // Arrange -
            var vm = new SelectProviderViewModel { SelectedProviderName = null };
            _orchestratorMock.Setup(o => o.ValidateSelectProviderViewModel(It.IsAny<SelectProviderViewModel>(), It.IsAny<ModelStateDictionary>())).ReturnsAsync(false);

            // Act
            var result = await _sut.SelectProvider(vm);

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.SelectProviderRouteGet);
        }

        [Test]
        public async Task SelectProvider_Post_Unknown_Sets_Unknown_And_Saves()
        {
            // Arrange
            var vm = new SelectProviderViewModel { SelectedProviderName = SelectProviderViewModel.UnknownProviderSentinel };

            _orchestratorMock.Setup(o => o.ValidateSelectProviderViewModel(It.IsAny<SelectProviderViewModel>(), It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedProviderAsync(It.IsAny<SelectProviderViewModel>())).ReturnsAsync((SelectProviderViewModel m) => m);

            // Act
            var result = await _sut.SelectProvider(vm);

            // Assert 
            _orchestratorMock.Verify(o => o.SaveSelectedProviderAsync(It.Is<SelectProviderViewModel>(m => m.SelectedProviderUnknown == true && m.SelectedProviderName == null)), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CheckAnswersRouteGet);
        }

        [Test]
        public async Task SelectProvider_Post_Valid_Saves_And_Returns_View()
        {
            // Arrange 
            var vm = new SelectProviderViewModel { SelectedProviderName = "Provider A" };

            _orchestratorMock.Setup(o => o.ValidateSelectProviderViewModel(It.IsAny<SelectProviderViewModel>(), It.IsAny<ModelStateDictionary>())).ReturnsAsync(true);
            _orchestratorMock.Setup(o => o.SaveSelectedProviderAsync(It.IsAny<SelectProviderViewModel>())).ReturnsAsync((SelectProviderViewModel m) => m);

            // Act
            var result = await _sut.SelectProvider(vm);

            // Assert
            _orchestratorMock.Verify(o => o.SaveSelectedProviderAsync(It.Is<SelectProviderViewModel>(m => m.SelectedProviderUnknown != true && m.SelectedProviderName == "Provider A")), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CheckAnswersRouteGet);
        }

        [Test]
        public async Task NeedMoreInformationContinue_Clears_Session_And_Redirects_To_KnowYourUln()
        {
            // Arrange
            _sessionServiceMock.Setup(s => s.ClearAuthorisationAnswersAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.NeedMoreInformationContinue();

            // Assert
            _sessionServiceMock.Verify(s => s.ClearAuthorisationAnswersAsync(), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.KnowYourUlnRouteGet);
        }

        [Test]
        public async Task CheckAnswers_Get_ModelNull_Redirects_To_NeedMoreInformation()
        {
            // Arrange
            _orchestratorMock.Setup(o => o.GetCheckAnswersViewModelAsync()).ReturnsAsync((CheckAnswersViewModel)null);

            // Act
            var result = await _sut.CheckAnswers();

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.NeedMoreInformationRouteGet);
        }

        [Test]
        public async Task CheckAnswers_Get_Sets_BackLink_Based_On_IsShortJourney()
        {
            // Arrange
            var vm = new CheckAnswersViewModel { IsShortJourney = true };
            _orchestratorMock.Setup(o => o.GetCheckAnswersViewModelAsync()).ReturnsAsync(vm);

            // Act
            var result = await _sut.CheckAnswers();

            // Assert
            result.Should().BeOfType<ViewResult>();
            var view = result as ViewResult;
            var model = view.Model as CheckAnswersViewModel;
            model.BackLinkRouteName.Should().Be(AuthoriseController.SelectCourseRouteGet);

            vm.IsShortJourney = false;
            _orchestratorMock.Setup(o => o.GetCheckAnswersViewModelAsync()).ReturnsAsync(vm);
            result = await _sut.CheckAnswers();
            view = result as ViewResult;
            model = view.Model as CheckAnswersViewModel;
            model.BackLinkRouteName.Should().Be(AuthoriseController.SelectProviderRouteGet);
        }

        [Test]
        public async Task CheckAnswersPost_SingleMatch_Redirects_To_Certificates()
        {
            // Arrange
            _orchestratorMock.Setup(o => o.SubmitCheckAnswersAsync()).ReturnsAsync(MatchOutcome.SingleMatch);

            // Act
            var result = await _sut.CheckAnswersPost();

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(CertificatesController.CertificatesListRouteGet);
        }

        [Test]
        public async Task CheckAnswersPost_Locked_Clears_Session_And_Redirects_To_CannotMatch()
        {
            // Arrange
            _orchestratorMock.Setup(o => o.SubmitCheckAnswersAsync()).ReturnsAsync(MatchOutcome.Locked);
            _sessionServiceMock.Setup(s => s.ClearAuthorisationAnswersAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.CheckAnswersPost();

            // Assert
            _sessionServiceMock.Verify(s => s.ClearAuthorisationAnswersAsync(), Times.Once);
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CannotMatchRouteGet);
        }

        [Test]
        public async Task CheckAnswersPost_NoMatch_Adds_Flash_To_TempData_And_Redirects_To_CheckAnswers()
        {
            // Arrange
            _orchestratorMock.Setup(o => o.SubmitCheckAnswersAsync()).ReturnsAsync(MatchOutcome.NoMatch);

            var tempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(new Microsoft.AspNetCore.Http.DefaultHttpContext(), Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
            _sut.TempData = tempData;

            // Act
            var result = await _sut.CheckAnswersPost();

            // Assert
            result.Should().BeOfType<RedirectToRouteResult>();
            var redirect = result as RedirectToRouteResult;
            redirect.RouteName.Should().Be(AuthoriseController.CheckAnswersRouteGet);
            _sut.TempData.ContainsKey(TempDataDictionaryExtensions.FlashMessageBodyTempDataKey).Should().BeTrue();
            _sut.TempData.ContainsKey(TempDataDictionaryExtensions.FlashMessageTempDetailKey).Should().BeTrue();
            _sut.TempData.ContainsKey(TempDataDictionaryExtensions.FlashMessageLevelTempDataKey).Should().BeTrue();
        }
    }
}
