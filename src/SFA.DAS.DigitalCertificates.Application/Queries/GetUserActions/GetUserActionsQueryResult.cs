using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;
using SFA.DAS.DigitalCertificates.Domain.Models;

namespace SFA.DAS.DigitalCertificates.Application.Queries.GetUserActions
{
    public class GetUserActionsQueryResult
    {
        public List<UserActionDetail>? UserActions { get; set; }

        public static implicit operator GetUserActionsQueryResult?(GetUserActionsResponse? source)
        {
            if (source == null) return null;

            return new GetUserActionsQueryResult
            {
                UserActions = source?.UserActions?.Select(a => new SFA.DAS.DigitalCertificates.Domain.Models.UserActionDetail
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    ActionType = Enum.Parse<ActionType>(a.ActionType),
                    ActionTime = a.ActionTime,
                    ActionStatus = Enum.Parse<UserActionStatus>(a.ActionStatus),
                    FamilyName = a.FamilyName,
                    GivenNames = a.GivenNames,
                    CertificateId = a.CertificateId,
                    CertificateType = !string.IsNullOrWhiteSpace(a.CertificateType) ? Enum.Parse<CertificateType>(a.CertificateType) as CertificateType? : null,
                    CourseName = a.CourseName ?? string.Empty,
                    ActionCode = a.ActionCode,
                    AdminActions = a.AdminActions?.Select(ad => new SFA.DAS.DigitalCertificates.Domain.Models.AdminActionDetail
                    {
                        Username = ad.Username,
                        ActionTime = ad.ActionTime,
                        Action = Enum.Parse<AdminActionType>(ad.Action)
                    }).ToList() ?? new List<SFA.DAS.DigitalCertificates.Domain.Models.AdminActionDetail>()
                }).ToList()
            };
        }
    }
}
