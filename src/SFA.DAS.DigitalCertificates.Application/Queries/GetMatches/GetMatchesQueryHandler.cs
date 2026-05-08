using MediatR;
using SFA.DAS.DigitalCertificates.Domain.Interfaces;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetMatches
{
    public class GetMatchesQueryHandler : IRequestHandler<GetMatchesQuery, MatchesAndMasks?>
    {
        private readonly IDigitalCertificatesOuterApi _outerApi;

        public GetMatchesQueryHandler(IDigitalCertificatesOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<MatchesAndMasks?> Handle(GetMatchesQuery request, CancellationToken cancellationToken)
        {
            var resp = await _outerApi.GetMatches(request.UserId);
            if (resp == null) return null;

            var result = new MatchesAndMasks();

            foreach (var m in resp.Matches)
            {
                var ct = Enum.TryParse<CertificateType>(m.CertificateType, out var parsed) ? parsed : CertificateType.Unknown;

                result.Matches.Add(new Match
                {
                    Uln = m.Uln,
                    CertificateType = ct,
                    CourseCode = m.CourseCode,
                    CourseName = m.CourseName,
                    CourseLevel = m.CourseLevel,
                    DateAwarded = m.DateAwarded,
                    ProviderName = m.ProviderName,
                    Ukprn = m.Ukprn
                });
            }

            foreach (var mk in resp.Masks)
            {
                var ct = Enum.TryParse<CertificateType>(mk.CertificateType, out var parsed) ? parsed : CertificateType.Unknown;

                result.Masks.Add(new Mask
                {
                    CertificateType = ct,
                    CourseCode = mk.CourseCode,
                    CourseName = mk.CourseName,
                    CourseLevel = mk.CourseLevel,
                    ProviderName = mk.ProviderName
                });
            }

            return result;
        }
    }
}
