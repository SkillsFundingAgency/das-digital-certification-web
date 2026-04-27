using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Web.Orchestrators;
using SFA.DAS.DigitalCertificates.Web.Enums;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Authentication;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

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
        public const string SelectProviderRouteGet = nameof(SelectProviderRouteGet);
        public const string SelectProviderRoutePost = nameof(SelectProviderRoutePost);
        public const string CheckAnswersRouteGet = nameof(CheckAnswersRouteGet);
        public const string KnowYearRouteGet = nameof(KnowYearRouteGet);
        public const string KnowYearRoutePost = nameof(KnowYearRoutePost);
        public const string CannotMatchRouteGet = nameof(CannotMatchRouteGet);
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
            var model = await _authoriseOrchestrator.GetKnowYourUlnViewModelAsync();
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
            await _authoriseOrchestrator.SaveKnowYourUlnAsync(model);
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

            var matchOutcome = await _authoriseOrchestrator.GetCourseMatchOutcomeAsync(model);

            if (matchOutcome == CourseMatchOutcome.NoData)
            {
                return RedirectToRoute(CannotMatchRouteGet);
            }

            if (matchOutcome == CourseMatchOutcome.NoMatch)
            {
                return RedirectToRoute(KnowYearRouteGet);
            }

            if (matchOutcome == CourseMatchOutcome.MultipleMatches)
            {
                return RedirectToRoute(KnowYearRouteGet);
            }

            return RedirectToRoute(CheckAnswersRouteGet);
        }

        [HttpGet("select-provider", Name = SelectProviderRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectProvider()
        {
            var model = await _authoriseOrchestrator.GetSelectProviderViewModelAsync();
            return View(model);
        }

        [HttpPost("select-provider", Name = SelectProviderRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> SelectProvider(SelectProviderViewModel model)
        {
            if (string.Equals(model.SelectedProviderName?.Trim(), SelectProviderViewModel.UnknownProviderSentinel, System.StringComparison.OrdinalIgnoreCase))
            {
                model.SelectedProviderUnknown = true;
                model.SelectedProviderName = null;
            }

            if (!await _authoriseOrchestrator.ValidateSelectProviderViewModel(model, ModelState))
            {
                return RedirectToRoute(SelectProviderRouteGet);
            }

            await _authoriseOrchestrator.SaveSelectedProviderAsync(model);

            // Page not implemented - refresh page per AC2
            return View(model);
        }

        [HttpGet("check-answers", Name = CheckAnswersRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> CheckAnswers()
        {
            var model = await _authoriseOrchestrator.GetSelectCourseViewModelAsync();
            return View(model);
        }

        [HttpGet("cannot-match", Name = CannotMatchRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public IActionResult CannotMatch()
        {
            return View();
        }

        [HttpGet("know-year", Name = KnowYearRouteGet)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYear()
        {
            var model = await _authoriseOrchestrator.GetKnowYearViewModelAsync();
            return View(model);
        }

        [HttpPost("know-year", Name = KnowYearRoutePost)]
        [Authorize(Policy = nameof(DigitalCertificatesPolicyNames.VerifiedAndNotUlnAuthorised))]
        public async Task<IActionResult> KnowYear(KnowYearViewModel model)
        {
            if (!await _authoriseOrchestrator.ValidateKnowYearViewModel(model, ModelState))
            {
                return RedirectToRoute(KnowYearRouteGet);
            }

            await _authoriseOrchestrator.SaveKnowYearAsync(model);

            // Per AC1: after saving year, proceed to provider selection
            return RedirectToRoute(SelectProviderRouteGet);
        }
    }
}
