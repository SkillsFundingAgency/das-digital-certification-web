using MediatR;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class BaseOrchestrator
    {
        private readonly IMediator _mediator;

        public BaseOrchestrator(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IMediator Mediator => _mediator;
    }
}
