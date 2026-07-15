using MediatR;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class HomeOrchestrator : BaseOrchestrator, IHomeOrchestrator
    {
        public HomeOrchestrator(IMediator mediator, IHttpContextAccessor httpContextAccessor)
            : base(mediator, httpContextAccessor) { }
    }
}
