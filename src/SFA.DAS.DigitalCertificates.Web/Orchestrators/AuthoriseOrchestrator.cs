using System.Threading.Tasks;
using System.Linq;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Services;
using SFA.DAS.DigitalCertificates.Domain.Models;
using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class AuthoriseOrchestrator : BaseOrchestrator, IAuthoriseOrchestrator
    {
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly ISessionService _sessionService;
        private readonly IValidator<KnowYourUlnViewModel> _knowUlnValidator;
        private readonly IValidator<SelectCourseViewModel> _selectCourseValidator;

        public AuthoriseOrchestrator(IMediator mediator, ISessionService sessionService, IUserService userService, ICacheService cacheService,
            IValidator<KnowYourUlnViewModel> knowUlnValidator,
            IValidator<SelectCourseViewModel> selectCourseValidator)
            : base(mediator)
        {
            _sessionService = sessionService;
            _userService = userService;
            _cacheService = cacheService;
            _knowUlnValidator = knowUlnValidator;
            _selectCourseValidator = selectCourseValidator;
        }

        public async Task PrepareNeedMoreInformationAsync()
        {
            var govUkId = _userService.GetGovUkIdentifier();
            if (string.IsNullOrWhiteSpace(govUkId)) return;

            var userId = _userService.GetUserId();
            if (userId == null) return;

            //TODO: The “no matches” response scenario needs to be handled. This may be addressed as part of upcoming tickets.
            var matches = await _cacheService.GetOrCreateMatchesAsync(govUkId, userId.Value);
        }

        public async Task<bool> ValidateKnowYourUlnViewModel(KnowYourUlnViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_knowUlnValidator, viewModel, modelState);
        }

        public async Task<KnowYourUlnViewModel?> GetKnowYourUlnViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            if (answers == null) return new KnowYourUlnViewModel();

            return new KnowYourUlnViewModel
            {
                KnowUln = answers.KnowUln,
                Uln = answers.Uln
            };
        }

        public async Task SaveKnowYourUlnAsync(KnowYourUlnViewModel viewModel)
        {
            if (viewModel == null) return;

            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            answers.KnowUln = viewModel.KnowUln;
            answers.Uln = viewModel.KnowUln == true ? viewModel.Uln : null;

            await _sessionService.SetAuthorisationAnswersAsync(answers);
        }

        private async Task<MatchesAndMasks?> GetMatchesAsync()
        {
            var govUkId = _userService.GetGovUkIdentifier();
            if (string.IsNullOrWhiteSpace(govUkId)) return null;

            var userId = _userService.GetUserId();
            if (userId == null) return null;

            var matches = await _cacheService.GetOrCreateMatchesAsync(govUkId, userId.Value);
            return matches;
        }

        public async Task<bool> ValidateSelectCourseViewModel(SelectCourseViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_selectCourseValidator, viewModel, modelState);
        }

        public async Task<SelectCourseViewModel?> GetSelectCourseViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            var matches = await GetMatchesAsync();
            var courseOptions = MapMatchesToCourseOptions(matches);

            var model = new SelectCourseViewModel
            {
                SelectedCourseCode = answers?.CourseCode,
                Courses = courseOptions
            };

            return model;
        }

        public async Task SaveSelectedCourseAsync(SelectCourseViewModel viewModel)
        {
            if (viewModel == null || string.IsNullOrWhiteSpace(viewModel.SelectedCourseCode))
            {
                return;
            }

            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            answers.CourseCode = viewModel.SelectedCourseCode?.Trim();

            var matches = await GetMatchesAsync();
            var courseOptions = MapMatchesToCourseOptions(matches);

            var selected = courseOptions.FirstOrDefault(c => c.CourseCode == answers.CourseCode);
            answers.CourseName = selected?.CourseName;

            await _sessionService.SetAuthorisationAnswersAsync(answers);
        }

        private static List<SelectCourseViewModel.CourseOption> MapMatchesToCourseOptions(MatchesAndMasks? matches)
        {
            var courseOptions = new List<SelectCourseViewModel.CourseOption>();
            if (matches == null) return courseOptions;

            if (matches.Masks != null)
            {
                courseOptions.AddRange(matches.Masks.Select(m => new SelectCourseViewModel.CourseOption
                {
                    CourseCode = m.CourseCode,
                    CourseName = m.CourseName,
                    CourseLevel = m.CourseLevel,
                    CertificateType = m.CertificateType
                }));
            }

            if (matches.Matches != null)
            {
                courseOptions.AddRange(matches.Matches.Select(m => new SelectCourseViewModel.CourseOption
                {
                    CourseCode = m.CourseCode,
                    CourseName = m.CourseName,
                    CourseLevel = m.CourseLevel,
                    CertificateType = m.CertificateType
                }));
            }

            return courseOptions;
        }
    }
}
