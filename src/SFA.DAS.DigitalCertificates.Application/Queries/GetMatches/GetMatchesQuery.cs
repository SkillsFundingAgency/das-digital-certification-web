using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetMatches
{
    public class GetMatchesQuery : IRequest<MatchesAndMasks?>
    {
        public Guid UserId { get; set; }
    }
}
