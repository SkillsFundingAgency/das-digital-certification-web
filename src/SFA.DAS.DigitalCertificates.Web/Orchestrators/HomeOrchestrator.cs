using MediatR;
using SFA.DAS.DigitalCertificates.Application.Commands.CreateOrUpdateUser;
using SFA.DAS.DigitalCertificates.Infrastructure.Api.Types;
using SFA.DAS.DigitalCertificates.Web.Models.User;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.DigitalCertificates.Web.Orchestrators
{
    public class HomeOrchestrator : BaseOrchestrator, IHomeOrchestrator
    {
        public HomeOrchestrator(IMediator mediator)
            : base(mediator) { }

        public async Task<Guid> CreateOrUpdateUser(CreateOrUpdateUserModel model)
        {
            var userId = await Mediator.Send(new CreateOrUpdateUserCommand
            {
                GovUkIdentifier = model.GovUkIdentifier,
                EmailAddress = model.EmailAddress,
                PhoneNumber = model.PhoneNumber,
                Names = model.Names
                    .Select(x => new Name
                    {
                        ValidSince = x.ValidSince,
                        ValidUntil = x.ValidUntil,
                        FamilyName = x.FamilyName,
                        GivenNames = x.GivenNames
                    })
                    .ToList(),
                DateOfBirth = model.DateOfBirth
            });

            return userId;
        }
    }
}
