using System.Threading.Tasks;
using MediatR;
using SFA.DAS.DigitalCertificates.Web.Services;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class AuthoriseOrchestrator : BaseOrchestrator, IAuthoriseOrchestrator
    {
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;

        public AuthoriseOrchestrator(IMediator mediator, IUserService userService, ICacheService cacheService)
            : base(mediator)
        {
            _userService = userService;
            _cacheService = cacheService;
        }

        public async Task PrepareNeedMoreInformationAsync()
        {
            var govUkId = _userService.GetGovUkIdentifier();
            if (string.IsNullOrWhiteSpace(govUkId)) return;

            var userId = _userService.GetUserId();
            if (userId == null) return;

            //TODO: The “no matches” response scenario needs to be handled. This may be addressed as part of upcoming tickets.
            var matches = await _cacheService.GetOrCreateMatchesAsync(govUkId, userId.Value);
        }
    }
}
