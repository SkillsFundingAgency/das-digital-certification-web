using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Web.Models.Authorise;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface IAuthoriseOrchestrator
    {
        Task PrepareNeedMoreInformationAsync();
        Task<bool> ValidateKnowYourUlnViewModel(KnowYourUlnViewModel viewModel, ModelStateDictionary modelState);
        Task<bool> ValidateSelectCourseViewModel(SelectCourseViewModel viewModel, ModelStateDictionary modelState);
        Task<SelectCourseViewModel?> GetSelectCourseViewModelAsync();
        Task SaveSelectedCourseAsync(SelectCourseViewModel viewModel);
    }
}
