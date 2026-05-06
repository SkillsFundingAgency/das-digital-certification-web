using System.Collections.Generic;

namespace SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses
{
    public class GetUserActionsResponse
    {
        public List<UserActionResponse> UserActions { get; set; } = new List<UserActionResponse>();
    }
}
