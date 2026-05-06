using MediatR;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Requests;

namespace SFA.DAS.DigitalCertificates.Application.Commands.AuthoriseUser
{
    public class AuthoriseUserCommand : IRequest<Unit>
    {
        public required Guid UserId { get; set; }

        public required long Uln { get; set; }

        public static implicit operator AuthoriseUserRequest(AuthoriseUserCommand c)
        {
            return new AuthoriseUserRequest
            {
                Uln = c.Uln
            };
        }
    }
}
