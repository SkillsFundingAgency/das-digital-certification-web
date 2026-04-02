using System.IO;
using Aspose.Pdf;

namespace SFA.DAS.DigitalCertificates.Web.Services
{
    public class AsposeLicenseWrapper : IAsposeLicenseWrapper
    {
        public void SetLicense(Stream stream)
        {
            var license = new License();
            license.SetLicense(stream);
        }
    }
}