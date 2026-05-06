using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Enums;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface IAuthoriseOrchestrator
    {
        Task<bool> PrepareNeedMoreInformationAsync();
        Task<bool> ValidateKnowYourUlnViewModel(KnowYourUlnViewModel viewModel, ModelStateDictionary modelState);
        Task<KnowYourUlnViewModel?> GetKnowYourUlnViewModelAsync();
        Task<KnowYourUlnViewModel> SaveKnowYourUlnAsync(KnowYourUlnViewModel viewModel);
        Task<bool> ValidateKnowYearViewModel(KnowYearViewModel viewModel, ModelStateDictionary modelState);
        Task<KnowYearViewModel?> GetKnowYearViewModelAsync();
        Task<KnowYearViewModel> SaveKnowYearAsync(KnowYearViewModel viewModel);
        
        Task<bool> ValidateSelectCourseViewModel(SelectCourseViewModel viewModel, ModelStateDictionary modelState);
        Task<SelectCourseViewModel?> GetSelectCourseViewModelAsync();
        Task<SelectCourseViewModel> SaveSelectedCourseAsync(SelectCourseViewModel viewModel);
        Task<bool> ValidateSelectProviderViewModel(SelectProviderViewModel viewModel, ModelStateDictionary modelState);
        Task<SelectProviderViewModel?> GetSelectProviderViewModelAsync();
        Task<SelectProviderViewModel> SaveSelectedProviderAsync(SelectProviderViewModel viewModel);
        Task<CheckAnswersViewModel?> GetCheckAnswersViewModelAsync();
        Task<MatchOutcome> GetUlnMatchOutcomeAsync(KnowYourUlnViewModel viewModel);
        Task<MatchOutcome> GetCourseMatchOutcomeAsync(SelectCourseViewModel viewModel);
        Task<MatchOutcome> SubmitCheckAnswersAsync();
    }
}
