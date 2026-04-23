using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface IAuthoriseOrchestrator
    {
        Task PrepareNeedMoreInformationAsync();
        Task<bool> ValidateKnowYourUlnViewModel(KnowYourUlnViewModel viewModel, ModelStateDictionary modelState);
        Task<KnowYourUlnViewModel?> GetKnowYourUlnViewModelAsync();
        Task SaveKnowYourUlnAsync(KnowYourUlnViewModel viewModel);
        Task<bool> ValidateSelectCourseViewModel(SelectCourseViewModel viewModel, ModelStateDictionary modelState);
        Task<SelectCourseViewModel?> GetSelectCourseViewModelAsync();
        Task SaveSelectedCourseAsync(SelectCourseViewModel viewModel);
    }
}
