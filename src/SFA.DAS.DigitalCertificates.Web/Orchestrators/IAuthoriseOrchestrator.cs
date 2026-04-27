using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;
using SFA.DAS.DigitalCertificates.Web.Enums;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface IAuthoriseOrchestrator
    {
        Task PrepareNeedMoreInformationAsync();
        Task<CourseMatchOutcome> GetCourseMatchOutcomeAsync(SelectCourseViewModel viewModel);
        Task<bool> ValidateKnowYourUlnViewModel(KnowYourUlnViewModel viewModel, ModelStateDictionary modelState);
        Task<KnowYourUlnViewModel?> GetKnowYourUlnViewModelAsync();
        Task SaveKnowYourUlnAsync(KnowYourUlnViewModel viewModel);
        Task<bool> ValidateKnowYearViewModel(KnowYearViewModel viewModel, ModelStateDictionary modelState);
        Task<KnowYearViewModel?> GetKnowYearViewModelAsync();
        Task SaveKnowYearAsync(KnowYearViewModel viewModel);
        
        Task<bool> ValidateSelectCourseViewModel(SelectCourseViewModel viewModel, ModelStateDictionary modelState);
        Task<SelectCourseViewModel?> GetSelectCourseViewModelAsync();
        Task SaveSelectedCourseAsync(SelectCourseViewModel viewModel);
        Task<bool> ValidateSelectProviderViewModel(SelectProviderViewModel viewModel, ModelStateDictionary modelState);
        Task<SelectProviderViewModel?> GetSelectProviderViewModelAsync();
        Task SaveSelectedProviderAsync(SelectProviderViewModel viewModel);
    }
}
