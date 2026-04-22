using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using NUnit.Framework;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Controllers;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Services;

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
            var answers = new AuthorisationAnswers
            {
                KnowUln = true,
                Uln = 1234567890L
            };

            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync(answers);

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
            _sessionServiceMock.Setup(s => s.GetAuthorisationAnswersAsync()).ReturnsAsync((AuthorisationAnswers)null);

            // Act
            var result = await _sut.KnowYourUln(vm);

            // Assert
            _sessionServiceMock.Verify(s => s.SetAuthorisationAnswersAsync(It.Is<AuthorisationAnswers>(a => a.KnowUln == true && a.Uln == 1234567890L)), Times.Once);
            result.Should().BeOfType<ViewResult>();
        }
    }
}
