using System.Threading.Tasks;
using System.Linq;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Enums;
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
        private readonly IValidator<KnowYearViewModel> _knowYearValidator;
        private readonly IValidator<SelectCourseViewModel> _selectCourseValidator;
        private readonly IValidator<SelectProviderViewModel> _selectProviderValidator;

        public AuthoriseOrchestrator(IMediator mediator, ISessionService sessionService, IUserService userService, ICacheService cacheService,
                IValidator<KnowYourUlnViewModel> knowUlnValidator,
                IValidator<KnowYearViewModel> knowYearValidator,
                IValidator<SelectCourseViewModel> selectCourseValidator,
                IValidator<SelectProviderViewModel> selectProviderValidator)
            : base(mediator)
        {
            _sessionService = sessionService;
            _userService = userService;
            _cacheService = cacheService;
            _knowUlnValidator = knowUlnValidator;
            _knowYearValidator = knowYearValidator;
            _selectCourseValidator = selectCourseValidator;
            _selectProviderValidator = selectProviderValidator;
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

        public async Task<bool> ValidateKnowYearViewModel(KnowYearViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_knowYearValidator, viewModel, modelState);
        }

        public async Task<KnowYearViewModel?> GetKnowYearViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            if (answers == null) return new KnowYearViewModel();

            return new KnowYearViewModel
            {
                KnowYear = answers.KnowYear,
                YearCompleted = answers.YearCompleted
            };
        }

        public async Task SaveKnowYearAsync(KnowYearViewModel viewModel)
        {
            if (viewModel == null) return;

            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            answers.KnowYear = viewModel.KnowYear;
            answers.YearCompleted = viewModel.KnowYear == true ? viewModel.YearCompleted : null;

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
            // TODO: This needs to be refined in the upcoming ticket
        public async Task<bool> ValidateSelectProviderViewModel(SelectProviderViewModel viewModel, ModelStateDictionary modelState)
        {
            return await ValidateViewModel(_selectProviderValidator, viewModel, modelState);
        }

        public async Task<SelectProviderViewModel?> GetSelectProviderViewModelAsync()
        {
            var answers = await _sessionService.GetAuthorisationAnswersAsync();
            var matches = await GetMatchesAsync();
            var providerOptions = MapMatchesToProviderOptions(matches);

            var model = new SelectProviderViewModel
            {
                SelectedProviderName = answers?.ProviderName,
                SelectedProviderUnknown = answers?.ProviderUnknown,
                Providers = providerOptions
            };

            return model;
        }

        public async Task SaveSelectedProviderAsync(SelectProviderViewModel viewModel)
        {
            if (viewModel == null) return;

            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();

            if (viewModel.SelectedProviderUnknown == true)
            {
                answers.ProviderUnknown = true;
                answers.ProviderUkprn = null;
                answers.ProviderName = null;
                await _sessionService.SetAuthorisationAnswersAsync(answers);
                return;
            }

            var selectedName = viewModel.SelectedProviderName?.Trim();

            var matches = await GetMatchesAsync();
            var providerOptions = MapMatchesToProviderOptions(matches);

            var selected = providerOptions.FirstOrDefault(p =>
                !string.IsNullOrWhiteSpace(p.ProviderName) && string.Equals(p.ProviderName?.Trim(), selectedName, System.StringComparison.OrdinalIgnoreCase)
            );

            if (selected != null)
            {
                answers.ProviderUkprn = string.IsNullOrWhiteSpace(selected.Ukprn) ? null : selected.Ukprn?.Trim();
                answers.ProviderName = selected.ProviderName?.Trim();
                answers.ProviderUnknown = false;
            }
            else
            {
                answers.ProviderUkprn = null;
                answers.ProviderName = null;
                answers.ProviderUnknown = false;
            }

            await _sessionService.SetAuthorisationAnswersAsync(answers);
        }

        private static List<SelectProviderViewModel.ProviderOption> MapMatchesToProviderOptions(MatchesAndMasks? matches)
        {
            var providerOptions = new List<SelectProviderViewModel.ProviderOption>();
            if (matches == null) return providerOptions;

            if (matches.Matches != null)
            {
                providerOptions.AddRange(matches.Matches
                    .Where(m => !string.IsNullOrWhiteSpace(m.ProviderName))
                    .Select(m => new SelectProviderViewModel.ProviderOption
                    {
                        Ukprn = m.Ukprn,
                        ProviderName = m.ProviderName
                    }));
            }

            if (matches.Masks != null)
            {
                providerOptions.AddRange(matches.Masks
                    .Where(m => !string.IsNullOrWhiteSpace(m.ProviderName))
                    .Select(m => new SelectProviderViewModel.ProviderOption
                    {
                        Ukprn = null,
                        ProviderName = m.ProviderName
                    }));
            }

            var distinct = providerOptions
                .GroupBy(p => (p.ProviderName ?? string.Empty).Trim())
                .Select(g => g.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Ukprn)) ?? g.First())
                .OrderBy(p => p.ProviderName)
                .ToList();

            return distinct;
        }
        public async Task<CourseMatchOutcome> GetCourseMatchOutcomeAsync(SelectCourseViewModel viewModel)
        {
            if (viewModel == null) return CourseMatchOutcome.NoData;

            var selected = viewModel.SelectedCourseCode?.Trim();
            if (string.IsNullOrWhiteSpace(selected)) return CourseMatchOutcome.NoMatch;

            var matches = await GetMatchesAsync();
            if (matches == null) return CourseMatchOutcome.NoData;

            var matchCount = matches.Matches?.Count(m => string.Equals(m.CourseCode?.Trim(), selected, System.StringComparison.OrdinalIgnoreCase)) ?? 0;
            var maskCount = matches.Masks?.Count(m => string.Equals(m.CourseCode?.Trim(), selected, System.StringComparison.OrdinalIgnoreCase)) ?? 0;

            // More than one real match, or a single real match with masks -> There is no such real scenario
            if (matchCount > 1 || (matchCount == 1 && maskCount > 0))
            {
                return CourseMatchOutcome.MultipleMatches;
            }

            // Single exact match (no masks)
            if (matchCount == 1) return CourseMatchOutcome.SingleMatch;

            // No real matches (masks only or none) -> record failure and route to KnowYear
            var answers = await _sessionService.GetAuthorisationAnswersAsync() ?? new AuthorisationAnswers();
            answers.FailedMatchCount = answers.FailedMatchCount + 1;
            await _sessionService.SetAuthorisationAnswersAsync(answers);

            return CourseMatchOutcome.NoMatch;
        }
    }
}
