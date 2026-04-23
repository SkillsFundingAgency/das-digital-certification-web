using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Web.Controllers
{
    [Route("authorise")]
    public class AuthoriseController : BaseController
    {
        #region Routes
        public const string NeedMoreInformationRouteGet = nameof(NeedMoreInformationRouteGet);
        public const string NeedMoreInformationContinueRoutePost = nameof(NeedMoreInformationContinueRoutePost);
        public const string KnowYourUlnRouteGet = nameof(KnowYourUlnRouteGet);
        public const string KnowYourUlnRoutePost = nameof(KnowYourUlnRoutePost);
        public const string SelectCourseRouteGet = nameof(SelectCourseRouteGet);
        public const string SelectCourseRoutePost = nameof(SelectCourseRoutePost);
        #endregion

        private readonly ISessionService _sessionService;
        private readonly IAuthoriseOrchestrator _authoriseOrchestrator;

        public AuthoriseController(IHttpContextAccessor httpContextAccessor, ISessionService sessionService, IAuthoriseOrchestrator authoriseOrchestrator)
            : base(httpContextAccessor)
        {
            _sessionService = sessionService;
            _authoriseOrchestrator = authoriseOrchestrator;
        }

        [HttpGet("need-more-information", Name = NeedMoreInformationRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> NeedMoreInformation()
        {
            await _authoriseOrchestrator.PrepareNeedMoreInformationAsync();

            return View();
        }

        [HttpPost("need-more-information", Name = NeedMoreInformationContinueRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> NeedMoreInformationContinue()
        {
            await _sessionService.ClearAuthorisationAnswersAsync();

            return RedirectToRoute(KnowYourUlnRouteGet);
        }

        [HttpGet("know-your-uln", Name = KnowYourUlnRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYourUln()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            var model = new KnowYourUlnViewModel
            {
                KnowUln = answers?.KnowUln,
                Uln = answers?.Uln
            };

            return View(model);
        }

        [HttpPost("know-your-uln", Name = KnowYourUlnRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYourUln(KnowYourUlnViewModel model)
        {
            if (!await _authoriseOrchestrator.ValidateKnowYourUlnViewModel(model, ModelState))
            {
                return RedirectToRoute(KnowYourUlnRouteGet);
            }

            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            answers.KnowUln = model.KnowUln;

            answers.Uln = model.KnowUln == true ? model.Uln : null;

            await _sessionService.SetAuthorisationAnswersAsync(answers);
            return RedirectToRoute(SelectCourseRouteGet);
        }

        [HttpGet("select-course", Name = SelectCourseRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectCourse()
        {
            var model = await _authoriseOrchestrator.GetSelectCourseViewModelAsync();
            return View(model);
        }

        [HttpPost("select-course", Name = SelectCourseRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectCourse(Models.Authorise.SelectCourseViewModel model)
        {
            if (!await _authoriseOrchestrator.ValidateSelectCourseViewModel(model, ModelState))
            {
                return RedirectToRoute(SelectCourseRouteGet);
            }
            await _authoriseOrchestrator.SaveSelectedCourseAsync(model);

            //ToDo: Next page not implemented yet - stay on the page per AC2
            return View(model);
        }
    }
}
