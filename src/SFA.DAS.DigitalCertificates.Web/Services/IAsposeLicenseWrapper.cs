using System.IO;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public interface IAsposeLicenseWrapper
    {
        void SetLicense(Stream stream);
    }
}
