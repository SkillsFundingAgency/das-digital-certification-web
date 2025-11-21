using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificatesOrchestrator : BaseOrchestrator, ICertificatesOrchestrator
    {
        private readonly ISessionStorageService _sessionStorageService;
        private readonly IUserService _userService;

        public CertificatesOrchestrator(IMediator mediator, ISessionStorageService sessionStorageService, IUserService userService)
            : base(mediator)
        {
            _sessionStorageService = sessionStorageService;
            _userService = userService;
        }

        public async Task<CertificatesListViewModel> GetCertificatesListViewModel()
        {
            return new CertificatesListViewModel
            {
                Certificates = await _sessionStorageService.GetOwnedCertificatesAsync(_userService.GetGovUkIdentifier())
            };
        }
    }
}
