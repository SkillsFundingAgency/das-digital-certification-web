using System;

namespace SFA.DAS.DigitalCertificates.Web.Services.User
{
    public interface IUserService
    {
        string GetGovUkIdentifier();
        Guid? GetUserId();
    }
}