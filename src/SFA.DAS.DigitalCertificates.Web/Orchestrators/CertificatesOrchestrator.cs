using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Web.Models.Certificates;
using SFA.DAS.DigitalCertificates.Web.Services.SessionStorage;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class CertificatesOrchestrator : BaseOrchestrator, ICertificatesOrchestrator
    {
        private readonly ISessionStorageService _sessionStorageService;

        public CertificatesOrchestrator(IMediator mediator, ISessionStorageService sessionStorageService)
            : base(mediator)
        {
            _sessionStorageService = sessionStorageService;
        }

        public async Task<CertificatesListViewModel> GetCertificatesListViewModel()
        {
            return new CertificatesListViewModel
            {
                Certificates = await _sessionStorageService.GetOwnedCertificatesAsync()
            };
        }
    }
}
