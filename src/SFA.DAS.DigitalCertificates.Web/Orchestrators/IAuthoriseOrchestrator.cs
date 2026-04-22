using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface IAuthoriseOrchestrator
    {
        Task PrepareNeedMoreInformationAsync();
        Task<bool> ValidateKnowYourUlnViewModel(Models.Authorise.KnowYourUlnViewModel viewModel, Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState);
    }
}
