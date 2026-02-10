using SFA.DAS.DigitalCertificates.Domain.Models;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetSharings
{
    public class GetSharingsQueryResult
    {
        public Guid UserId { get; set; }
        public Guid CertificateId { get; set; }
        public CertificateType CertificateType { get; set; }
        public required string CourseName { get; set; }
        public List<Sharing>? Sharings { get; set; }

        public static implicit operator GetSharingsQueryResult?(GetSharingsResponse? source)
        {
            if (source is null)
            {
                return null;
            }

            return new GetSharingsQueryResult
            {
                UserId = source.UserId,
                CertificateId = source.CertificateId,
                CertificateType = Enum.Parse<CertificateType>(source.CertificateType),
                CourseName = source.CourseName,
                Sharings = source.Sharings != null
                    ? source.Sharings
                        .Where(s => s is not null)
                        .Select(s => (Sharing)s!)
                        .ToList()
                    : null
            };
        }
    }
}
