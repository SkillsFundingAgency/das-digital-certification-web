using System;
using System.Threading.Tasks;
using SFA.DAS.DigitalCertificates.Web.Models.Home;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface IHomeOrchestrator
    {
        Task<Guid> CreateOrUpdateUser(CreateOrUpdateUserModel model);
    }
}