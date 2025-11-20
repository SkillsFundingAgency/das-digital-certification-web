using SFA.DAS.DigitalCertificates.Web.Models.User;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public interface IHomeOrchestrator
    {
        Task<Guid> CreateOrUpdateUser(CreateOrUpdateUserModel model);
    }
}