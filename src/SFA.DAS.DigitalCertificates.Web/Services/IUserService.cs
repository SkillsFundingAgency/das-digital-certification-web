using System;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface IUserService
    {
        string GetGovUkIdentifier();
        Guid? GetUserId();
    }
}