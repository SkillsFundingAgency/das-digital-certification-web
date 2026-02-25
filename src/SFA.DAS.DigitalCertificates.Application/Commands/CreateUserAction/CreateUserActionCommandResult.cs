using SFA.DAS.DigitalCertificates.Infrastructure.Api.Responses;

namespace SFA.DAS.DigitalCertificates.Application.Commands.CreateUserAction
{
    public class CreateUserActionCommandResult
    {
        public required string ActionCode { get; set; }

        public static implicit operator CreateUserActionCommandResult?(CreateUserActionResponse? source)
        {
            if (source == null)
                return null;

            return new CreateUserActionCommandResult
            {
                ActionCode = source.ActionCode
            };
        }
    }
}
